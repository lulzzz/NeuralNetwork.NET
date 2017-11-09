﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeuralNetworkNET.Helpers;

namespace NeuralNetworkNET.Unit
{
    /// <summary>
    /// Test class for the <see cref="MatrixExtensions"/> class
    /// </summary>
    [TestClass]
    [TestCategory(nameof(MatrixExtensionsTest))]
    public class MatrixExtensionsTest
    {
        /// <summary>
        /// Vector-matrix multiplication test
        /// </summary>
        [TestMethod]
        public void LinearMultiplication()
        {
            // Test values
            double[,] m =
            {
                { 1, 1, 1, 1 },
                { 0, 2, -1, 0 },
                { 1, 1, 1, 1 },
                { 0, 0, -1, 1 }
            };
            double[]
                v = { 1, 2, 0.1, -2 },
                r = { 1.1, 5.1, 1.1, -0.9 },
                t = v.Multiply(m);
            Assert.IsTrue(t.ContentEquals(r));

            // Exception test
            double[] f = { 1, 2, 3, 4, 5, 6 };
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => f.Multiply(m));
        }

        /// <summary>
        /// Matrix-matrix multiplication test
        /// </summary>
        [TestMethod]
        public void SpatialMultiplication()
        {
            // Test values
            double[,]
                m1 =
                {
                    { 1, 2, 3 },
                    { 5, 0.1, -2 }
                },
                m2 =
                {
                    { 5, 2, -1, 3 },
                    { -5, 2, -7, 0.9 },
                    { 0.1, 0.2, -0.1, 2 }
                },
                r =
                {
                    { -4.7, 6.6, -15.3, 10.8 },
                    { 24.3, 9.7999999999999989, -5.5, 11.09 }
                },
                t = m1.Multiply(m2);
            Assert.IsTrue(t.ContentEquals(r));

            // Exception test
            double[,] f =
            {
                { 1, 2, 1, 0, 0 },
                { 5, 0.1, 0, 0, 0 }
            };
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => f.Multiply(m1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => m2.Multiply(f));
        }

        /// <summary>
        /// Element-wise matrix-matrix multiplication test
        /// </summary>
        [TestMethod]
        public void HadamardProductTest()
        {
            // Test values
            double[,]
                m1 =
                {
                    { 1, 2, 3 },
                    { 5, 1, -2 },
                    { 1, 2, 3 },
                },
                m2 =
                {
                    { 5, 2, -1 },
                    { -5, 2, -7 },
                    { 1, 2, 2 }
                },
                r =
                {
                    { 5, 4, -3 },
                    { -25, 2, 14 },
                    { 1, 4, 6 }
                },
                t = m1.HadamardProduct(m2);
            Assert.IsTrue(t.ContentEquals(r));

            // Exception test
            double[,] f =
            {
                { 1, 2, 1, 0, 0 },
                { 5, 0.1, 0, 0, 0 }
            };
            Assert.ThrowsException<ArgumentException>(() => f.HadamardProduct(m1));
            Assert.ThrowsException<ArgumentException>(() => m2.HadamardProduct(f));
        }

        /// <summary>
        /// Matrix transposition
        /// </summary>
        [TestMethod]
        public void Transposition()
        {
            // Test values
            double[,]
                m =
                {
                    { 1, 1, 1, 1 },
                    { 0, 2, -1, 0 }
                },
                r =
                {
                    { 1, 0 },
                    { 1, 2 },
                    { 1, -1 },
                    { 1, 0 }
                },
                t = m.Transpose();
            Assert.IsTrue(t.ContentEquals(r));
        }

        /// <summary>
        /// Matrix array flattening
        /// </summary>
        [TestMethod]
        public void Flattening()
        {
            // Test values
            double[][,] mv =
            {
                new[,]
                {
                    { 1.0, 2.0 },
                    { 3.0, 4.0 }
                },
                new[,]
                {
                    { 0.1, 0.2 },
                    { 0.3, 0.4 }
                },
                new[,]
                {
                    { -1.0, -2.0 },
                    { -3.0, -4.0 }
                }
            };
            double[]
                r = { 1.0, 2.0, 3.0, 4.0, 0.1, 0.2, 0.3, 0.4, -1.0, -2.0, -3.0, -4.0 },
                t = mv.Flatten();
            Assert.IsTrue(t.ContentEquals(r));
        }

        [TestMethod]
        public void IndexOfMax1()
        {
            double[]
                v1 = new double[0],
                v2 = new double[1];
            Assert.IsTrue(v1.IndexOfMax() == 0);
            Assert.IsTrue(v2.IndexOfMax() == 0);
        }

        [TestMethod]
        public void IndexOfMax2()
        {
            double[]
                v1 = { 1d, 2d, 3d, 4d, 5d },
                v2 = { 99d, 11d },
                v3 = { -2d, -2.1d },
                v4 = { 0d, 0d, 0d, 0d, 1d, 0d, 0d, 0d, 0d };
            Assert.IsTrue(v1.IndexOfMax() == 4);
            Assert.IsTrue(v2.IndexOfMax() == 0);
            Assert.IsTrue(v3.IndexOfMax() == 0);
            Assert.IsTrue(v4.IndexOfMax() == 4);
        }

        [TestMethod]
        public void ToFormattedString()
        {
            double[,]
                empty = { { } },
                oneLine = { { 1.0, 2.0, 3.0 } },
                complete = { { 1.0, 2.0, 3.0 }, { 4.0, 5.0, 6.0 } };
            String
                emptyString = "{ { } }",
                oneLineString = "{ { 1, 2, 3 } }",
                completeString = "{ { 1, 2, 3 },\n  { 4, 5, 6 } }";
            Assert.IsTrue(empty.ToFormattedString().Equals(emptyString));
            Assert.IsTrue(oneLine.ToFormattedString().Equals(oneLineString));
            Assert.IsTrue(complete.ToFormattedString().Equals(completeString));
        }
    }
}
