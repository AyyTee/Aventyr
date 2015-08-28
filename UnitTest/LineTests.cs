using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;

namespace UnitTest
{
    [TestClass]
    public class LineTests
    {
        #region PointIsLeft tests
        [TestMethod]
        public void PointIsLeftTest0()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(1, 0));
            Assert.IsFalse(line.PointIsLeft(new Vector2(0, 0)));
        }
        [TestMethod]
        public void PointIsLeftTest1()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(1, 0));
            Assert.IsTrue(line.PointIsLeft(new Vector2(0, 100)));
        }
        [TestMethod]
        public void PointIsLeftTest2()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(1, 0));
            Assert.IsFalse(line.PointIsLeft(new Vector2(0, -100)));
        }
        [TestMethod]
        public void PointIsLeftTest3()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(0, 1));
            Assert.IsFalse(line.PointIsLeft(new Vector2(0, -100)));
        }
        [TestMethod]
        public void PointIsLeftTest4()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(0, 0));
            Assert.IsFalse(line.PointIsLeft(new Vector2(0, -100)));
        }
        [TestMethod]
        public void PointIsLeftTest5()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(0, 0));
            Assert.IsFalse(line.PointIsLeft(new Vector2(0, 100)));
        }
        [TestMethod]
        public void PointIsLeftTest6()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(1, 1));
            Assert.IsTrue(line.PointIsLeft(new Vector2(0.5f, 100)));
        }
        [TestMethod]
        public void PointIsLeftTest7()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(-1, -1));
            Assert.IsFalse(line.PointIsLeft(new Vector2(0, 100)));
        }
        [TestMethod]
        public void PointIsLeftTest8()
        {
            Line line = new Line(new Vector2(1, -1), new Vector2(0, 0));
            Assert.IsFalse(line.PointIsLeft(new Vector2(0, 100)));
        }
        [TestMethod]
        public void PointIsLeftTest9()
        {
            Line line = new Line(new Vector2(-1, 1), new Vector2(0, 0));
            Assert.IsTrue(line.PointIsLeft(new Vector2(0, 100)));
        }
        [TestMethod]
        public void PointIsLeftTest10()
        {
            Line line = new Line(new Vector2(-1, 1), new Vector2(0, 0));
            Assert.IsFalse(line.PointIsLeft(new Vector2(0, -100)));
        }
        [TestMethod]
        public void PointIsLeftTest11()
        {
            Line line = new Line(new Vector2(-1, 1), new Vector2(0, 0));
            Assert.IsFalse(line.PointIsLeft(new Vector2(0, -100)));
        }
        [TestMethod]
        public void PointIsLeftTest12()
        {
            Line line = new Line(new Vector2(-5, 20), new Vector2(20, 0));
            Assert.IsFalse(line.PointIsLeft(new Vector2(0, 0)));
        }
        #endregion
    }
}
