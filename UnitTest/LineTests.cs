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
        #region IsInsideFOV test
        [TestMethod]
        public void IsInsideFOVTest0()
        {
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal();
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
                Portal p0 = new Portal(new Vector2(x, y));
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
                Portal p0 = new Portal(new Vector2(x, y));
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
                Portal p0 = new Portal(new Vector2(x, y));
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
                Portal p0 = new Portal(new Vector2(x, y));
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
                Portal p0 = new Portal(new Vector2(x, y));
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
                Portal p0 = new Portal(new Vector2(x, y));
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
                Portal p0 = new Portal(new Vector2(x, y));
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
