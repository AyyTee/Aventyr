using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;

namespace UnitTest
{
    [TestClass]
    public class MathExtTests
    {
        #region PointLeftOfLine tests
        [TestMethod]
        public void PointLeftOfLineTest0()
        {
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0)));
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2d(0, 0), new Vector2d(1, 0), new Vector2d(0, 0)));
        }
        [TestMethod]
        public void PointLeftOfLineTest1()
        {
            Assert.IsTrue(MathExt.PointLeftOfLine(new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 100)));
            Assert.IsTrue(MathExt.PointLeftOfLine(new Vector2d(0, 0), new Vector2d(1, 0), new Vector2d(0, 100)));
        }
        [TestMethod]
        public void PointLeftOfLineTest2()
        {
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, -100)));
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2d(0, 0), new Vector2d(1, 0), new Vector2d(0, -100)));
        }
        [TestMethod]
        public void PointLeftOfLineTest3()
        {
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, -100)));
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2d(0, 0), new Vector2d(0, 1), new Vector2d(0, -100)));
        }
        [TestMethod]
        public void PointLeftOfLineTest4()
        {
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, -100)));
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, -100)));
        }
        [TestMethod]
        public void PointLeftOfLineTest5()
        {
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 100)));
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2d(0, 0), new Vector2d(0, 0), new Vector2d(0, 100)));
        }
        [TestMethod]
        public void PointLeftOfLineTest6()
        {
            Assert.IsTrue(MathExt.PointLeftOfLine(new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 100)));
            Assert.IsTrue(MathExt.PointLeftOfLine(new Vector2d(0, 0), new Vector2d(1, 1), new Vector2d(0.5f, 100)));
        }
        [TestMethod]
        public void PointLeftOfLineTest7()
        {
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2(0, 0), new Vector2(-1, -1), new Vector2(0, 100)));
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2d(0, 0), new Vector2d(-1, -1), new Vector2d(0, 100)));
        }
        [TestMethod]
        public void PointLeftOfLineTest8()
        {
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2(1, -1), new Vector2(0, 0), new Vector2(0, 100)));
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2d(1, -1), new Vector2d(0, 0), new Vector2d(0, 100)));
        }
        [TestMethod]
        public void PointLeftOfLineTest9()
        {
            Assert.IsTrue(MathExt.PointLeftOfLine(new Vector2(-1, 1), new Vector2(0, 0), new Vector2(0, 100)));
            Assert.IsTrue(MathExt.PointLeftOfLine(new Vector2d(-1, 1), new Vector2d(0, 0), new Vector2d(0, 100)));
        }
        [TestMethod]
        public void PointLeftOfLineTest10()
        {
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2(-1, 1), new Vector2(0, 0), new Vector2(0, -100)));
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2d(-1, 1), new Vector2d(0, 0), new Vector2d(0, -100)));
        }
        [TestMethod]
        public void PointLeftOfLineTest11()
        {
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2(-1, 1), new Vector2(0, 0), new Vector2(0, -100)));
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2d(-1, 1), new Vector2d(0, 0), new Vector2d(0, -100)));
        }
        [TestMethod]
        public void PointLeftOfLineTest12()
        {
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2(-5, 20), new Vector2(20, 0), new Vector2(0, 0)));
            Assert.IsFalse(MathExt.PointLeftOfLine(new Vector2d(-5, 20), new Vector2d(20, 0), new Vector2d(0, 0)));
        }
        #endregion
        #region LineInRectangle tests
        [TestMethod]
        public void LineInRectangle0()
        {
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2(0, 0), new Vector2(10, 10), new Vector2(1, 1), new Vector2(2, 2)));
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2d(0, 0), new Vector2d(10, 10), new Vector2d(1, 1), new Vector2d(2, 2)));
        }
        [TestMethod]
        public void LineInRectangle1()
        {
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2(0, 0), new Vector2(-10, -10), new Vector2(-1, -1), new Vector2(-2, -2)));
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2d(0, 0), new Vector2d(-10, -10), new Vector2d(-1, -1), new Vector2d(-2, -2)));
        }
        [TestMethod]
        public void LineInRectangle2()
        {
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(4, 6), new Vector2(20, 4)));
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(4, 6), new Vector2d(20, 4)));
        }
        [TestMethod]
        public void LineInRectangle3()
        {
            Assert.IsFalse(MathExt.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(0, 0), new Vector2(20, 1)));
            Assert.IsFalse(MathExt.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(0, 0), new Vector2d(20, 1)));
        }
        [TestMethod]
        public void LineInRectangle4()
        {
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(5, 5), new Vector2(0, 0)));
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(5, 5), new Vector2d(0, 0)));
        }
        [TestMethod]
        public void LineInRectangle5()
        {
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(0, 0), new Vector2(5, 5)));
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(0, 0), new Vector2d(5, 5)));
        }
        [TestMethod]
        public void LineInRectangle6()
        {
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(6, 5), new Vector2(0, 0)));
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(6, 5), new Vector2d(0, 0)));
        }
        [TestMethod]
        public void LineInRectangle7()
        {
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(0, 0), new Vector2(6, 5)));
            Assert.IsTrue(MathExt.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(0, 0), new Vector2d(6, 5)));
        }
        [TestMethod]
        public void LineInRectangle8()
        {
            Assert.IsFalse(MathExt.LineInRectangle(new Vector2(10, 10), new Vector2(5, 5), new Vector2(0, 0), new Vector2(0, 0)));
            Assert.IsFalse(MathExt.LineInRectangle(new Vector2d(10, 10), new Vector2d(5, 5), new Vector2d(0, 0), new Vector2d(0, 0)));
        }
        #endregion
    }
}
