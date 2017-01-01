using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using Game.Common;
using OpenTK;

namespace UnitTest
{
    [TestClass]
    public class Transform2Tests
    {
        #region GetNormal test
        [TestMethod]
        public void GetNormalTest0()
        {
            Transform2 t = new Transform2();
            t.Position = new Vector2(100, -200);

            Vector2 normal = t.GetRight();
            Assert.IsTrue(normal == new Vector2(1, 0));
        }
        [TestMethod]
        public void GetNormalTest1()
        {
            Transform2 t = new Transform2();
            t.Rotation = (float)Math.PI;
            t.Position = new Vector2(100, -200);

            Vector2 normal = t.GetRight();
            Vector2 reference = new Vector2(-1, 0);
            Assert.IsTrue(Math.Abs(normal.X - reference.X) < 0.00001 && Math.Abs(normal.Y - reference.Y) < 0.00001);
        }
        [TestMethod]
        public void GetNormalTest2()
        {
            Transform2 t = new Transform2();
            t.Rotation = (float)Math.PI/4;
            t.Position = new Vector2(100, -200);

            Vector2 normal = t.GetRight();
            Vector2 reference = new Vector2((float)Math.Cos(Math.PI/4), (float)Math.Sin(Math.PI/4));
            Assert.IsTrue(Math.Abs(normal.X - reference.X) < 0.00001 && Math.Abs(normal.Y - reference.Y) < 0.00001);
        }
        [TestMethod]
        public void GetNormalTest3()
        {
            Transform2 t = new Transform2();
            t.MirrorX = true;
            t.Position = new Vector2(100, -200);

            Vector2 normal = t.GetRight();
            Vector2 reference = new Vector2(-1, 0);
            Assert.IsTrue(Math.Abs(normal.X - reference.X) < 0.00001 && Math.Abs(normal.Y - reference.Y) < 0.00001);
        }
        [TestMethod]
        public void GetNormalTest4()
        {
            Transform2 t = new Transform2();
            t.MirrorX = true;
            t.Size = -1;
            t.Position = new Vector2(100, -200);

            Vector2 normal = t.GetRight();
            Vector2 reference = new Vector2(1, 0);
            Assert.IsTrue(Math.Abs(normal.X - reference.X) < 0.00001 && Math.Abs(normal.Y - reference.Y) < 0.00001);
        }
        [TestMethod]
        public void GetNormalTest5()
        {
            Transform2 t = new Transform2();
            t.Size = -1;
            t.Position = new Vector2(100, -200);

            Vector2 normal = t.GetRight();
            Vector2 reference = new Vector2(-1, 0);
            Assert.IsTrue(Math.Abs(normal.X - reference.X) < 0.00001 && Math.Abs(normal.Y - reference.Y) < 0.00001);
        }
        [TestMethod]
        public void GetNormalTest6()
        {
            Transform2 t = new Transform2();
            t.MirrorX = true;
            t.Size = -1;
            t.Position = new Vector2(100, -200);
            t.Rotation = (float)Math.PI / 4;

            Vector2 normal = t.GetRight();
            Vector2 reference = new Vector2((float)Math.Cos(Math.PI / 4), (float)Math.Sin(Math.PI / 4));
            Assert.IsTrue(Math.Abs(normal.X - reference.X) < 0.00001 && Math.Abs(normal.Y - reference.Y) < 0.00001);
        }
        [TestMethod]
        public void GetNormalTest7()
        {
            Transform2 t = new Transform2();
            t.MirrorX = true;
            t.Position = new Vector2(100, -200);
            t.Rotation = (float)Math.PI / 4;

            Vector2 normal = t.GetRight();
            Vector2 reference = -new Vector2((float)Math.Cos(Math.PI / 4), (float)Math.Sin(Math.PI / 4));
            Assert.IsTrue(Math.Abs(normal.X - reference.X) < 0.0001 && Math.Abs(normal.Y - reference.Y) < 0.0001);
        }
        #endregion
        #region Equal tests
        [TestMethod]
        public void EqualTest0()
        {
            Transform2 transform0 = new Transform2();
            Transform2 transform1 = null;
            Assert.IsFalse(transform0.AlmostEqual(transform1));
        }
        [TestMethod]
        public void EqualTest1()
        {
            Transform2 transform0 = new Transform2(new Vector2(1,-4));
            Transform2 transform1 = new Transform2();
            Assert.IsFalse(transform0.AlmostEqual(transform1));
        }
        [TestMethod]
        public void EqualTest2()
        {
            Transform2 transform0 = new Transform2(new Vector2(1, -4), 50f, 130f);
            Transform2 transform1 = new Transform2(new Vector2(1, -4), 50f, 130f);
            Assert.IsTrue(transform0.AlmostEqual(transform1));
        }
        [TestMethod]
        public void EqualTest3()
        {
            Transform2 transform0 = new Transform2(new Vector2(1, -4), 50f, 130f);
            Transform2 transform1 = new Transform2(new Vector2(1, -4), 4f, 130f);
            Assert.IsFalse(transform0.AlmostEqual(transform1));
        }
        [TestMethod]
        public void EqualTest4()
        {
            Transform2 transform0 = new Transform2(new Vector2(1, -4), 25f, 130f);
            Transform2 transform1 = new Transform2(new Vector2(1, -1), 25f, 130f);
            Assert.IsFalse(transform0.AlmostEqual(transform1));
        }
        [TestMethod]
        public void EqualTest5()
        {
            Transform2 transform0 = new Transform2(new Vector2(1, -4), -100f, 130f);
            Transform2 transform1 = new Transform2(new Vector2(1, -4), -100f, -20f);
            Assert.IsFalse(transform0.AlmostEqual(transform1));
        }
        #endregion
        #region Inverted tests
        [TestMethod]
        public void InvertedTest0()
        {
            Transform2 t = new Transform2();
            Transform2 tInverted = t.Inverted();
            Assert.IsTrue(t.GetMatrix() == tInverted.GetMatrix().Inverted());
        }
        [TestMethod]
        public void InvertedTest1()
        {
            Transform2 t = new Transform2();
            t.SetScale(new Vector2(5, -5));
            Transform2 tInverted = t.Inverted();
            Assert.IsTrue(t.GetMatrix() == tInverted.GetMatrix().Inverted());
        }
        [TestMethod]
        public void InvertedTest2()
        {
            Transform2 t = new Transform2();
            t.Rotation = 123;
            Transform2 tInverted = t.Inverted();
            Assert.IsTrue(Matrix4Ext.AlmostEqual(t.GetMatrix(), tInverted.GetMatrix().Inverted()));
        }
        [TestMethod]
        public void InvertedTest3()
        {
            Transform2 t = new Transform2();
            t.SetScale(new Vector2(-5, -5));
            t.Rotation = (float)Math.PI/5;
            Transform2 tInverted = t.Inverted();
            Assert.IsTrue(Matrix4Ext.AlmostEqual(t.GetMatrix(), tInverted.GetMatrix().Inverted()));
        }
        [TestMethod]
        public void InvertedTest4()
        {
            Transform2 t = new Transform2();
            t.SetScale(new Vector2(5, -5));
            t.Rotation = (float)Math.PI / 3;
            Transform2 tInverted = t.Inverted();
            Assert.IsTrue(Matrix4Ext.AlmostEqual(t.GetMatrix(), tInverted.GetMatrix().Inverted()));
        }
        [TestMethod]
        public void InvertedTest5()
        {
            Transform2 t = new Transform2();
            t.SetScale(new Vector2(-5, 5));
            t.Rotation = (float)(Math.PI / 3.4);
            Transform2 tInverted = t.Inverted();
            Assert.IsTrue(Matrix4Ext.AlmostEqual(t.GetMatrix(), tInverted.GetMatrix().Inverted()));
        }
        [TestMethod]
        public void InvertedTest6()
        {
            Transform2 t = new Transform2();
            t.Position = new Vector2(2, 1);
            t.SetScale(new Vector2(5, -5));
            //t.Rotation = (float)(Math.PI / 3.4);
            Transform2 tInverted = t.Inverted();
            Assert.IsTrue(Matrix4Ext.AlmostEqual(t.GetMatrix(), tInverted.GetMatrix().Inverted()));
        }
        [TestMethod]
        public void InvertedTest7()
        {
            Transform2 t = new Transform2();
            t.Position = new Vector2(2, 1);
            t.SetScale(new Vector2(-5, -5));
            t.Rotation = (float)(Math.PI / 3.4);
            Transform2 tInverted = t.Inverted();
            Assert.IsTrue(Matrix4Ext.AlmostEqual(t.GetMatrix(), tInverted.GetMatrix().Inverted()));
        }
        [TestMethod]
        public void InvertedTest8()
        {
            Transform2 t = new Transform2();
            t.Position = new Vector2(2, 1);
            t.SetScale(new Vector2(-5, 5));
            t.Rotation = (float)(Math.PI / 3.4);
            Transform2 tInverted = t.Inverted();
            Assert.IsTrue(Matrix4Ext.AlmostEqual(t.GetMatrix(), tInverted.GetMatrix().Inverted()));
        }
        #endregion
        #region Transform tests
        [TestMethod]
        public void TransformTest0()
        {
            Transform2 t0 = new Transform2();
            Transform2 t1 = new Transform2(new Vector2(1,2), 3, 123);
            Transform2 result = t0.Transform(t1);
            Assert.IsTrue(Matrix4Ext.AlmostEqual(result.GetMatrix(), t0.GetMatrix() * t1.GetMatrix()));
        }
        [TestMethod]
        public void TransformTest1()
        {
            Transform2 t0 = new Transform2(new Vector2(1,1), -2, 3.21f);
            Transform2 t1 = new Transform2(new Vector2(-100, 2), 3, 123);
            Transform2 result = t0.Transform(t1);
            Assert.IsTrue(Matrix4Ext.AlmostEqual(result.GetMatrix(), t0.GetMatrix() * t1.GetMatrix()));
        }
        [TestMethod]
        public void TransformTest2()
        {
            Transform2 t0 = new Transform2(new Vector2(), 1, 0);
            Transform2 t1 = new Transform2(new Vector2(), 1, 2, true);
            Transform2 result = t0.Transform(t1);
            Assert.IsTrue(Matrix4Ext.AlmostEqual(result.GetMatrix(), t0.GetMatrix() * t1.GetMatrix()));
        }
        [TestMethod]
        public void TransformTest3()
        {
            Transform2 t0 = new Transform2(new Vector2(), 1, 2);
            Transform2 t1 = new Transform2(new Vector2(), 1, 0, true);
            Transform2 result = t0.Transform(t1);
            Assert.IsTrue(Matrix4Ext.AlmostEqual(result.GetMatrix(), t0.GetMatrix() * t1.GetMatrix()));
        }
        [TestMethod]
        public void TransformTest4()
        {
            Transform2 t0 = new Transform2(new Vector2(), 1, 2);
            Transform2 t1 = new Transform2(new Vector2(), 1, 3.4f, true);
            Transform2 result = t0.Transform(t1);
            Assert.IsTrue(Matrix4Ext.AlmostEqual(result.GetMatrix(), t0.GetMatrix() * t1.GetMatrix()));
        }
        [TestMethod]
        public void TransformTest5()
        {
            Transform2 t0 = new Transform2(new Vector2(), 1, 0, true);
            Transform2 t1 = new Transform2(new Vector2(), 1, 2);
            Transform2 result = t0.Transform(t1);
            Assert.IsTrue(Matrix4Ext.AlmostEqual(result.GetMatrix(), t0.GetMatrix() * t1.GetMatrix()));
        }
        [TestMethod]
        public void TransformTest6()
        {
            Transform2 t0 = new Transform2(new Vector2(), 1, 2, true);
            Transform2 t1 = new Transform2(new Vector2(), 1, 0);
            Transform2 result = t0.Transform(t1);
            Assert.IsTrue(Matrix4Ext.AlmostEqual(result.GetMatrix(), t0.GetMatrix() * t1.GetMatrix()));
        }
        [TestMethod]
        public void TransformTest7()
        {
            Transform2 t0 = new Transform2(new Vector2(), 1, 2, true);
            Transform2 t1 = new Transform2(new Vector2(), 1, 3.4f);
            Transform2 result = t0.Transform(t1);
            Assert.IsTrue(Matrix4Ext.AlmostEqual(result.GetMatrix(), t0.GetMatrix() * t1.GetMatrix()));
        }
        [TestMethod]
        public void TransformTest8()
        {
            Transform2 t0 = new Transform2(new Vector2(), 1, 2, true);
            Transform2 t1 = new Transform2(new Vector2(), 1, 3.4f, true);
            Transform2 result = t0.Transform(t1);
            Assert.IsTrue(Matrix4Ext.AlmostEqual(result.GetMatrix(), t0.GetMatrix() * t1.GetMatrix()));
        }
        #endregion
    }
}
