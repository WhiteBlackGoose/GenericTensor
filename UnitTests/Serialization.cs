﻿#region copyright
/*
 * MIT License
 * 
 * Copyright (c) 2020-2021 WhiteBlackGoose
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
using System.Numerics;
using GenericTensor.Core;
using GenericTensor.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class Serialization
    {
        public void CircleTest<T, TWrapper>(GenTensor<T, TWrapper> tensor) 
            where TWrapper : struct, IOperations<T>
        {
            var serialized = tensor.Serialize();
            var deserialized = GenTensor<T, TWrapper>.Deserialize(serialized);
            Assert.AreEqual(tensor, deserialized);
        }

        [TestMethod]
        public void TestVecInt()
            => CircleTest(GenTensor<int, IntWrapper>.CreateVector(3, 4, 5));

        [TestMethod]
        public void TestVecFloat()
            => CircleTest(GenTensor<float, FloatWrapper>.CreateVector(3.5f, 4, -5.1f, 6.4f));

        [TestMethod]
        public void TestVecDouble()
            => CircleTest(GenTensor<double, DoubleWrapper>.CreateVector(3.5d, 4, -5.1d, 6.4d));

        [TestMethod]
        public void TestMatrixInt()
            => CircleTest(GenTensor<int, IntWrapper>.CreateMatrix(
                new[,]
                {
                    {1, 2, 3},
                    {4, 5, 6}
                }));

        [TestMethod]
        public void TestMatrixFloat()
            => CircleTest(GenTensor<float, FloatWrapper>.CreateMatrix(
                new[,]
                {
                    {1f, 2f, 3.3f},
                    {4, 5.5f, 6.6f}
                }));

        [TestMethod]
        public void TestMatrixDouble()
            => CircleTest(GenTensor<double, DoubleWrapper>.CreateMatrix(
                new[,]
                {
                    {1d, 2d, 3.3d},
                    {4, 5.5d, 6.6d}
                }));

        [TestMethod]
        public void TestTensorInt()
            => CircleTest(GenTensor<int, IntWrapper>.CreateTensor(
                new[,,]
                {
                    {
                        {1, 3, 3},
                        {4, 5, 6}
                    },
                    {
                        {-4, 2, 3},
                        {7, 8, 11}
                    }
                }
                ));

        [TestMethod]
        public void TestTensorFloat()
            => CircleTest(GenTensor<float, FloatWrapper>.CreateTensor(
                new[, ,]
                {
                    {
                        {1.1f, 3f, 3f},
                        {4f, 5.5f, 6f}
                    },
                    {
                        {-4f, 2.5f, 3f},
                        {7f, 8f, 11.4f}
                    }
                }
            ));

        [TestMethod]
        public void TestTensorDouble()
            => CircleTest(GenTensor<double, DoubleWrapper>.CreateTensor(
                new[, ,]
                {
                    {
                        {1.1d, 3d, 3d},
                        {4d, 5.5d, 6d}
                    },
                    {
                        {-4d, 2.5d, 3d},
                        {7d, 8d, 11.4d}
                    }
                }
            ));

        [TestMethod]
        public void TestTensorGWInt()
            => CircleTest(GenTensor<int, GenericWrapper<int>>.CreateTensor(
                new[, ,]
                {
                    {
                        {1, 3, 3},
                        {4, 5, 6}
                    },
                    {
                        {-4, 2, 3},
                        {7, 8, 11}
                    }
                }
            ));

        [TestMethod]
        public void TestTensorGWFloat()
            => CircleTest(GenTensor<float, GenericWrapper<float>>.CreateTensor(
                new[, ,]
                {
                    {
                        {1f, 3f, 3f},
                        {4f, 5f, 6f}
                    },
                    {
                        {-4f, 2f,  3f},
                        { 7f, 8f, 11f}
                    }
                }
            ));

        [TestMethod]
        public void TestTensorGWDouble()
            => CircleTest(GenTensor<double, GenericWrapper<double>>.CreateTensor(
                new[, ,]
                {
                    {
                        {1d, 3d, 3d},
                        {4d, 5d, 6d}
                    },
                    {
                        {-4d, 2d, 3d},
                        { 7d, 8d, 11d}
                    }
                }
            ));

        [TestMethod]
        public void TestTensorGWComplex()
            => CircleTest(GenTensor<Complex, GenericWrapper<Complex>>.CreateTensor(
                new[, ,]
                {
                    {
                        {new Complex(1, 0), new Complex(3, 0), new Complex(3, 0)},
                        {new Complex(4, 0), new Complex(5, 0), new Complex(6, 0)}
                    },
                    {
                        {new Complex(-4, 0), new Complex(2,  0), new Complex(3,  0)},
                        {new Complex(7,  0),  new Complex(8, 0), new Complex(11, 0)}
                    }
                }
            ));

        [TestMethod]
        public void TestTensorGWByte()
        {
            Assert.ThrowsException<NotSupportedException>(() =>
            CircleTest(GenTensor<byte, GenericWrapper<byte>>.CreateTensor(
                  new[, ,]
                  {
                    {
                        {(byte)1, (byte)3, (byte)3},
                        {(byte)4, (byte)5, (byte)6}
                    },
                    {
                        {(byte)4, (byte)2, (byte)3},
                        {(byte)7,  (byte)8, (byte)11}
                    }
                  }
              )));
        }

        [TestMethod]
        public void TestTensorComplex()
            => CircleTest(GenTensor<Complex, ComplexWrapper>.CreateTensor(
                new[, ,]
                {
                    {
                        {new Complex(4, 6), 3d, 3d},
                        {4d, 5.5d, new Complex(-1, 5)}
                    },
                    {
                        {-4d, new Complex(5, 6), 3d},
                        {7d, 8d, new Complex(4, 5.4d)}
                    }
                }
            ));
    }
}
