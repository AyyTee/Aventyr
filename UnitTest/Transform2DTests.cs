using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;

namespace UnitTest
{
    [TestClass]
    public class Transform2DTests
    {
        #region GetNormal test
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
        #endregion
        #region GetWorldTransform test
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
        [TestMethod]
        public void GetWorldTransformTest6()
        {
            Transform2D grandparent = new Transform2D();
            Transform2D parent = new Transform2D();
            Transform2D child = new Transform2D();
            child.Parent = parent;
            parent.Parent = grandparent;
            Assert.IsTrue(WorldEqualsTransform(child, new Transform2D()));
        }
        [TestMethod]
        public void GetWorldTransformTest7()
        {
            Vector2 v0 = new Vector2(1f, 2.5f);
            Vector2 v1 = new Vector2(-1.5f, 0.5f);
            Vector2 v2 = new Vector2(5f, 100f);
            Transform2D grandparent = new Transform2D(v0);
            Transform2D parent = new Transform2D(v1);
            Transform2D child = new Transform2D(v2);
            child.Parent = parent;
            parent.Parent = grandparent;
            Assert.IsTrue(WorldEqualsTransform(child, new Transform2D(v0 + v1 + v2)));
        }
        [TestMethod]
        public void GetWorldTransformTest8()
        {
            Vector2 v0 = new Vector2(1f, 2.5f);
            Vector2 v1 = new Vector2(-1.5f, 0.5f);
            Vector2 v2 = new Vector2(5f, 100f);
            Vector2 s0 = new Vector2(1.2f, 1.5f);
            Vector2 s1 = new Vector2(1000.2f, -1.5f);
            Vector2 s2 = new Vector2(0.001f, 9.5f);
            Transform2D grandparent = new Transform2D(v0, s0);
            Transform2D parent = new Transform2D(v1, s1);
            Transform2D child = new Transform2D(v2, s2);
            child.Parent = parent;
            parent.Parent = grandparent;
            Assert.IsTrue(WorldEqualsTransform(child, new Transform2D(v0 + v1 * s0 + v2 * s0 * s1, s0 * s1 * s2)));
        }
        [TestMethod]
        public void GetWorldTransformTest9()
        {
            Vector2 v0 = new Vector2(1f, 2.5f);
            Vector2 v1 = new Vector2(-1.5f, 0.5f);
            Vector2 v2 = new Vector2(5f, 100f);
            Vector2 s0 = new Vector2(1.2f, 1.5f);
            Vector2 s1 = new Vector2(1000.2f, -1.5f);
            Vector2 s2 = new Vector2(0.001f, 9.5f);
            Vector2 error = new Vector2(0.001f, 0);
            Transform2D grandparent = new Transform2D(v0, s0);
            Transform2D parent = new Transform2D(v1, s1);
            Transform2D child = new Transform2D(v2 + error, s2);
            child.Parent = parent;
            parent.Parent = grandparent;
            Assert.IsFalse(WorldEqualsTransform(child, new Transform2D(v0 + v1 * s0 + v2 * s0 * s1, s0 * s1 * s2)));
        }
        [TestMethod]
        public void GetWorldTransformTest10()
        {
            float r0 = 15f;
            float r1 = -1000f;
            float r2 = 123f;
            Vector2 s0 = new Vector2(1.2f, 1.5f);
            Vector2 s1 = new Vector2(1000.2f, -1.5f);
            Vector2 s2 = new Vector2(0.001f, 9.5f);
            Vector2 error = new Vector2(0.001f, 0);
            Transform2D grandparent = new Transform2D(new Vector2(), s0, r0);
            Transform2D parent = new Transform2D(new Vector2(), s1, r1);
            Transform2D child = new Transform2D(new Vector2(), s2, r2);
            child.Parent = parent;
            parent.Parent = grandparent;
            Assert.IsTrue(WorldEqualsTransform(child, new Transform2D(new Vector2(), s0 * s1 * s2, r0 + r1 + r2)));
        }
        #endregion
        private bool WorldEqualsTransform(Transform2D world, Transform2D transform)
        {
            return (world.WorldPosition == transform.Position && world.WorldRotation == transform.Rotation && world.WorldScale == transform.Scale);
        }
        #region GetWorldMatrix tests
        [TestMethod]
        public void GetWorldMatrixTest0()
        {
            Transform2D transform = new Transform2D(new Vector2(123f, -23f), new Vector2(0.5f, -100f), 24.4f);
            Assert.IsTrue(transform.GetWorldMatrix() == transform.GetMatrix());
        }
        [TestMethod]
        public void GetWorldMatrixTest1()
        {
            Transform2D parent = new Transform2D(new Vector2(123f, -23f), new Vector2(0.5f, -100f), 24.4f);
            Transform2D child = new Transform2D(new Vector2(-13f, -3f), new Vector2(10.5f, -10f), 2f);
            child.Parent = parent;
            Assert.IsTrue(child.GetWorldMatrix() == child.GetMatrix() * parent.GetMatrix());
        }
        [TestMethod]
        public void GetWorldMatrixTest2()
        {
            Transform2D grandparent = new Transform2D(new Vector2(12f, -23f), new Vector2(1.55f, -15f), 23.4f);
            Transform2D parent = new Transform2D(new Vector2(13f, -23f), new Vector2(1.5f, 11f), 24.4f);
            Transform2D child = new Transform2D(new Vector2(-13f, -3f), new Vector2(10.5f, -10f), 2f);
            child.Parent = parent;
            parent.Parent = grandparent;
            Matrix4 worldMatrix = child.GetWorldMatrix();
            Matrix4 referenceMatrix = child.GetMatrix() * parent.GetMatrix() * grandparent.GetMatrix();
            Assert.IsTrue(MatrixExt4.Equals(worldMatrix, referenceMatrix));
        }
        #endregion
        #region Equal tests
        [TestMethod]
        public void EqualTest0()
        {
            Transform2D transform0 = new Transform2D();
            Transform2D transform1 = null;
            Assert.IsFalse(transform0.LocalEquals(transform1));
        }
        [TestMethod]
        public void EqualTest1()
        {
            Transform2D transform0 = new Transform2D(new Vector2(1,-4));
            Transform2D transform1 = new Transform2D();
            Assert.IsFalse(transform0.LocalEquals(transform1));
        }
        [TestMethod]
        public void EqualTest2()
        {
            Transform2D transform0 = new Transform2D(new Vector2(1, -4), new Vector2(0.4f, 50f), 130f);
            Transform2D transform1 = new Transform2D(new Vector2(1, -4), new Vector2(0.4f, 50f), 130f);
            Assert.IsTrue(transform0.LocalEquals(transform1));
        }
        [TestMethod]
        public void EqualTest3()
        {
            Transform2D transform0 = new Transform2D(new Vector2(1, -4), new Vector2(0.4f, 50f), 130f);
            Transform2D transform1 = new Transform2D(new Vector2(1, -4), new Vector2(50f, 50f), 130f);
            Assert.IsFalse(transform0.LocalEquals(transform1));
        }
        [TestMethod]
        public void EqualTest4()
        {
            Transform2D transform0 = new Transform2D(new Vector2(1, -4), new Vector2(0.4f, 50f), 130f);
            Transform2D transform1 = new Transform2D(new Vector2(1, -1), new Vector2(0.4f, 50f), 130f);
            Assert.IsFalse(transform0.LocalEquals(transform1));
        }
        [TestMethod]
        public void EqualTest5()
        {
            Transform2D transform0 = new Transform2D(new Vector2(1, -4), new Vector2(0.4f, 50f), 130f);
            Transform2D transform1 = new Transform2D(new Vector2(1, -4), new Vector2(0.4f, 50f), -20f);
            Assert.IsFalse(transform0.LocalEquals(transform1));
        }
        #endregion
        #region ParentLoop tests
        [TestMethod]
        public void ParentLoopTest0()
        {
            Transform2D transform0 = new Transform2D();
            try
            { 
                transform0.Parent = transform0;
                Assert.Fail();
            }
            catch 
            { 
            }
        }
        [TestMethod]
        public void ParentLoopTest1()
        {
            Transform2D transform0 = new Transform2D();
            Transform2D transform1 = new Transform2D();
            transform0.Parent = transform1;
            try
            {
                transform1.Parent = transform0;
                Assert.Fail();
            }
            catch
            {
            }
        }
        [TestMethod]
        public void ParentLoopTest2()
        {
            Transform2D transform0 = new Transform2D();
            Transform2D transform1 = new Transform2D();
            Transform2D transform2 = new Transform2D();
            transform0.Parent = transform1;
            transform1.Parent = transform2;
            try
            {
                transform2.Parent = transform0;
                Assert.Fail();
            }
            catch
            {
            }
        }
        [TestMethod]
        public void ParentLoopTest3()
        {
            Transform2D transform0 = new Transform2D();
            Transform2D transform1 = new Transform2D();
            Transform2D transform2 = new Transform2D();
            transform0.Parent = transform1;
            transform1.Parent = transform2;
            transform0.Parent = transform2;
        }
        [TestMethod]
        public void ParentLoopTest4()
        {
            Transform2D transform0 = new Transform2D();
            Transform2D transform1 = new Transform2D();
            Transform2D transform2 = new Transform2D();
            Transform2D transform3 = new Transform2D();
            Transform2D transform4 = new Transform2D();
            transform0.Parent = transform1;
            transform0.Parent = transform2;
            transform1.Parent = transform2;
            transform3.Parent = transform2;
            transform4.Parent = transform3;
        }
        [TestMethod]
        public void ParentLoopTest5()
        {
            Transform2D transform0 = new Transform2D();
            Transform2D transform1 = new Transform2D();
            Transform2D transform2 = new Transform2D();
            Transform2D transform3 = new Transform2D();
            Transform2D transform4 = new Transform2D();
            transform0.Parent = transform1;
            transform1.Parent = transform2;
            transform2.Parent = transform3;
            transform3.Parent = transform4;
            try
            {
                transform4.Parent = transform1;
                Assert.Fail();
            }
            catch
            {
            }
        }
        #endregion
    }
}
