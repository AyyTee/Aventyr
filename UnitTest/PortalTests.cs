using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using Game.Common;
using OpenTK;
using Game.Portals;

namespace GameTests
{
    [TestClass]
    public class PortalTests
    {
        const double PathIntersectionDelta = 0.0001;

        [TestMethod]
        public void GetPortalTransformTest0()
        {
            Scene scene = new Scene();
            FloatPortal p0 = new FloatPortal(scene);
            p0.SetTransform(new Transform2(new Vector2(1, 2), 1, 0));
            FloatPortal p1 = new FloatPortal(scene);
            p1.SetTransform(new Transform2(new Vector2(0, 0), 1f, 0));
            PortalCommon.UpdateWorldTransform(scene);

            Transform2 t = Portal.GetLinkedTransform(p0, p1);
            Matrix4 result = t.GetMatrix();
            Matrix4 expected = Portal.GetLinkedMatrix(p0, p1);
            Assert.IsTrue(Matrix4Ext.AlmostEqual(result, expected));
        }

        [TestMethod]
        public void GetPortalTransformTest1()
        {
            Scene scene = new Scene();
            FloatPortal p0 = new FloatPortal(scene);
            p0.SetTransform(new Transform2(new Vector2(1, 2), 4, 23));
            FloatPortal p1 = new FloatPortal(scene);
            p1.SetTransform(new Transform2(new Vector2(4, -1), 1.4f, -3));
            PortalCommon.UpdateWorldTransform(scene);

            Transform2 t = Portal.GetLinkedTransform(p0, p1);
            Matrix4 result = t.GetMatrix();
            Matrix4 expected = Portal.GetLinkedMatrix(p0, p1);
            Assert.IsTrue(Matrix4Ext.AlmostEqual(result, expected));
        }

        [TestMethod]
        public void GetPortalTransformTest2()
        {
            Scene scene = new Scene();
            FloatPortal p0 = new FloatPortal(scene);
            p0.SetTransform(new Transform2(new Vector2(1, 2), 4, 23));
            FloatPortal p1 = new FloatPortal(scene);
            p1.SetTransform(new Transform2(new Vector2(4, -1), 1.4f, -3, true));
            PortalCommon.UpdateWorldTransform(scene);

            Transform2 result = Portal.GetLinkedTransform(p0, p1);
            Assert.IsTrue(Matrix4Ext.AlmostEqual(result.GetMatrix(), Portal.GetLinkedMatrix(p0, p1)));
        }

        #region PathIntersections tests
        [TestMethod]
        public void PathIntersectionsTest0()
        {
            Scene scene = new Scene();
            FloatPortal p0 = new FloatPortal(scene);
            p0.SetTransform(new Transform2(new Vector2(1, 0)));
            FloatPortal p1 = new FloatPortal(scene);
            p1.SetTransform(new Transform2(new Vector2(10, -1)));
            p0.Linked = p1;
            p1.Linked = p0;

            PortalCommon.UpdateWorldTransform(scene);

            LineF ray = new LineF(new Vector2(0, 0), new Vector2(8, -1));
            PortalPath path = new PortalPath();
            path.Enter(p0);
            var intersections = Portal.PathIntersections(path, ray);
            Assert.AreEqual(1, intersections.Length);
            Assert.AreEqual(0.5, intersections[0].First, PathIntersectionDelta);
            Assert.AreEqual(1.0 / 3, intersections[0].Last, PathIntersectionDelta);
        }

        [TestMethod]
        public void PathIntersectionsTest1()
        {
            Scene scene = new Scene();
            FloatPortal p0 = new FloatPortal(scene);
            p0.SetTransform(new Transform2(new Vector2(1, 0)));
            FloatPortal p1 = new FloatPortal(scene);
            p1.SetTransform(new Transform2(new Vector2(2, 0), 1, (float)Math.PI));

            p0.Linked = p1;
            p1.Linked = p0;

            FloatPortal p2 = new FloatPortal(scene);
            p2.SetTransform(new Transform2(new Vector2(3, 0)));
            FloatPortal p3 = new FloatPortal(scene);
            p3.SetTransform(new Transform2(new Vector2(4, 0), 1, (float)Math.PI));

            p2.Linked = p3;
            p3.Linked = p2;

            PortalCommon.UpdateWorldTransform(scene);

            LineF ray = new LineF(new Vector2(0, 0), new Vector2(5, 0));
            PortalPath path = new PortalPath();
            path.Enter(p0);
            path.Enter(p2);

            var intersections = Portal.PathIntersections(path, ray);
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(0.5, intersections[0].First, PathIntersectionDelta);
            Assert.AreEqual(1.0 / 3, intersections[0].Last, PathIntersectionDelta);

            Assert.AreEqual(0.5, intersections[1].First, PathIntersectionDelta);
            Assert.AreEqual(2.0 / 3, intersections[1].Last, PathIntersectionDelta);
        }

