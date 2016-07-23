using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;
using Game.Portals;

namespace UnitTest
{
    [TestClass]
    public class PortalTests
    {
        [TestMethod]
        public void GetPortalTransformTest0()
        {
            Scene scene = new Scene();
            FloatPortal p0 = new FloatPortal(scene);
            p0.SetTransform(new Transform2(new Vector2(1, 2), 4, 23));
            FloatPortal p1 = new FloatPortal(scene);
            p1.SetTransform(new Transform2(new Vector2(4, -1), 1.4f, -3));

            Transform2 result = Portal.GetLinkedTransform(p0, p1);
            Assert.IsTrue(Matrix4Ext.AlmostEqual(result.GetMatrix(), Portal.GetLinkedMatrix(p0, p1)));
        }

        [TestMethod]
        public void GetPortalTransformTest1()
        {
            Scene scene = new Scene();
            FloatPortal p0 = new FloatPortal(scene);
            p0.SetTransform(new Transform2(new Vector2(1, 2), 4, 23));
            FloatPortal p1 = new FloatPortal(scene);
            p1.SetTransform(new Transform2(new Vector2(4, -1), 1.4f, -3, true));

            Transform2 result = Portal.GetLinkedTransform(p0, p1);
            Assert.IsTrue(Matrix4Ext.AlmostEqual(result.GetMatrix(), Portal.GetLinkedMatrix(p0, p1)));
        }
    }
}
