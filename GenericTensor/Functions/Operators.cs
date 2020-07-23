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
    public partial class Tensor<T>
    {
        /// <summary>
        /// A tensor is a matrix if has two dimensions, e. g. [3 x 4]
        /// </summary>
        public bool IsMatrix => Shape.Count == 2;

        /// <summary>
        /// A tensor is a vector if has one dimension
        /// </summary>
        public bool IsVector => Shape.Count == 1;

        /// <summary>
        /// Calls your TWrapper.Equals
        /// Be sure to override it when using this function or ==, != operators
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is Tensor<T> ten))
                return false;
            if (ten.Shape != Shape)
                return false;
            foreach (var (index, _) in ten.Iterate())
                if (!ConstantsAndFunctions<T>.AreEqual(this.GetValueNoCheck(index), ten.GetValueNoCheck(index)))
                    return false;
            return true;
        }

        public static bool operator ==(Tensor<T> a, Tensor<T> b)
            => a.Equals(b);

        public static bool operator !=(Tensor<T> a, Tensor<T> b)
            => a.Equals(b);

        
    }
}
