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

        [TestMethod]
        public void GetWorldTransformTest0()
        {
            Transform2D parent = new Transform2D();
            Transform2D child = new Transform2D();
            child.Parent = parent;
            Assert.IsTrue(WorldEqualsTransform(child, child));
        }
        [TestMethod]
        public void GetWorldTransformTest1()
        {
            Transform2D parent = new Transform2D();
            Transform2D child = new Transform2D();
            child.Parent = parent;
            float detail = 60;
            for (int i = 0; i < detail; i++)
            {
                child.Position = new Vector2(100, 20);
                child.Rotation = (float)(Math.PI * 2 * i)/detail;
                Assert.IsTrue(WorldEqualsTransform(child, child));
            }
        }
        [TestMethod]
        public void GetWorldTransformTest2()
        {
            Transform2D parent = new Transform2D();
            Transform2D child = new Transform2D();
            child.Parent = parent;
            child.Scale = new Vector2(-1, 1);
            float detail = 60;
            for (int i = 0; i < detail; i++)
            {
                child.Position = new Vector2(100, 20);
                child.Rotation = (float)(Math.PI * 2 * i) / detail;
                Assert.IsTrue(WorldEqualsTransform(child, child));
            }
        }
        [TestMethod]
        public void GetWorldTransformTest3()
        {
            Transform2D parent = new Transform2D();
            Transform2D child = new Transform2D();
            child.Scale = new Vector2(1, -1);
            child.Parent = parent;
            float detail = 60;
            for (int i = 0; i < detail; i++)
            {
                child.Position = new Vector2(100, 20);
                child.Rotation = (float)(Math.PI * 2 * i) / detail;
                Assert.IsTrue(WorldEqualsTransform(child, child));
            }
        }
        [TestMethod]
        public void GetWorldTransformTest4()
        {
            Transform2D child = new Transform2D();
            child.Scale = new Vector2(1, -1);
            float detail = 60;
            for (int i = 0; i < detail; i++)
            {
                child.Position = new Vector2(100, 20);
                child.Rotation = (float)(Math.PI * 2 * i) / detail;
                Assert.IsTrue(WorldEqualsTransform(child, child));
            }
        }
        [TestMethod]
        public void GetWorldTransformTest5()
        {
            Transform2D parent = new Transform2D(new Vector2(100, 200), new Vector2(0.5f, 3.3f), 4.4f);
            Transform2D child = new Transform2D();
            child.Parent = parent;
            Assert.IsTrue(WorldEqualsTransform(child, parent));
        }

        private bool WorldEqualsTransform(Transform2D world, Transform2D transform)
        {
            return (world.WorldPosition == transform.Position && world.WorldRotation == transform.Rotation && world.WorldScale == transform.Scale);
        }

        [TestMethod]
        public void EqualTest0()
        {
            Transform2D transform0 = null;
            Transform2D transform1 = null;
            Assert.IsTrue(transform0 == transform1);
        }
        [TestMethod]
        public void EqualTest1()
        {
            Transform2D transform0 = new Transform2D();
            Transform2D transform1 = null;
            Assert.IsFalse(transform0 == transform1);
        }
        [TestMethod]
        public void EqualTest2()
        {
            Transform2D transform0 = null;
            Transform2D transform1 = new Transform2D();
            Assert.IsFalse(transform0 == transform1);
        }
        [TestMethod]
        public void EqualTest3()
        {
            Transform2D transform0 = new Transform2D(new Vector2(1,-4));
            Transform2D transform1 = new Transform2D();
            Assert.IsFalse(transform0 == transform1);
        }
    }
}
