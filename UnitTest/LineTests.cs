using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;
using FarseerPhysics;
using Game.Common;
using Game.Portals;

namespace GameTests
{
    [TestClass]
    public class LineTests
    {
        #region Constructor tests
        [TestMethod]
        public void ConstructorTest0()
        {
            LineF line = new LineF();
            Assert.IsTrue(line[0] == new Vector2());
            Assert.IsTrue(line[1] == new Vector2());
        }
        #endregion
        #region GetSideOf tests
        [TestMethod]
        public void GetSideOfTest0()
        {
            LineF line = new LineF(new Vector2(0, 0), new Vector2(1, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, 0)) == Side.Left);
        }
        [TestMethod]
        public void GetSideOfTest1()
        {
            LineF line = new LineF(new Vector2(0, 0), new Vector2(1, 0));
            Assert.IsTrue(line.GetSideOf(new Vector2(0, 100)) == Side.Left);
        }
        [TestMethod]
        public void GetSideOfTest2()
        {
            LineF line = new LineF(new Vector2(0, 0), new Vector2(1, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, -100)) == Side.Left);
        }
        [TestMethod]
        public void GetSideOfTest3()
        {
            LineF line = new LineF(new Vector2(0, 0), new Vector2(0, 1));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, -100)) == Side.Left);
        }
        [TestMethod]
        public void GetSideOfTest4()
        {
            LineF line = new LineF(new Vector2(0, 0), new Vector2(0, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, -100)) == Side.Left);
        }
        [TestMethod]
        public void GetSideOfTest5()
        {
            LineF line = new LineF(new Vector2(0, 0), new Vector2(0, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, 100)) == Side.Left);
        }
        [TestMethod]
        public void GetSideOfTest6()
        {
            LineF line = new LineF(new Vector2(0, 0), new Vector2(1, 1));
            Assert.IsTrue(line.GetSideOf(new Vector2(0.5f, 100)) == Side.Left);
        }
        [TestMethod]
        public void GetSideOfTest7()
        {
            LineF line = new LineF(new Vector2(0, 0), new Vector2(-1, -1));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, 100)) == Side.Left);
        }
        [TestMethod]
        public void GetSideOfTest8()
        {
            LineF line = new LineF(new Vector2(1, -1), new Vector2(0, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, 100)) == Side.Left);
        }
        [TestMethod]
        public void GetSideOfTest9()
        {
            LineF line = new LineF(new Vector2(-1, 1), new Vector2(0, 0));
            Assert.IsTrue(line.GetSideOf(new Vector2(0, 100)) == Side.Left);
        }
        [TestMethod]
        public void GetSideOfTest10()
        {
            LineF line = new LineF(new Vector2(-1, 1), new Vector2(0, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, -100)) == Side.Left);
        }
        [TestMethod]
        public void GetSideOfTest11()
        {
            LineF line = new LineF(new Vector2(-1, 1), new Vector2(0, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, -100)) == Side.Left);
        }
        [TestMethod]
        public void GetSideOfTest12()
        {
            LineF line = new LineF(new Vector2(-5, 20), new Vector2(20, 0));
            Assert.IsFalse(line.GetSideOf(new Vector2(0, 0)) == Side.Left);
        }
        [TestMethod]
        public void GetSideOfTest13()
        {
            LineF line = new LineF(new Vector2(5, 20), new Vector2(10, 25));
            LineF lineCheck = new LineF(new Vector2(-25, 10), new Vector2(25, 10));
            Assert.IsTrue(line.GetSideOf(lineCheck) == Side.Neither);
        }
        [TestMethod]
        public void GetSideOfTest14()
        {
            float rot = 0;
            for (int i = 0; i < 10; i++)
            {
                Vector2 v0 = new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot));
                Vector2 v1 = new Vector2((float)Math.Cos(rot + Math.PI), (float)Math.Sin(rot + Math.PI));
                Vector2 v2 = new Vector2((float)Math.Cos(rot + Math.PI / 2), (float)Math.Sin(rot + Math.PI / 2));
                LineF line = new LineF(v0, v1);
                Assert.IsTrue(line.GetSideOf(v2) == Side.Right);
            }
        }
        #endregion
        #region ArrayAccessor tests
        [TestMethod]
        public void ArrayAccessorTest0()
        {
            LineF line = new LineF(new Vector2(1f, 5f), new Vector2(100.1f, 2f));
            Assert.IsTrue(line[0] == new Vector2(1f, 5f));
            Assert.IsTrue(line[1] == new Vector2(100.1f, 2f));
        }
        [TestMethod]
        public void ArrayAccessorTest1()
        {
            LineF line = new LineF(new Vector2(1f, 5f), new Vector2(100.1f, 2f));
            line[0] = new Vector2(9f, 2.2f);
            line[1] = new Vector2(99f, 92.2f);
            Assert.IsTrue(line[0] == new Vector2(9f, 2.2f));
            Assert.IsTrue(line[1] == new Vector2(99f, 92.2f));
        }
        #endregion
        #region IsInsideFOV test
        [TestMethod]
        public void IsInsideFovTest0()
        {
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Scene scene = new Scene();
                Entity node = new Entity(scene);
                FloatPortal p0 = new FloatPortal(scene);
                p0.SetParent(node);
                node.SetRotation((float)(i + Math.PI / 4));
                PortalCommon.UpdateWorldTransform(scene);
                Vector2 viewPoint = new Vector2((float)Math.Cos(i + Math.PI), (float)Math.Sin(i + Math.PI));
                Vector2 lookPoint = new Vector2((float)Math.Cos(i), (float)Math.Sin(i));
                LineF line = new LineF(Vector2Ext.Transform(Portal.Vertices, p0.WorldTransform.GetMatrix()));
                Assert.IsTrue(line.IsInsideFov(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFovTest1()
        {
            float x = 100;
            float y = 100;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Scene scene = new Scene();
                Entity node = new Entity(scene);
                node.SetPosition(new Vector2(x, y));
                node.SetRotation((float)(i + Math.PI / 4));
                FloatPortal p0 = new FloatPortal(scene);
                p0.SetParent(node);
                PortalCommon.UpdateWorldTransform(scene);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i + Math.PI), y + (float)Math.Sin(i + Math.PI));
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                LineF line = new LineF(Vector2Ext.Transform(Portal.Vertices, p0.WorldTransform.GetMatrix()));
                Assert.IsTrue(line.IsInsideFov(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFovTest2()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Scene scene = new Scene();
                Entity node = new Entity(scene);
                node.SetPosition(new Vector2(x, y));
                node.SetRotation((float)(i + Math.PI / 4));
                FloatPortal p0 = new FloatPortal(scene);
                p0.SetParent(node);
                PortalCommon.UpdateWorldTransform(scene);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i + Math.PI) / 100000, y + (float)Math.Sin(i + Math.PI) / 100000);
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i) / 100000, y + (float)Math.Sin(i) / 100000);
                LineF line = new LineF(Vector2Ext.Transform(Portal.Vertices, p0.WorldTransform.GetMatrix()));
                Assert.IsTrue(line.IsInsideFov(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFovTest3()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Scene scene = new Scene();
                FloatPortal p0 = new FloatPortal(scene);
                Entity node = new Entity(scene);
                node.SetPosition(new Vector2(x, y));
                node.SetRotation((float)(i + Math.PI / 4));
                p0.SetParent(node);
                PortalCommon.UpdateWorldTransform(scene);
                Vector2 viewPoint = new Vector2(x, y);
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i + Math.PI), y + (float)Math.Sin(i + Math.PI));
                LineF line = new LineF(Vector2Ext.Transform(Portal.Vertices, p0.WorldTransform.GetMatrix()));

                Assert.IsFalse(line.IsInsideFov(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFovTest4()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Scene scene = new Scene();
                FloatPortal p0 = new FloatPortal(scene);
                Entity node = new Entity(scene);
                node.SetPosition(new Vector2(x, y));
                node.SetRotation((float)(i + Math.PI / 4));
                p0.SetParent(node);
                PortalCommon.UpdateWorldTransform(scene);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i + Math.PI), y + (float)Math.Sin(i + Math.PI));
                Vector2 lookPoint = new Vector2(x, y);
                LineF line = new LineF(Vector2Ext.Transform(Portal.Vertices, p0.WorldTransform.GetMatrix()));

                Assert.IsFalse(line.IsInsideFov(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFovTest5()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Scene scene = new Scene();
                FloatPortal p0 = new FloatPortal(scene);
                Entity node = new Entity(scene);
                node.SetPosition(new Vector2(x, y));
                node.SetRotation((float)(i + Math.PI / 4));
                p0.SetParent(node);
                PortalCommon.UpdateWorldTransform(scene);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                Vector2 lookPoint = new Vector2(x, y);
                LineF line = new LineF(Vector2Ext.Transform(Portal.Vertices, p0.WorldTransform.GetMatrix()));
                Assert.IsTrue(line.IsInsideFov(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFovTest6()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Scene scene = new Scene();
                FloatPortal p0 = new FloatPortal(scene);
                Entity node = new Entity(scene);
                node.SetPosition(new Vector2(x, y));
                node.SetRotation((float)(i + Math.PI / 4));
                p0.SetParent(node);
                PortalCommon.UpdateWorldTransform(scene);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i) * 2, y + (float)Math.Sin(i) * 2);
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                LineF line = new LineF(Vector2Ext.Transform(Portal.Vertices, p0.WorldTransform.GetMatrix()));
                Assert.IsFalse(line.IsInsideFov(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFovTest7()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Scene scene = new Scene();
                FloatPortal p0 = new FloatPortal(scene);
                Entity node = new Entity(scene);
                node.SetPosition(new Vector2(x, y));
                node.SetRotation((float)(i + Math.PI / 4));
                p0.SetParent(node);
                PortalCommon.UpdateWorldTransform(scene);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i) * 2, y + (float)Math.Sin(i) * 2);
                LineF line = new LineF(Vector2Ext.Transform(Portal.Vertices, p0.WorldTransform.GetMatrix()));
                Assert.IsFalse(line.IsInsideFov(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFovTest8()
        {
            float x = -2;
            float y = 2;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Vector2 viewPoint = new Vector2(x - 1, y);
                LineF lookLine = new LineF(new Vector2(x + 1, y), (float)i, 1f);
                LineF line = new LineF(new Vector2(x, y + 0.5f), new Vector2(x, y - 0.5f));
                Assert.IsTrue(line.IsInsideFov(viewPoint, lookLine));
            }
        }
        [TestMethod]
        public void IsInsideFovTest9()
        {
            float x = -2;
            float y = 2;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Vector2 viewPoint = new Vector2(x - 1, y);
                LineF lookLine = new LineF(new Vector2(x + 1, y), (float)i, 1f);
                LineF line = new LineF(new Vector2(x, y + 0.5f), new Vector2(x, y - 0.5f));
                Assert.IsTrue(line.IsInsideFov(viewPoint, lookLine));
            }
        }
        [TestMethod]
        public void IsInsideFovTest10()
        {
            float x = -2;
            float y = 2;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Vector2 viewPoint = new Vector2(x - 1, y);
                LineF lookLine = new LineF(new Vector2(x + 2, y), (float)i, 1f);
                LineF line = new LineF(new Vector2(x, y + 0.5f), new Vector2(x, y - 0.5f));
                Assert.IsFalse(lookLine.IsInsideFov(viewPoint, line));
            }
        }
        [TestMethod]
        public void IsInsideFovTest11()
        {
            float x = -2;
            float y = 2;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 2000)
            {
                Vector2 viewPoint = new Vector2(x - 1, y);
                LineF lookLine = new LineF(new Vector2(x + 3, y + 0.5f), new Vector2(x + 3, y - 0.5f));
                LineF line = new LineF(new Vector2(x + 1, y), (float)i, 1f);
                Assert.IsTrue(line.IsInsideFov(viewPoint, lookLine));
            }
        }
        #endregion
        #region NearestT tests
        [TestMethod]
        public void NearestTTest0()
        {
            LineF line = new LineF(new Vector2(), new Vector2(1, 0));
            Assert.IsTrue(line.NearestT(new Vector2(1, 5), false) == 1);
        }
        [TestMethod]
        public void NearestTTest1()
        {
            LineF line = new LineF(new Vector2(), new Vector2(0, 1));
            Assert.IsTrue(line.NearestT(new Vector2(-4, 2), false) == 2);
        }
        [TestMethod]
        public void NearestTTest2()
        {
            LineF line = new LineF(new Vector2(3.3f, -4.9f), new Vector2(-5.3f, -6.1f));
            Assert.IsTrue(line.NearestT(new Vector2(-4, 2), false) == 0.722811639f);
        }
        #endregion
        #region Intersect tests
        [TestMethod]
        public void IntersectTest0()
        {
            LineF line0 = new LineF(new Vector2(), new Vector2(0, 1f));
            LineF line1 = new LineF(new Vector2(-0.5f, 0.6f), new Vector2(0.5f, 0.6f));
            IntersectCoord intersect = MathExt.LineLineIntersect(line0, line1, false);

            IntersectCoord comparison = new IntersectCoord(new Vector2d(0, 0.6), 0.6, 0.5);
            Assert.IsTrue(intersect.AlmostEqual(comparison));
        }

        [TestMethod]
        public void IntersectTest1()
        {
            LineF line0 = new LineF(new Vector2(), new Vector2(0, 1f));
            LineF line1 = new LineF(new Vector2(-0.5f, 1f), new Vector2(0.5f, 1f));
            IntersectCoord intersect = MathExt.LineLineIntersect(line0, line1, false);

            IntersectCoord comparison = new IntersectCoord(new Vector2d(0, 1), 1, 0.5);
            Assert.IsTrue(intersect.AlmostEqual(comparison));
        }

        [TestMethod]
        public void IntersectTest2()
        {
            LineF line0 = new LineF(new Vector2(), new Vector2(0, -1f));
            LineF line1 = new LineF(new Vector2(-0.5f, -1f), new Vector2(0.5f, -1f));
            IntersectCoord intersect = MathExt.LineLineIntersect(line0, line1, false);

            IntersectCoord comparison = new IntersectCoord(new Vector2d(0, -1), 1, 0.5);
            Assert.IsTrue(intersect.AlmostEqual(comparison));
        }

        [TestMethod]
        public void IntersectTest3()
        {
            LineF line0 = new LineF(new Vector2(), new Vector2(0, -1000000f));
            LineF line1 = new LineF(new Vector2(-0.5f, -1000000), new Vector2(0.5f, -1000000f));
            IntersectCoord intersect = MathExt.LineLineIntersect(line0, line1, false);

            IntersectCoord comparison = new IntersectCoord(new Vector2d(0, -1000000), 1, 0.5);
            Assert.IsTrue(intersect.AlmostEqual(comparison));
        }

        [TestMethod]
        public void IntersectTest4()
        {
            LineF line0 = new LineF(new Vector2(), new Vector2(0, -1f));
            LineF line1 = new LineF(new Vector2(-0.5f, -1f), new Vector2(0.5f, -1f));
            IntersectCoord intersect = MathExt.LineLineIntersect(line0, line1, true);

            IntersectCoord comparison = new IntersectCoord(new Vector2d(0, -1), 1, 0.5);
            Assert.IsTrue(intersect.AlmostEqual(comparison));
        }

        [TestMethod]
        public void IntersectTest5()
        {
            LineF line0 = new LineF(new Vector2(), new Vector2(0, -1000000f));
            LineF line1 = new LineF(new Vector2(-0.5f, -1000000), new Vector2(0.5f, -1000000f));
            IntersectCoord intersect = MathExt.LineLineIntersect(line0, line1, true);

            IntersectCoord comparison = new IntersectCoord(new Vector2d(0, -1000000), 1, 0.5);
            Assert.IsTrue(intersect.AlmostEqual(comparison));
        }

        [TestMethod]
        public void IntersectTest6()
        {
            LineF line0 = new LineF(new Vector2(1f, 1f), new Vector2(2f, 2f));
            LineF line1 = new LineF(new Vector2(1.5f, 3f), new Vector2(1.5f, -1.5f));
            IntersectCoord intersect = MathExt.LineLineIntersect(line0, line1, false);

            IntersectCoord comparison = new IntersectCoord(new Vector2d(1.5, 1.5), 0.5, 1.0 / 3.0);
            Assert.IsTrue(intersect.AlmostEqual(comparison));
        }

        [TestMethod]
        public void IntersectTest7()
        {
            LineF line0 = new LineF(new Vector2(1f, 1f), new Vector2(2f, 2f));
            LineF line1 = new LineF(new Vector2(3f, 9f), new Vector2(12f, -6f));
            IntersectCoord intersect = MathExt.LineLineIntersect(line0, line1, true);

            Assert.IsTrue(intersect == null);
        }

        [TestMethod]
        public void IntersectTest8()
        {
            LineF line0 = new LineF(new Vector2(1f, 1f), new Vector2(2f, 2f));
            LineF line1 = new LineF(new Vector2(3f, 9f), new Vector2(12f, -6f));
            IntersectCoord intersect = MathExt.LineLineIntersect(line0, line1, false);

            IntersectCoord comparison = new IntersectCoord(new Vector2d(5.25, 5.25), 4.25, 0.25);
            Assert.IsTrue(intersect.AlmostEqual(comparison));
        }
        #endregion
        #region IntersectParametric tests
        /*[TestMethod]
        public void IntersectParametricTest0()
        {
            Line line = new Line(new Vector2(), new Vector2(0, 1));
            Line pointMotion = new Line(new Vector2(-1, 0.5f), new Vector2(2, 0.5f));
            Vector2 velocity = new Vector2(1, 0);
            IntersectPoint intersect = line.IntersectsParametric(velocity, 0, pointMotion, 1);
            Assert.IsTrue(intersect.Exists);
            Assert.IsTrue(intersect.TFirst >= 0 && intersect.TFirst <= 1);
        }
        [TestMethod]
        public void IntersectParametricTest1()
        {
            Line line = new Line(new Vector2(), new Vector2(0, 1));
            Line pointMotion = new Line(new Vector2(-1, 0.5f), new Vector2(2, 0.5f));
            Vector2 velocity = new Vector2(1, 0);
            IntersectPoint intersect = line.IntersectsParametric(velocity, 0, pointMotion, 10);
            Assert.IsTrue(intersect.Exists);
            Assert.IsTrue(intersect.TFirst >= 0 && intersect.TFirst <= 1);
        }
        [TestMethod]
        public void IntersectParametricTest2()
        {
            Line line = new Line(new Vector2(), new Vector2(0, 1));
            Line pointMotion = new Line(new Vector2(2, 0.5f), new Vector2(-1, 0.5f));
            Vector2 velocity = new Vector2(1, 0);
            IntersectPoint intersect = line.IntersectsParametric(velocity, 0, pointMotion, 10);
            Assert.IsTrue(intersect.Exists);
            Assert.IsTrue(intersect.TFirst >= 0 && intersect.TFirst <= 1);
        }
        [TestMethod]
        public void IntersectParametricTest3()
        {
            Line line = new Line(new Vector2(), new Vector2(0, 1));
            Line pointMotion = new Line(new Vector2(0.6f, 0.4f), new Vector2(0.6f, 0.4f));
            Vector2 velocity = new Vector2(1, 0);
            IntersectPoint intersect = line.IntersectsParametric(velocity, 0, pointMotion, 1);
            Assert.IsTrue(intersect.Exists);
            Assert.IsTrue(intersect.TFirst >= 0 && intersect.TFirst <= 1);
        }*/
        #endregion
    }
}
