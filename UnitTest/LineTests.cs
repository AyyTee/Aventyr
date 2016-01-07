using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;
using FarseerPhysics;

namespace UnitTest
{
    [TestClass]
    public class LineTests
    {
        #region Constructor tests
        [TestMethod]
        public void ConstructorTest0()
        {
            Line line = new Line();
            Assert.IsTrue(line[0] == new Vector2());
            Assert.IsTrue(line[1] == new Vector2());
        }
        #endregion
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
        #region ArrayAccessor tests
        [TestMethod]
        public void ArrayAccessorTest0()
        {
            Line line = new Line(new Vector2(1f, 5f), new Vector2(100.1f, 2f));
            Assert.IsTrue(line[0] == new Vector2(1f, 5f));
            Assert.IsTrue(line[1] == new Vector2(100.1f, 2f));
        }
        [TestMethod]
        public void ArrayAccessorTest1()
        {
            Line line = new Line(new Vector2(1f, 5f), new Vector2(100.1f, 2f));
            line[0] = new Vector2(9f, 2.2f);
            line[1] = new Vector2(99f, 92.2f);
            Assert.IsTrue(line[0] == new Vector2(9f, 2.2f));
            Assert.IsTrue(line[1] == new Vector2(99f, 92.2f));
        }
        #endregion
        #region IsInsideFOV test
        [TestMethod]
        public void IsInsideFOVTest0()
        {
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Scene scene = new Scene();
                SceneNodePlaceable node = new SceneNodePlaceable(scene);
                FloatPortal p0 = new FloatPortal(scene);
                p0.SetParent(node);
                node.SetRotation((float)(i + Math.PI / 4));
                Vector2 viewPoint = new Vector2((float)Math.Cos(i + Math.PI), (float)Math.Sin(i + Math.PI));
                Vector2 lookPoint = new Vector2((float)Math.Cos(i), (float)Math.Sin(i));
                Line line = new Line(Vector2Ext.Transform(p0.GetVerts(), p0.GetWorldTransform().GetMatrix()));
                Assert.IsTrue(line.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest1()
        {
            float x = 100;
            float y = 100;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Scene scene = new Scene();
                SceneNodePlaceable node = new SceneNodePlaceable(scene);
                node.SetPosition(new Vector2(x, y));
                node.SetRotation((float)(i + Math.PI / 4));
                FloatPortal p0 = new FloatPortal(scene);
                p0.SetParent(node);
                
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i + Math.PI), y + (float)Math.Sin(i + Math.PI));
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                Line line = new Line(Vector2Ext.Transform(p0.GetVerts(), p0.GetWorldTransform().GetMatrix()));
                Assert.IsTrue(line.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest2()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Scene scene = new Scene();
                SceneNodePlaceable node = new SceneNodePlaceable(scene);
                node.SetPosition(new Vector2(x, y));
                node.SetRotation((float)(i + Math.PI / 4));
                FloatPortal p0 = new FloatPortal(scene);
                p0.SetParent(node);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i + Math.PI)/100000, y + (float)Math.Sin(i + Math.PI)/100000);
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i)/100000, y + (float)Math.Sin(i)/100000);
                Line line = new Line(Vector2Ext.Transform(p0.GetVerts(), p0.GetWorldTransform().GetMatrix()));
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
                Scene scene = new Scene();
                FloatPortal p0 = new FloatPortal(scene);
                SceneNodePlaceable node = new SceneNodePlaceable(scene);
                node.SetPosition(new Vector2(x, y));
                node.SetRotation((float)(i + Math.PI / 4));
                p0.SetParent(node);
                Vector2 viewPoint = new Vector2(x, y);
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i + Math.PI), y + (float)Math.Sin(i + Math.PI));
                Line line = new Line(Vector2Ext.Transform(p0.GetVerts(), p0.GetWorldTransform().GetMatrix()));
                Assert.IsFalse(line.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest4()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Scene scene = new Scene();
                FloatPortal p0 = new FloatPortal(scene);
                SceneNodePlaceable node = new SceneNodePlaceable(scene);
                node.SetPosition(new Vector2(x, y));
                node.SetRotation((float)(i + Math.PI / 4));
                p0.SetParent(node);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i + Math.PI), y + (float)Math.Sin(i + Math.PI));
                Vector2 lookPoint = new Vector2(x, y);
                Line line = new Line(Vector2Ext.Transform(p0.GetVerts(), p0.GetWorldTransform().GetMatrix()));
                Assert.IsFalse(line.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest5()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Scene scene = new Scene();
                FloatPortal p0 = new FloatPortal(scene);
                SceneNodePlaceable node = new SceneNodePlaceable(scene);
                node.SetPosition(new Vector2(x, y));
                node.SetRotation((float)(i + Math.PI / 4));
                p0.SetParent(node);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                Vector2 lookPoint = new Vector2(x, y);
                Line line = new Line(Vector2Ext.Transform(p0.GetVerts(), p0.GetWorldTransform().GetMatrix()));
                Assert.IsTrue(line.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest6()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Scene scene = new Scene();
                FloatPortal p0 = new FloatPortal(scene);
                SceneNodePlaceable node = new SceneNodePlaceable(scene);
                node.SetPosition(new Vector2(x, y));
                node.SetRotation((float)(i + Math.PI / 4));
                p0.SetParent(node);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i) * 2, y + (float)Math.Sin(i) * 2);
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                Line line = new Line(Vector2Ext.Transform(p0.GetVerts(), p0.GetWorldTransform().GetMatrix()));
                Assert.IsFalse(line.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest7()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Scene scene = new Scene();
                FloatPortal p0 = new FloatPortal(scene);
                SceneNodePlaceable node = new SceneNodePlaceable(scene);
                node.SetPosition(new Vector2(x, y));
                node.SetRotation((float)(i + Math.PI / 4));
                p0.SetParent(node);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i) * 2, y + (float)Math.Sin(i) * 2);
                Line line = new Line(Vector2Ext.Transform(p0.GetVerts(), p0.GetWorldTransform().GetMatrix()));
                Assert.IsFalse(line.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest8()
        {
            float x = -2;
            float y = 2;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Vector2 viewPoint = new Vector2(x - 1, y);
                Line lookLine = new Line(new Vector2(x + 1, y), (float)i, 1f);
                Line line = new Line(new Vector2(x, y + 0.5f), new Vector2(x, y - 0.5f));
                Assert.IsTrue(line.IsInsideFOV(viewPoint, lookLine));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest9()
        {
            float x = -2;
            float y = 2;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Vector2 viewPoint = new Vector2(x - 1, y);
                Line lookLine = new Line(new Vector2(x + 1, y), (float)i, 1f);
                Line line = new Line(new Vector2(x, y + 0.5f), new Vector2(x, y - 0.5f));
                Assert.IsTrue(line.IsInsideFOV(viewPoint, lookLine));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest10()
        {
            float x = -2;
            float y = 2;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 20)
            {
                Vector2 viewPoint = new Vector2(x - 1, y);
                Line lookLine = new Line(new Vector2(x + 2, y), (float)i, 1f);
                Line line = new Line(new Vector2(x, y + 0.5f), new Vector2(x, y - 0.5f));
                Assert.IsFalse(lookLine.IsInsideFOV(viewPoint, line));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest11()
        {
            float x = -2;
            float y = 2;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 2000)
            {
                Vector2 viewPoint = new Vector2(x - 1, y);
                Line lookLine = new Line(new Vector2(x + 3, y + 0.5f), new Vector2(x + 3, y - 0.5f));
                Line line = new Line(new Vector2(x + 1, y), (float)i, 1f);
                Assert.IsTrue(line.IsInsideFOV(viewPoint, lookLine));
            }
        }
        #endregion
        #region NearestT tests
        [TestMethod]
        public void NearestTTest0()
        {
            Line line = new Line(new Vector2(), new Vector2(1, 0));
            Assert.IsTrue(line.NearestT(new Vector2(1, 5), false) == 1);
        }
        [TestMethod]
        public void NearestTTest1()
        {
            Line line = new Line(new Vector2(), new Vector2(0, 1));
            Assert.IsTrue(line.NearestT(new Vector2(-4, 2), false) == 2);
        }
        [TestMethod]
        public void NearestTTest2()
        {
            Line line = new Line(new Vector2(3.3f,-4.9f), new Vector2(-5.3f, -6.1f));
            Assert.IsTrue(Math.Abs(line.NearestT(new Vector2(-4, 2), false) - 54.5) < 0.0001f);
        }
        #endregion
        #region IntersectParametric test
        [TestMethod]
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
        }
        #endregion
    }
}
