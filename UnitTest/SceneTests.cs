using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EditorLogic;
using Game;
using System.Linq;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using System.Collections.Generic;
using Game.Common;
using Game.Physics;
using OpenTK;
using Game.Portals;

namespace GameTests
{
    [TestClass]
    public class SceneTests
    {
        /// <summary>
        /// Tests to see if an actor partway into a portal stays in place when there is no gravity and the 
        /// exit portal is a different size compared to the entrance portal.
        /// </summary>
        [TestMethod]
        public void AsymmetricPortalSizeBugTest()
        {
            Scene scene = new Scene();
            scene.Gravity = new Vector2();
            FloatPortal portal0 = new FloatPortal(scene);
            FloatPortal portal1 = new FloatPortal(scene);
            portal0.SetTransform(new Transform2(new Vector2(0, 0), 1, (float)Math.PI / 2));
            portal1.SetTransform(new Transform2(new Vector2(10, 0), 2, (float)Math.PI / 2, true));
            Portal.SetLinked(portal0, portal1);
            PortalCommon.UpdateWorldTransform(scene);

            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(1, 4));
            Vector2 startPos = new Vector2(0, 1);
            actor.SetTransform(new Transform2(startPos));

            for (int i = 0; i < 10; i++)
            {
                scene.Step(1 / (float)60);
                Assert.IsTrue((actor.GetTransform().Position - startPos).Length < 0.001f);
            }
        }
    }
}
