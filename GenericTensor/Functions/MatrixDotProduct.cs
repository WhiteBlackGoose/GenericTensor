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
using System.Collections.Generic;
using System.Text;
using GenericTensor.Functions;

namespace GenericTensor.Core
{
    public partial class Tensor<TWrapper, TPrimitive>
    {
        /// <summary>
        /// Finds matrix multiplication result
        /// a and b are matrices
        /// a.Shape[1] should be equal to b.Shape[0]
        /// the resulting matrix is [a.Shape[0] x b.Shape[1]] shape
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Tensor<TWrapper, TPrimitive> MatrixDotProduct(Tensor<TWrapper, TPrimitive> a,
            Tensor<TWrapper, TPrimitive> b)
        {
            if (!a.IsMatrix || !b.IsMatrix)
                throw new InvalidShapeException($"Both {nameof(a)} and {nameof(b)} should be matrices");

            if (a.Shape[1] != b.Shape[0])
                throw new InvalidShapeException($"{nameof(a)}'s height must be equal to {nameof(b)}'s width");
            var width = a.Shape[0];
            var height = b.Shape[1];
            var row = a.Shape[1];
            var res = CreateMatrix(width, height);
            b.Transpose(0, 1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    
                    var s = ConstantsAndFunctions<TWrapper, TPrimitive>.CreateZero();
                    for (int i = 0; i < row; i++)
                    {
                        var v1 = a.GetCell(x, i).Copy();
                        var v2 = b.GetCell(i, y).Copy();
                        s.Add(ConstantsAndFunctions<TWrapper, TPrimitive>.MultiplySaveWrapper((TWrapper)v1, (TWrapper)v2));
                    }
                    res.SetCell(s, x, y);
                    
                    /*
                    var v1 = a.GetSubtensor(x);
                    var v2 = b.GetSubtensor(y);
                    var scalar = Tensor<TWrapper, TPrimitive>.VectorDotProduct(v1, v2);
                    res.SetCell(ConstantsAndFunctions<TWrapper, TPrimitive>.Create(scalar), x, y);*/
                }
            }
            return res;
        }

        /// <summary>
        /// Applies matrix dot product operation for
        /// all matrices in tensors
        /// </summary>
        public static Tensor<TWrapper, TPrimitive> TensorMatrixDotProduct(Tensor<TWrapper, TPrimitive> a,
            Tensor<TWrapper, TPrimitive> b)
        {
            if (a.Shape.Count < 2 || b.Shape.Count < 2)
                throw new InvalidShapeException($"Arguments should be at least matrices while their shapes are {a.Shape} and {b.Shape}");
            if (a.Shape.SubShape(0, 2) != b.Shape.SubShape(0, 2))
                throw new InvalidShapeException("Other dimensions of tensors should be equal");
            var oldShape = a.Shape.SubShape(0, 2).ToArray();
            var newShape = new int[oldShape.Length + 2];
            for (int i = 0; i < oldShape.Length; i++)
                newShape[i] = oldShape[i];
            newShape[newShape.Length - 2] = a.Shape[a.Shape.Length - 2];
            newShape[newShape.Length - 1] = b.Shape[b.Shape.Length - 1];
            var resTensor = new Tensor<TWrapper, TPrimitive>(newShape);
            foreach (var subDimensions in a.IterateOverMatrices())
            {
                var product = MatrixDotProduct(a.GetSubtensor(subDimensions), b.GetSubtensor(subDimensions));
                resTensor.SetSubtensor(product, subDimensions);
            }
            return resTensor;
        }
    }
}