using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using Game.Portals;
using OpenTK;
using FarseerPhysics.Dynamics;

namespace UnitTest
{
    [TestClass]
    public class PortalCommonTests
    {
        [TestMethod]
        public void UpdateWorldTransformTest0()
        {
            /*Scene scene = new Scene();
            NodePortalable parent = new NodePortalable(scene);

            FloatPortal p0 = new FloatPortal(scene);

            p0.SetParent(parent);

            FloatPortal p1 = new FloatPortal(scene);
            FloatPortal p2 = new FloatPortal(scene);

            p0.SetTransform(new Transform2(new Vector2(5, 0)));
            p1.SetTransform(new Transform2(new Vector2(-5, 0)));
            p2.SetTransform(new Transform2(new Vector2(0, 5)));

            Portal.SetLinked(p1, p2);

            PortalCommon.UpdateWorldTransform(scene);*/

            Scene scene = new Scene();
            Actor ground = CreateGround(scene);
            FloatPortal portal = new FloatPortal(scene);
            portal.Name = "enter";
            portal.SetTransform(new Transform2(new Vector2(3, 0)));
            portal.SetParent(ground);
            FloatPortal portalExit = new FloatPortal(scene);
            portalExit.Name = "exit";
            portalExit.Linked = portal;
            portal.Linked = portal;
            PortalCommon.UpdateWorldTransform(scene);

            //Assert.IsTrue()
        }

        public Actor CreateGround(Scene scene)
        {
            Vector2[] verts = new Vector2[] {
                new Vector2(),
                new Vector2(3, 0),
                new Vector2(3, 3),
                new Vector2(2.5f, 4),
                new Vector2(0, 3),
            };
            Actor ground = ActorFactory.CreateEntityPolygon(scene, new Transform2(), verts);
            ground.Name = "ground";
            return ground;
        }
    }
}
