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
using System.Linq;
using System.Text;

namespace GenericTensor.Core
{
    public partial class Tensor<TWrapper, TPrimitive>
    {
        /// <summary>
        /// Creates a tensor whose all matrices are identity matrices
        /// 1 is achieved with TWrapper.SetOne()
        /// 0 is achieved with TWrapper.SetZero()
        /// </summary>
        public static Tensor<TWrapper, TPrimitive> CreateIdentityTensor(int[] dimensions, int finalMatrixDiag)
        {
            var newDims = new int[dimensions.Length + 2];
            for (int i = 0; i < dimensions.Length; i++)
                newDims[i] = dimensions[i];
            newDims[newDims.Length - 2] = newDims[newDims.Length - 1] = finalMatrixDiag;
            var res = new Tensor<TWrapper, TPrimitive>(newDims);
            foreach (var index in res.IterateOverMatrices())
            {
                var iden = CreateIdentityMatrix(finalMatrixDiag);
                res.SetSubtensor(iden, index);
            }
            return res;
        }

        /// <summary>
        /// Creates an indentity matrix whose width and height are equal to diag
        /// 1 is achieved with TWrapper.SetOne()
        /// 0 is achieved with TWrapper.SetZero()
        /// </summary>
        public static Tensor<TWrapper, TPrimitive> CreateIdentityMatrix(int diag)
        {
            var res = new Tensor<TWrapper, TPrimitive>(diag, diag);
            for (int i = 0; i < res.Data.Length; i++)
                res.Data[i].SetZero();
            for (int i = 0; i < diag; i++)
            {
                res.GetFlattenedWrapper(i, i).SetOne();
            }
            return res;
        }

        /// <summary>
        /// Creates a vector from an array of primitives
        /// Its length will be equal to elements.Length
        /// </summary>
        public static Tensor<TWrapper, TPrimitive> CreateVector(params TPrimitive[] elements)
        {
            var res = new Tensor<TWrapper, TPrimitive>(elements.Length);
            for (int i = 0; i < elements.Length; i++)
                res[i] = elements[i];
            return res;
        }

        private static (int height, int width) ExtractAndCheck<T>(T[,] data)
        {
            var width = data.GetLength(0);
            if (width <= 0)
                throw new InvalidShapeException();
            var height = data.GetLength(1);
            if (height <= 0)
                throw new InvalidShapeException();
            return (width, height);
        }

        /// <summary>
        /// Creates a matrix from a two-dimensional array of primitives
        /// for example
        /// var M = Tensor.CreateMatrix(new[,]
        /// {
        ///     {1, 2},
        ///     {3, 4}
        /// });
        /// where yourData.GetLength(0) is Shape[0]
        /// yourData.GetLength(1) is Shape[1]
        /// </summary>
        public static Tensor<TWrapper, TPrimitive> CreateMatrix(TPrimitive[,] data)
        {
            var (width, height) = Tensor<TWrapper, TPrimitive>.ExtractAndCheck(data);
            var res = new Tensor<TWrapper, TPrimitive>(width, height);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                res[x, y] = data[x, y];
            return res;
        }

        /// <summary>
        /// Creates a matrix from a two-dimensional array of wrappers
        /// </summary>
        public static Tensor<TWrapper, TPrimitive> CreateMatrix(TWrapper[,] data)
        {
            var (width, height) = Tensor<TWrapper, TPrimitive>.ExtractAndCheck(data);
            var res = new Tensor<TWrapper, TPrimitive>(width, height);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                res.SetCell(data[x, y], x, y);
            return res;
        }

        /// <summary>
        /// Creates a matrix of width and height size
        /// and iterator for each pair of coordinate
        /// </summary>
        public static Tensor<TWrapper, TPrimitive> CreateMatrix(int width, int height, Func<(int x, int y), TWrapper> stepper)
        {
            var data = new TWrapper[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                data[x, y] = stepper((x, y));
            return CreateMatrix(data);
        }

        /// <summary>
        /// Creates a matrix of width and height size
        /// </summary>
        public static Tensor<TWrapper, TPrimitive> CreateMatrix(int width, int height)
        {
            var data = new TWrapper[width, height];
            return CreateMatrix(data);
        }

        /// <summary>
        /// Creates a tensor of given size with iterator over its indecies
        /// (its only argument is an array of integers which are indecies of the tensor)
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static Tensor<TWrapper, TPrimitive> CreateTensor(TensorShape shape, Func<int[], TPrimitive> operation)
        {
            var res = new Tensor<TWrapper, TPrimitive>(shape);
            foreach (var ind in res.IterateOverElements())
                res[ind] = operation(ind);
            return res;
        }
    }
}
