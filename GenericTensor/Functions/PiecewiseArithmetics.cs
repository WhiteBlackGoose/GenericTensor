﻿#region copyright
/*
 * MIT License
 * 
 * Copyright (c) 2020 WhiteBlackGoose
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
#endregion


using System;
using System.Threading.Tasks;
using GenericTensor.Core;

namespace GenericTensor.Functions
{
    internal static class PiecewiseArithmetics<T>
    {
        public static GenTensor<T> Zip(GenTensor<T> a,
            GenTensor<T> b, Func<T, T, T> operation, Threading threading = Threading.Single)
        {
            #if ALLOW_EXCEPTIONS
            if (a.Shape != b.Shape)
                throw new InvalidShapeException("Arguments should be of the same shape");
            #endif
            var res = new GenTensor<T>(a.Shape);

            var parallel = threading == Threading.Multi || (threading == Threading.Auto && a.Volume > 850);


            if (!parallel)
            {

                if (res.Shape.shape.Length == 1)
                    for (int x = 0; x < res.Shape.shape[0]; x++)
                        res.data[x] = ConstantsAndFunctions<T>.Forward(
                            operation(a.GetValueNoCheck(x), b.GetValueNoCheck(x)));
                else if (res.Shape.shape.Length == 2)
                    for (int x = 0; x < res.Shape.shape[0]; x++)
                    for (int y = 0; y < res.Shape.shape[1]; y++)
                        res.data[x * res.blocks[0] + y] = ConstantsAndFunctions<T>.Forward(
                            operation(a.GetValueNoCheck(x, y), b.GetValueNoCheck(x, y)));
                else if (res.Shape.shape.Length == 3)
                    for (int x = 0; x < res.Shape.shape[0]; x++)
                    for (int y = 0; y < res.Shape.shape[1]; y++)
                    for (int z = 0; z < res.Shape.shape[2]; z++)
                        res.data[x * res.blocks[0] + y * res.blocks[1] + z] = ConstantsAndFunctions<T>.Forward(
                            operation(a.GetValueNoCheck(x, y, z), b.GetValueNoCheck(x, y, z)));
                else
                    foreach (var index in res.IterateOverElements())
                        res.SetValueNoCheck(ConstantsAndFunctions<T>.Forward(
                            operation(a.GetValueNoCheck(index), b.GetValueNoCheck(index))), index);
            }
            else
            {
                if (res.Shape.shape.Length == 1)
                    for (int x = 0; x < res.Shape.shape[0]; x++)
                        res.data[x] = ConstantsAndFunctions<T>.Forward(
                            operation(a.GetValueNoCheck(x), b.GetValueNoCheck(x)));
                else if (res.Shape.shape.Length == 2)
                    Parallel.For(0, res.Shape.shape[0], x =>
                    {
                        for (int y = 0; y < res.Shape.shape[1]; y++)
                            res.data[x * res.blocks[0] + y] = ConstantsAndFunctions<T>.Forward(
                                operation(a.GetValueNoCheck(x, y), b.GetValueNoCheck(x, y)));
                    });
                else if (res.Shape.shape.Length == 3)
                    Parallel.For(0, res.Shape.shape[0], x =>
                    {
                        for (int y = 0; y < res.Shape.shape[1]; y++)
                        for (int z = 0; z < res.Shape.shape[2]; z++)
                            res.data[x * res.blocks[0] + y * res.blocks[1] + z] = ConstantsAndFunctions<T>.Forward(
                                operation(a.GetValueNoCheck(x, y, z), b.GetValueNoCheck(x, y, z)));
                    });
                else
                    foreach (var index in res.IterateOverElements())
                        res.SetValueNoCheck(ConstantsAndFunctions<T>.Forward(
                            operation(a.GetValueNoCheck(index), b.GetValueNoCheck(index))), index);
            }
            return res;
        }

        public static GenTensor<T> PiecewiseAdd(GenTensor<T> a,
            GenTensor<T> b, Threading threading)
            => Zip(a, b, ConstantsAndFunctions<T>.Add, threading);

        public static GenTensor<T> PiecewiseSubtract(GenTensor<T> a,
            GenTensor<T> b, Threading threading)
            => Zip(a, b, ConstantsAndFunctions<T>.Subtract, threading);

        public static GenTensor<T> PiecewiseMultiply(GenTensor<T> a,
            GenTensor<T> b, Threading threading)
            => Zip(a, b, ConstantsAndFunctions<T>.Multiply, threading);

        public static GenTensor<T> PiecewiseDivide(GenTensor<T> a,
            GenTensor<T> b, Threading threading)
            => Zip(a, b, ConstantsAndFunctions<T>.Divide, threading);

        public static GenTensor<T> PiecewiseAdd(GenTensor<T> a,
            T b, Threading threading)
            => Constructors<T>.CreateTensor(a.Shape, ind => 
                ConstantsAndFunctions<T>.Add(a[ind], b), threading);

        public static GenTensor<T> PiecewiseSubtract(GenTensor<T> a,
            T b, Threading threading)
            => Constructors<T>.CreateTensor(a.Shape, ind => 
                ConstantsAndFunctions<T>.Subtract(a[ind], b), threading);

        public static GenTensor<T> PiecewiseSubtract(
            T a, GenTensor<T> b, Threading threading)
            => Constructors<T>.CreateTensor(b.Shape, ind => 
                ConstantsAndFunctions<T>.Subtract(a, b[ind]), threading);

        public static GenTensor<T> PiecewiseMultiply(GenTensor<T> a,
            T b, Threading threading)
            => Constructors<T>.CreateTensor(a.Shape, ind => 
                ConstantsAndFunctions<T>.Multiply(a[ind], b), threading);

        public static GenTensor<T> PiecewiseDivide(GenTensor<T> a,
            T b, Threading threading)
            => Constructors<T>.CreateTensor(a.Shape, ind => 
                ConstantsAndFunctions<T>.Divide(a[ind], b), threading);

        public static GenTensor<T> PiecewiseDivide(
            T a, GenTensor<T> b, Threading threading)
            => Constructors<T>.CreateTensor(b.Shape, ind => 
                ConstantsAndFunctions<T>.Divide(a, b[ind]), threading);
    }
}
