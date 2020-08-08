﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using GenericTensor.Functions;

namespace GenericTensor.Core.Expressions
{
    public static class ExpressionCompiler<T, TWrapper> where TWrapper : struct, IOperations<T>
    {
        public static Func<Expression, Expression, Expression> Addition
            = (a, b) =>  Expression.Call(
                Expression.Default(typeof(TWrapper)),
                typeof(IOperations<T>).GetMethod(nameof(IOperations<T>.Add)),
                a, b
            );

        public static Func<Expression, Expression, Expression> Subtraction
            = (a, b) =>  Expression.Call(
                Expression.Default(typeof(TWrapper)),
                typeof(IOperations<T>).GetMethod(nameof(IOperations<T>.Subtract)),
                a, b
            );

        public static Func<Expression, Expression, Expression> Multiplication
            = (a, b) =>  Expression.Call(
                Expression.Default(typeof(TWrapper)),
                typeof(IOperations<T>).GetMethod(nameof(IOperations<T>.Multiply)),
                a, b
            );

        public static Func<Expression, Expression, Expression> Division
            = (a, b) =>  Expression.Call(
                Expression.Default(typeof(TWrapper)),
                typeof(IOperations<T>).GetMethod(nameof(IOperations<T>.Divide)),
                a, b
            );

        public static Expression CreateLoop(ParameterExpression var, Expression until, Expression onIter)
        {
            var label = Expression.Label();

            var loop = Expression.Loop(
                Expression.IfThenElse(

                    Expression.LessThan(var, until),

                    Expression.Block(
                        onIter,
                        Expression.PostIncrementAssign(var)
                    ),

                    Expression.Break(label)
                ),
                label);

            
            return Expression.Block(
                Expression.Assign(var, Expression.Constant(0)),
                loop
                );
        }

        public static Expression CompileNestedLoops(Expression[] shapes, Func<ParameterExpression[], Expression> onIter) 
        {
            var acts = new List<Expression>();

            var locals = new ParameterExpression[shapes.Length];
            for (int i = 0; i < shapes.Length; i++)
                locals[i] = Expression.Parameter(typeof(int), "x_" + i);
            
            var localShapes = new ParameterExpression[shapes.Length];
            for (int i = 0; i < shapes.Length; i++)
                localShapes[i] = Expression.Parameter(typeof(int), "shape_" + i);
            var localShapesAssigned = new Expression[shapes.Length];
            for (int i = 0; i < shapes.Length; i++)
                localShapesAssigned[i] = Expression.Assign(localShapes[i], shapes[i]);

            Expression currExpr = onIter(locals);

            for (int i = shapes.Length - 1; i >= 0; i--) {
                currExpr = CreateLoop(locals[i], localShapes[i], currExpr);
            }

            acts.AddRange(localShapesAssigned);
            acts.Add(currExpr);

            var localVariables = new List<ParameterExpression>();
            localVariables.AddRange(locals);
            localVariables.AddRange(localShapes);

            return Expression.Block(localVariables, Expression.Block(acts));
        }

        public static Expression CompileNestedLoops(Expression[] shapes, Func<ParameterExpression[], Expression> onIter,
            bool parallel)
        {
            if (!parallel)
                return CompileNestedLoops(shapes, onIter);
            else
            {
                var x = Expression.Parameter(typeof(int), "outerLoopVar");
                Func<ParameterExpression[], Expression> newOnIter = exprs =>
                {
                    var arr = new ParameterExpression[exprs.Length + 1];
                    arr[0] = x;
                    for (int i = 0; i < exprs.Length; i++)
                        arr[i + 1] = exprs[i];
                    return onIter(arr);
                };

                var shape0 = shapes[0];
                var others = new ArraySegment<Expression>(shapes, 1, shapes.Length - 1).ToArray();
                var compiledLoops = CompileNestedLoops(others, newOnIter);

                var del = Expression.Lambda<Action<int>>(compiledLoops, x);

                var mi = typeof(Parallel)
                    .GetMethods()
                    .Where(mi => mi.Name == nameof(Parallel.For))
                    .Where(mi => mi.GetParameters().Length == 3).FirstOrDefault();
                var call = Expression.Call(null, mi, Expression.Constant(0), shape0, del);
                return call;
            }
        }

