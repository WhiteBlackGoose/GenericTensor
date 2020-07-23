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
using GenericTensor.Core;
using GenericTensor.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class MatrixProduct
    {
        public MatrixProduct()
        {
            BuiltinTypeInitter.InitForInt();
        }

        [TestMethod]
        public void MatrixDotProduct1()
        {
            var A = Tensor<int>.CreateMatrix(
                new [,]
                {
                    { 12, -1, 1 },
                    { 0,   1, 4 },
                });
            var B = Tensor<int>.CreateMatrix(
                new [,]
                {
                    { 1, -1 },
                    { 0,  1 },
                    { 3,  0 },
                    { 0,  3 },
                });

            A.TransposeMatrix();
            B.TransposeMatrix();
            Assert.AreEqual(Tensor<int>.MatrixDotProduct(A, B), Tensor<int>.CreateMatrix(
                new [,]
                {
                    { 12, 0, 36, 0 },
                    { -2, 1, -3, 3 },
                    {-3,  4, 3,  12}
                }
            ));
        }

        [TestMethod]
        public void MatrixTensorMp()
        {
            var a = Tensor<int>.CreateMatrix(
                new [,]
                {
                    { 1, 2 },
                    { 3, 4 }
                }
                );
            var b = Tensor<int>.CreateMatrix(
                new [,]
                {
                    { 5, 7 },
                    { 6, 8 }
                }
            );
            var T1 = Tensor<int>.Stack(a, b);

            var c = Tensor<int>.CreateMatrix(
                new [,]
                {
                    { -3, 2 },
                    { 3, 5 }
                }
            );

            var d = Tensor<int>.CreateMatrix(
                new [,]
                {
                    { -3, 2 },
                    { 23, 5 }
                }
            );

            var T2 = Tensor<int>.Stack(c, d);

            var exp1 = Tensor<int>.CreateMatrix(
                new [,]
                {
                    { 3, 12 },
                    { 3, 26 }
                }
            );

            var exp2 = Tensor<int>.CreateMatrix(
                new [,]
                {
                    { 146, 45 },
                    { 166, 52 }
                }
            );

            var exp = Tensor<int>.Stack(exp1, exp2);

            Assert.AreEqual(exp, Tensor<int>.TensorMatrixDotProduct(T1, T2));
        }
    }
}
