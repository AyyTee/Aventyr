using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;

namespace UnitTest
{
    [TestClass]
    public class FixtureUserDataTests
    {
        public Entity CreateGround(Scene scene)
        {
            Vector2[] verts = new Vector2[] {
                new Vector2(),
                new Vector2(3, 0),
                new Vector2(3, 3),
                new Vector2(2.5f, 4),
                new Vector2(0, 3),
            };
            Entity ground = ActorFactory.CreateEntityPolygon(scene, new Transform2D(new Vector2(), new Vector2(1, 1)), verts);
            ground.Name = "ground";
            return ground;
        }

        public Scene CreateSceneWithPortal()
        {
            Scene scene = new Scene();
            Entity ground = CreateGround(scene);
            scene.World.ProcessChanges();
            Fixture fixture = ground.Body.FixtureList[0];
            FixturePortal portal = new FixturePortal(scene, new FixtureEdgeCoord(fixture, 0, 0.3f));

            FixtureExt.GetUserData(fixture).ProcessChanges();
            return scene;
        }

        /*[TestMethod]
        public void PortalParentTest0()
        {
            Scene scene = CreateSceneWithPortal();
            Entity ground = scene.GetEntityByName("ground");

            Assert.IsTrue(ground.Body.FixtureList.Count == 3, "There should be three fixtures.  The original fixture and two fixtures that are a part of the FixturePortal.");
        }*/

        /*[TestMethod]
        public void PortalParentTest1()
        {
            Scene scene = CreateSceneWithPortal();
            Entity ground = scene.GetEntityByName("ground");
            int parentCount = 0;
            foreach (Fixture f in ground.Body.FixtureList)
            {
                FixtureUserData userData = FixtureExt.GetUserData(f);
                if (userData.IsPortalParentless() == false)
                {
                    parentCount++;
                }
            }
            Assert.IsTrue(parentCount == 2);
        }*/

        /*[TestMethod]
        public void PortalParentTest2()
        {
            Scene scene = CreateSceneWithPortal();
            Entity ground = scene.GetEntityByName("ground");
            FixturePortal portal = (FixturePortal)scene.PortalList[0];

            FixtureUserData userData;
            userData = FixtureExt.GetUserData(ground.Body.FixtureList[0]);
            Assert.IsTrue(userData.PortalParents[0] == null && userData.PortalParents[1] == null);
            userData = FixtureExt.GetUserData(ground.Body.FixtureList[1]);
            Assert.IsTrue(userData.IsPortalChild(portal));
            userData = FixtureExt.GetUserData(ground.Body.FixtureList[2]);
            Assert.IsTrue(userData.IsPortalChild(portal));
        }*/

        /*[TestMethod]
        public void PortalParentTest3()
        {
            Scene scene = CreateSceneWithPortal();
            Entity ground = scene.GetEntityByName("ground");
            FixturePortal portal0 = (FixturePortal)scene.PortalList[0];

            FixturePortal portal1 = new FixturePortal(scene, new FixtureEdgeCoord(ground.Body.FixtureList[0], 0, 0.6f));
            FixtureUserData userData = FixtureExt.GetUserData(ground.Body.FixtureList[0]);
            userData.ProcessChanges();

            int parentCount = 0;
            foreach (Fixture f in ground.Body.FixtureList)
            {
                userData = FixtureExt.GetUserData(f);
                if (userData.IsPortalParentless() == false)
                {
                    parentCount++;
                }
            }
            Assert.IsTrue(parentCount == 3);
        }*/

        /*[TestMethod]
        public void PortalParentTest4()
        {
            Scene scene = CreateSceneWithPortal();
            Entity ground = scene.GetEntityByName("ground");
            FixturePortal portal0 = (FixturePortal)scene.PortalList[0];

            FixturePortal portal1 = new FixturePortal(scene, new FixtureEdgeCoord(ground.Body.FixtureList[0], 0, 0.6f));
            FixtureUserData userData = FixtureExt.GetUserData(ground.Body.FixtureList[0]);
            userData.ProcessChanges();

            PolygonShape shape;
            shape = (PolygonShape)ground.Body.FixtureList[0].Shape;
            Assert.IsTrue(shape.Vertices.Count == 5);
            shape = (PolygonShape)ground.Body.FixtureList[1].Shape;
            Assert.IsTrue(shape.Vertices.Count == 3);
            shape = (PolygonShape)ground.Body.FixtureList[2].Shape;
            Assert.IsTrue(shape.Vertices.Count == 4);
            shape = (PolygonShape)ground.Body.FixtureList[3].Shape;
            Assert.IsTrue(shape.Vertices.Count == 3);
        }*/
    }
}