        public static Expression BuildIndexToData(ParameterExpression[] vars, ParameterExpression[] blocks)
        {
            if (vars.Length != blocks.Length)
                throw new InvalidShapeException();
            Expression res = Expression.Multiply(vars[0], blocks[0]);
            for (int i = 1; i < blocks.Length; i++)
                res = Expression.Add(Expression.Multiply(vars[i], blocks[i]), res);
            return res;
        }

        static (ParameterExpression[] locals, Expression[] assignes) CompileLocalBlocks(ParameterExpression arr, int N, string pref)
        {
            var blocks = new ParameterExpression[N];
            for (int i = 0; i < N; i++)
                blocks[i] = Expression.Parameter(typeof(int), pref + "blocks_" + i);
            var assignes = new Expression[N];
            for (int i = 0; i < N; i++)
            {
                assignes[i] = Expression.Assign(blocks[i],
                    Expression.ArrayIndex(
                        Expression.Field(arr, typeof(GenTensor<T, TWrapper>).GetField(nameof(GenTensor<T, TWrapper>.blocks)))
                        ,
                        Expression.Constant(i)
                    )
                );
            }
            return (blocks, assignes);
        }

        static Action<GenTensor<T, TWrapper>, GenTensor<T, TWrapper>, GenTensor<T, TWrapper>> CompileForNDimensions(int N, Func<Expression, Expression, Expression> operation, bool parallel)
        {
            var a = Expression.Parameter(typeof(GenTensor<T, TWrapper>), "a");
            var b = Expression.Parameter(typeof(GenTensor<T, TWrapper>), "b");
            var res = Expression.Parameter(typeof(GenTensor<T, TWrapper>), "res");

            var actions = new List<Expression>();

            var (local_aBlocks, actABlocks) = CompileLocalBlocks(a, N, "a");

            // ablocks_0 = a.blocks[0]
            // ablocks_1 = a.blocks[1]
            // ...
            actions.AddRange(actABlocks);

            var (local_bBlocks, actBBlocks) = CompileLocalBlocks(b, N, "b");

            // bblocks_0 = b.blocks[0]
            // bblocks_1 = b.blocks[1]
            // ...
            actions.AddRange(actBBlocks);
            var (local_resBlocks, actResBlocks) = CompileLocalBlocks(res, N, "res");

            // resblocks_0 = res.blocks[0]
            // resblocks_1 = res.blocks[1]
            // ...
            actions.AddRange(actResBlocks);


            var local_ALinOffset = Expression.Parameter(typeof(int), "aLin");

            // aLin = a.LinOffset
            actions.Add(Expression.Assign(local_ALinOffset, 
                Expression.Field(a, typeof(GenTensor<T, TWrapper>).GetField(nameof(GenTensor<T, TWrapper>.LinOffset)))));

            var local_BLinOffset = Expression.Parameter(typeof(int), "bLin");

            // bLin = b.LinOffset
            actions.Add(Expression.Assign(local_BLinOffset, 
                Expression.Field(b, typeof(GenTensor<T, TWrapper>).GetField(nameof(GenTensor<T, TWrapper>.LinOffset)))));
            
            Func<ParameterExpression[], Expression> onIter = vars =>
            {
                // ablocks_0 * x_0 + ablocks_1 * x_1 + ...
                var aIndex = ExpressionCompiler<T, TWrapper>.BuildIndexToData(vars, local_aBlocks);

                // bblocks_0 * x_0 + bblocks_1 * x_1 + ...
                var bIndex = ExpressionCompiler<T, TWrapper>.BuildIndexToData(vars, local_bBlocks);

                // + aLinOffset
                aIndex = Expression.Add(aIndex, local_ALinOffset);

                // + bLinOffset
                bIndex = Expression.Add(bIndex, local_BLinOffset);

                // a.data
                var aDataField = Expression.Field(a,
                    typeof(GenTensor<T, TWrapper>).GetField(nameof(GenTensor<T, TWrapper>.data)));

                // d.data
                var bDataField = Expression.Field(b,
                    typeof(GenTensor<T, TWrapper>).GetField(nameof(GenTensor<T, TWrapper>.data)));

                // a.data[aIndex]
                var aDataIndex = Expression.ArrayIndex(aDataField, aIndex);

                // b.data[bIndex]
                var bDataIndex = Expression.ArrayIndex(bDataField, bIndex);

                // a.data[aIndex] + b.data[bIndex]
                var added = operation(aDataIndex, bDataIndex);

                // resblocks_0 * x_0 + ...
                var resIndex = ExpressionCompiler<T, TWrapper>.BuildIndexToData(vars, local_resBlocks);

                // res.data
                var resField = Expression.Field(res,
                    typeof(GenTensor<T, TWrapper>).GetField(nameof(GenTensor<T, TWrapper>.data)));

                // res.data[resIndex] = 
                var accessRes = Expression.ArrayAccess(resField, resIndex);

                var assign = Expression.Assign(accessRes, added);

                return assign;
            };

            var shapeInfo = typeof(GenTensor<T, TWrapper>).GetProperty(nameof(GenTensor<T, TWrapper>.Shape));
            var shapeFieldInfo = typeof(TensorShape).GetField(nameof(TensorShape.shape));
            var shapeProperty = Expression.Property(res, shapeInfo);
            var shapeField = Expression.Field(shapeProperty, shapeFieldInfo);

            var loops = ExpressionCompiler<T, TWrapper>.CompileNestedLoops(
                Enumerable.Range(0, N).Select(
                    id =>
                        Expression.ArrayIndex(
                            shapeField,
                            Expression.Constant(id))
                ).ToArray(),
                onIter, parallel
            );

            actions.Add(loops);

            var locals = new List<ParameterExpression>();
            locals.AddRange(local_aBlocks);
            locals.AddRange(local_bBlocks);
            locals.AddRange(local_resBlocks);
            locals.Add(local_ALinOffset);
            locals.Add(local_BLinOffset);

            Expression bl = Expression.Block(
                locals, actions.ToArray()
                );

            if (bl.CanReduce)
                bl = bl.Reduce();

            return Expression.Lambda<Action<GenTensor<T, TWrapper>, GenTensor<T, TWrapper>, GenTensor<T, TWrapper>>>(bl, a, b, res).Compile();
        }