        [TestMethod]
        public void PathIntersectionsTest2()
        {
            Scene scene = new Scene();
            FloatPortal p0 = new FloatPortal(scene);
            p0.SetTransform(new Transform2(new Vector2(1, 0)));
            FloatPortal p1 = new FloatPortal(scene);
            p1.SetTransform(new Transform2(new Vector2(2, 0), 1, (float)Math.PI));

            p0.Linked = p1;
            p1.Linked = p0;

            FloatPortal p2 = new FloatPortal(scene);
            p2.SetTransform(new Transform2(new Vector2(3, 0)));
            FloatPortal p3 = new FloatPortal(scene);
            p3.SetTransform(new Transform2(new Vector2(6, 3), 1, (float)Math.PI/2));

            p2.Linked = p3;
            p3.Linked = p2;

            PortalCommon.UpdateWorldTransform(scene);

            LineF ray = new LineF(new Vector2(0, 0), new Vector2(6, 2));
            PortalPath path = new PortalPath();
            path.Enter(p0);
            path.Enter(p2);

            var intersections = Portal.PathIntersections(path, ray);
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(0.5, intersections[0].First, PathIntersectionDelta);
            Assert.AreEqual(1.0 / 3, intersections[0].Last, PathIntersectionDelta);

            Assert.AreEqual(0.5, intersections[1].First, PathIntersectionDelta);
            Assert.AreEqual(2.0 / 3, intersections[1].Last, PathIntersectionDelta);
        }
        #endregion

        #region Enter tests
        [TestMethod]
        public void EnterTest0()
        {
            Scene scene = new Scene();
            NodePortalable parent = new NodePortalable(scene);
            FloatPortal portal = new FloatPortal(scene);
            portal.SetTransform(new Transform2(new Vector2(5, 0)));
            portal.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            FloatPortal exit = new FloatPortal(scene);

            enter.SetTransform(new Transform2(new Vector2(2, 0)));
            exit.SetTransform(new Transform2(new Vector2(100, 5)));

            enter.Linked = exit;
            exit.Linked = enter;

            PortalCommon.UpdateWorldTransform(scene);
            Portal.Enter(enter, parent, 0.5f);
            PortalCommon.UpdateWorldTransform(scene);
            Assert.IsTrue(new Transform2(new Vector2(5, 0)).AlmostEqual(portal.WorldTransform));
        }
        #endregion

        #region SetLinked tests

        [TestMethod]
        public void SetLinkedTest0()
        {
            Scene scene = new Scene();
            FloatPortal p0 = new FloatPortal(scene);
            FloatPortal p1 = new FloatPortal(scene);
            
            Portal.SetLinked(p0, p1);

            Assert.IsTrue(p0.Linked == p1 && p1.Linked == p0);
        }

        [TestMethod]
        public void SetLinkedTest1()
        {
            Scene scene = new Scene();
            FloatPortal p0 = new FloatPortal(scene);

            Portal.SetLinked(p0, null);

            Assert.IsTrue(p0.Linked == null);
        }

        [TestMethod]
        public void SetLinkedTest2()
        {
            Scene scene = new Scene();
            FloatPortal p0 = new FloatPortal(scene);

            Portal.SetLinked(null, p0);

            Assert.IsTrue(p0.Linked == null);
        }

        [TestMethod]
        public void SetLinkedTest3()
        {
            Scene scene = new Scene();
            FloatPortal p0 = new FloatPortal(scene);
            FloatPortal p1 = new FloatPortal(scene);

            Portal.SetLinked(p0, p1);
            Portal.SetLinked(p0, null);

            Assert.IsTrue(p0.Linked == null && p1.Linked == null);
        }

        [TestMethod]
        public void SetLinkedTest4()
        {
            Scene scene = new Scene();
            FloatPortal p0 = new FloatPortal(scene);
            FloatPortal p1 = new FloatPortal(scene);
            FloatPortal p2 = new FloatPortal(scene);

            Portal.SetLinked(p0, p1);
            Portal.SetLinked(p0, p2);

            Assert.IsTrue(p0.Linked == p2 && p1.Linked == null && p2.Linked == p0);
        }
        #endregion
    }
}
