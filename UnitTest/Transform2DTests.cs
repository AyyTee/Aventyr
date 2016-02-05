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
            t.IsMirrored = true;
            t.Position = new Vector2(100, -200);

            Vector2 normal = t.GetRight();
            Vector2 reference = new Vector2(-1, 0);
            Assert.IsTrue(Math.Abs(normal.X - reference.X) < 0.00001 && Math.Abs(normal.Y - reference.Y) < 0.00001);
        }
        [TestMethod]
        public void GetNormalTest4()
        {
            Transform2 t = new Transform2();
            t.IsMirrored = true;
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
            t.IsMirrored = true;
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
            t.IsMirrored = true;
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
            Assert.IsFalse(transform0.Compare(transform1));
        }
        [TestMethod]
        public void EqualTest1()
        {
            Transform2 transform0 = new Transform2(new Vector2(1,-4));
            Transform2 transform1 = new Transform2();
            Assert.IsFalse(transform0.Compare(transform1));
        }
        [TestMethod]
        public void EqualTest2()
        {
            Transform2 transform0 = new Transform2(new Vector2(1, -4), 50f, 130f);
            Transform2 transform1 = new Transform2(new Vector2(1, -4), 50f, 130f);
            Assert.IsTrue(transform0.Compare(transform1));
        }
        [TestMethod]
        public void EqualTest3()
        {
            Transform2 transform0 = new Transform2(new Vector2(1, -4), 50f, 130f);
            Transform2 transform1 = new Transform2(new Vector2(1, -4), 4f, 130f);
            Assert.IsFalse(transform0.Compare(transform1));
        }
        [TestMethod]
        public void EqualTest4()
        {
            Transform2 transform0 = new Transform2(new Vector2(1, -4), 25f, 130f);
            Transform2 transform1 = new Transform2(new Vector2(1, -1), 25f, 130f);
            Assert.IsFalse(transform0.Compare(transform1));
        }
        [TestMethod]
        public void EqualTest5()
        {
            Transform2 transform0 = new Transform2(new Vector2(1, -4), -100f, 130f);
            Transform2 transform1 = new Transform2(new Vector2(1, -4), -100f, -20f);
            Assert.IsFalse(transform0.Compare(transform1));
        }
        #endregion
        #region ParentLoop tests
        /*[TestMethod]
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
        }*/
        #endregion
    }
}