        public enum OperationType
        {
            Addition,
            Subtraction,
            Multiplication,
            Division
        }

        private static Dictionary<(OperationType opId, int N, bool parallel), Action<GenTensor<T, TWrapper>, GenTensor<T, TWrapper>, GenTensor<T, TWrapper>>> storage 
                 = new Dictionary<(OperationType opId, int N, bool parallel), Action<GenTensor<T, TWrapper>, GenTensor<T, TWrapper>, GenTensor<T, TWrapper>>>();

        private static Action<GenTensor<T, TWrapper>, GenTensor<T, TWrapper>, GenTensor<T, TWrapper>> GetFunc(int N, Func<Expression, Expression, Expression> operation, bool parallel, OperationType ot)
        {
            var key = (ot, N, parallel);
            if (!storage.ContainsKey(key))
                storage[key] = CompileForNDimensions(N, operation, parallel);
            return storage[key];
        }

        public static GenTensor<T, TWrapper> PiecewiseAdd(GenTensor<T, TWrapper> a, GenTensor<T, TWrapper> b, bool parallel)
        {
            if (a.Shape != b.Shape)
                throw new InvalidShapeException();
            var res = new GenTensor<T, TWrapper>(a.Shape);
            GetFunc(a.Shape.Length, Addition, parallel, OperationType.Addition)(a, b, res);
            return res;
        }

        public static GenTensor<T, TWrapper> PiecewiseSubtract(GenTensor<T, TWrapper> a, GenTensor<T, TWrapper> b, bool parallel)
        {
            if (a.Shape != b.Shape)
                throw new InvalidShapeException();
            var res = new GenTensor<T, TWrapper>(a.Shape);
            GetFunc(a.Shape.Length, Subtraction, parallel, OperationType.Subtraction)(a, b, res);
            return res;
        }

        public static GenTensor<T, TWrapper> PiecewiseMultiply(GenTensor<T, TWrapper> a, GenTensor<T, TWrapper> b, bool parallel)
        {
            if (a.Shape != b.Shape)
                throw new InvalidShapeException();
            var res = new GenTensor<T, TWrapper>(a.Shape);
            GetFunc(a.Shape.Length, Multiplication, parallel, OperationType.Multiplication)(a, b, res);
            return res;
        }

        public static GenTensor<T, TWrapper> PiecewiseDivision(GenTensor<T, TWrapper> a, GenTensor<T, TWrapper> b, bool parallel)
        {
            if (a.Shape != b.Shape)
                throw new InvalidShapeException();
            var res = new GenTensor<T, TWrapper>(a.Shape);
            GetFunc(a.Shape.Length, Division, parallel, OperationType.Division)(a, b, res);
            return res;
        }
    }
}