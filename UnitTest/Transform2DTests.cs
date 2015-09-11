using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;

namespace UnitTest
{
    [TestClass]
    public class Transform2DTests
    {
        [TestMethod]
        public void GetNormalTest0()
        {
            Transform2D t = new Transform2D();
            t.Position = new Vector2(100, -200);

            Vector2 normal = t.GetNormal();
            Assert.IsTrue(normal == new Vector2(1, 0));
        }
        [TestMethod]
        public void GetNormalTest1()
        {
            Transform2D t = new Transform2D();
            t.Rotation = (float)Math.PI;
            t.Position = new Vector2(100, -200);

            Vector2 normal = t.GetNormal();
            Vector2 reference = new Vector2(-1, 0);
            Assert.IsTrue(Math.Abs(normal.X - reference.X) < 0.00001 && Math.Abs(normal.Y - reference.Y) < 0.00001);
        }
        [TestMethod]
        public void GetNormalTest2()
        {
            Transform2D t = new Transform2D();
            t.Rotation = (float)Math.PI/4;
            t.Position = new Vector2(100, -200);

            Vector2 normal = t.GetNormal();
            Vector2 reference = new Vector2((float)Math.Cos(Math.PI/4), (float)Math.Sin(Math.PI/4));
            Assert.IsTrue(Math.Abs(normal.X - reference.X) < 0.00001 && Math.Abs(normal.Y - reference.Y) < 0.00001);
        }
        [TestMethod]
        public void GetNormalTest3()
        {
            Transform2D t = new Transform2D();
            t.Scale = new Vector2(-1, 1);
            t.Position = new Vector2(100, -200);

            Vector2 normal = t.GetNormal();
            Vector2 reference = new Vector2(-1, 0);
            Assert.IsTrue(Math.Abs(normal.X - reference.X) < 0.00001 && Math.Abs(normal.Y - reference.Y) < 0.00001);
        }
        [TestMethod]
        public void GetNormalTest4()
        {
            Transform2D t = new Transform2D();
            t.Scale = new Vector2(1, -1);
            t.Position = new Vector2(100, -200);

            Vector2 normal = t.GetNormal();
            Vector2 reference = new Vector2(1, 0);
            Assert.IsTrue(Math.Abs(normal.X - reference.X) < 0.00001 && Math.Abs(normal.Y - reference.Y) < 0.00001);
        }
        [TestMethod]
        public void GetNormalTest5()
        {
            Transform2D t = new Transform2D();
            t.Scale = new Vector2(-1, -1);
            t.Position = new Vector2(100, -200);

            Vector2 normal = t.GetNormal();
            Vector2 reference = new Vector2(-1, 0);
            Assert.IsTrue(Math.Abs(normal.X - reference.X) < 0.00001 && Math.Abs(normal.Y - reference.Y) < 0.00001);
        }
        [TestMethod]
        public void GetNormalTest6()
        {
            Transform2D t = new Transform2D();
            t.Scale = new Vector2(1, -1);
            t.Position = new Vector2(100, -200);
            t.Rotation = (float)Math.PI / 4;

            Vector2 normal = t.GetNormal();
            Vector2 reference = new Vector2((float)Math.Cos(Math.PI / 4), (float)Math.Sin(Math.PI / 4));
            Assert.IsTrue(Math.Abs(normal.X - reference.X) < 0.00001 && Math.Abs(normal.Y - reference.Y) < 0.00001);
        }
        [TestMethod]
        public void GetNormalTest7()
        {
            Transform2D t = new Transform2D();
            t.Scale = new Vector2(-1, 1);
            t.Position = new Vector2(100, -200);
            t.Rotation = (float)Math.PI / 4;

            Vector2 normal = t.GetNormal();
            Vector2 reference = -new Vector2((float)Math.Cos(Math.PI / 4), (float)Math.Sin(Math.PI / 4));
            Assert.IsTrue(Math.Abs(normal.X - reference.X) < 0.0001 && Math.Abs(normal.Y - reference.Y) < 0.0001);
        }
    }
}
