using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;

namespace UnitTest
{
    [TestClass]
    public class LineTests
    {
        #region GetSideOf tests
        [TestMethod]
        public void GetSideOfTest0()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(1, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, 0)) == Line.Side.IsLeftOf);
        }
        [TestMethod]
        public void GetSideOfTest1()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(1, 0));
            Assert.IsTrue(line.GetSideOf(new Vector2(0, 100)) == Line.Side.IsLeftOf);
        }
        [TestMethod]
        public void GetSideOfTest2()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(1, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, -100)) == Line.Side.IsLeftOf);
        }
        [TestMethod]
        public void GetSideOfTest3()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(0, 1));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, -100)) == Line.Side.IsLeftOf);
        }
        [TestMethod]
        public void GetSideOfTest4()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(0, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, -100)) == Line.Side.IsLeftOf);
        }
        [TestMethod]
        public void GetSideOfTest5()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(0, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, 100)) == Line.Side.IsLeftOf);
        }
        [TestMethod]
        public void GetSideOfTest6()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(1, 1));
            Assert.IsTrue(line.GetSideOf(new Vector2(0.5f, 100)) == Line.Side.IsLeftOf);
        }
        [TestMethod]
        public void GetSideOfTest7()
        {
            Line line = new Line(new Vector2(0, 0), new Vector2(-1, -1));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, 100)) == Line.Side.IsLeftOf);
        }
        [TestMethod]
        public void GetSideOfTest8()
        {
            Line line = new Line(new Vector2(1, -1), new Vector2(0, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, 100)) == Line.Side.IsLeftOf);
        }
        [TestMethod]
        public void GetSideOfTest9()
        {
            Line line = new Line(new Vector2(-1, 1), new Vector2(0, 0));
            Assert.IsTrue(line.GetSideOf(new Vector2(0, 100)) == Line.Side.IsLeftOf);
        }
        [TestMethod]
        public void GetSideOfTest10()
        {
            Line line = new Line(new Vector2(-1, 1), new Vector2(0, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, -100)) == Line.Side.IsLeftOf);
        }
        [TestMethod]
        public void GetSideOfTest11()
        {
            Line line = new Line(new Vector2(-1, 1), new Vector2(0, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, -100)) == Line.Side.IsLeftOf);
        }
        [TestMethod]
        public void GetSideOfTest12()
        {
            Line line = new Line(new Vector2(-5, 20), new Vector2(20, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, 0)) == Line.Side.IsLeftOf);
        }
        [TestMethod]
        public void GetSideOfTest13()
        {
            Line line = new Line(new Vector2(5, 20), new Vector2(10, 25));
            Line lineCheck = new Line(new Vector2(-25, 10), new Vector2(25, 10));
            Assert.IsTrue(line.GetSideOf(lineCheck) == Line.Side.IsNeither);
        }
        [TestMethod]
        public void GetSideOfTest14()
        {
            float rot = 0;
            for (int i = 0; i < 10; i++)
            {
                Vector2 v0 = new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot));
                Vector2 v1 = new Vector2((float)Math.Cos(rot + Math.PI), (float)Math.Sin(rot + Math.PI));
                Vector2 v2 = new Vector2((float)Math.Cos(rot + Math.PI/2), (float)Math.Sin(rot + Math.PI/2));
                Line line = new Line(v0, v1);
                Assert.IsTrue(line.GetSideOf(v2) == Line.Side.IsRightOf);
            }
        }
        #endregion
        #region IsInsideFOV test
        [TestMethod]
        public void IsInsideFOVTest0()
        {
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal(null);
                p0.Transform.Rotation = (float)(i + Math.PI / 4);
                Vector2 viewPoint = new Vector2((float)Math.Cos(i + Math.PI), (float)Math.Sin(i + Math.PI));
                Vector2 lookPoint = new Vector2((float)Math.Cos(i), (float)Math.Sin(i));
                Line line = new Line(VectorExt2.Transform(p0.GetVerts(), p0.Transform.GetMatrix()));
                Assert.IsTrue(line.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest1()
        {
            float x = 100;
            float y = 100;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal(null, new Vector2(x, y));
                p0.Transform.Rotation = (float)(i + Math.PI / 4);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i + Math.PI), y + (float)Math.Sin(i + Math.PI));
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                Line line = new Line(VectorExt2.Transform(p0.GetVerts(), p0.Transform.GetMatrix()));
                Assert.IsTrue(line.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest2()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal(null, new Vector2(x, y));
                p0.Transform.Rotation = (float)(i + Math.PI / 4);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i + Math.PI)/100000, y + (float)Math.Sin(i + Math.PI)/100000);
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i)/100000, y + (float)Math.Sin(i)/100000);
                Line line = new Line(VectorExt2.Transform(p0.GetVerts(), p0.Transform.GetMatrix()));
                Assert.IsTrue(line.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest3()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal(null, new Vector2(x, y));
                p0.Transform.Rotation = (float)(i + Math.PI / 4);
                Vector2 viewPoint = new Vector2(x, y);
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i + Math.PI), y + (float)Math.Sin(i + Math.PI));
                Line line = new Line(VectorExt2.Transform(p0.GetVerts(), p0.Transform.GetMatrix()));
                Assert.IsFalse(line.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest4()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal(null, new Vector2(x, y));
                p0.Transform.Rotation = (float)(i + Math.PI / 4);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i + Math.PI), y + (float)Math.Sin(i + Math.PI));
                Vector2 lookPoint = new Vector2(x, y);
                Line line = new Line(VectorExt2.Transform(p0.GetVerts(), p0.Transform.GetMatrix()));
                Assert.IsFalse(line.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest5()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal(null, new Vector2(x, y));
                p0.Transform.Rotation = (float)(i + Math.PI / 4);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                Vector2 lookPoint = new Vector2(x, y);
                Line line = new Line(VectorExt2.Transform(p0.GetVerts(), p0.Transform.GetMatrix()));
                Assert.IsTrue(line.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest6()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal(null, new Vector2(x, y));
                p0.Transform.Rotation = (float)(i + Math.PI / 4);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i) * 2, y + (float)Math.Sin(i) * 2);
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                Line line = new Line(VectorExt2.Transform(p0.GetVerts(), p0.Transform.GetMatrix()));
                Assert.IsFalse(line.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest7()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal(null, new Vector2(x, y));
                p0.Transform.Rotation = (float)(i + Math.PI / 4);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i) * 2, y + (float)Math.Sin(i) * 2);
                Line line = new Line(VectorExt2.Transform(p0.GetVerts(), p0.Transform.GetMatrix()));
                Assert.IsFalse(line.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        #endregion
    }
}
