using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;
using System.Linq;
using Game.Common;
using Game.Portals;
using Game.Physics;

namespace GameTests
{
    [TestClass]
    public class FixtureUserDataTests
    {
        public Actor CreateGround(Scene scene)
        {
            Vector2[] verts = new Vector2[] {
                new Vector2(),
                new Vector2(3, 0),
                new Vector2(3, 3),
                new Vector2(2.5f, 4),
                new Vector2(0, 3),
            };
            Actor ground = Factory.CreateEntityPolygon(scene, new Transform2(), verts);
            ground.Name = "ground";
            return ground;
        }

        public Scene CreateSceneWithPortal(out Actor ground)
        {
            Scene scene = new Scene();
            ground = CreateGround(scene);
            scene.World.ProcessChanges();
            Fixture fixture = ground.Body.FixtureList[0];
            FixturePortal portal = new FixturePortal(scene, ground, new PolygonCoord(0, 0.3f));
            FloatPortal portalExit = new FloatPortal(scene);
            Portal.SetLinked(portal, portalExit);
            PortalCommon.UpdateWorldTransform(scene);
            FixtureExt.GetData(fixture).ProcessChanges();
            return scene;
        }

        [TestMethod]
        public void PortalParentTest0()
        {
            Actor ground = null;
            Scene scene = CreateSceneWithPortal(out ground);

            Assert.IsTrue(ground.Body.FixtureList.Count == 3, "There should be three fixtures.  The original fixture and two fixtures that are a part of the FixturePortal.");
        }

        [TestMethod]
        public void PortalParentTest1()
        {
            Actor ground = null;
            Scene scene = CreateSceneWithPortal(out ground);
            int parentCount = 0;
            foreach (Fixture f in ground.Body.FixtureList)
            {
                FixtureData userData = FixtureExt.GetData(f);
                if (userData.IsPortalParentless() == false)
                {
                    parentCount++;
                }
            }
            Assert.IsTrue(parentCount == 2);
        }

        [TestMethod]
        public void PortalParentTest2()
        {
            Actor ground = null;
            Scene scene = CreateSceneWithPortal(out ground);
            FixturePortal portal = scene.GetPortalList().OfType<FixturePortal>().First();

            FixtureData userData;
            userData = FixtureExt.GetData(ground.Body.FixtureList[0]);
            Assert.IsTrue(userData.PortalParents[0] == null && userData.PortalParents[1] == null);
            userData = FixtureExt.GetData(ground.Body.FixtureList[1]);
            Assert.IsTrue(userData.PartOfPortal(portal));
            userData = FixtureExt.GetData(ground.Body.FixtureList[2]);
            Assert.IsTrue(userData.PartOfPortal(portal));
        }

        [TestMethod]
        public void PortalParentTest3()
        {
            Actor ground = null;
            Scene scene = CreateSceneWithPortal(out ground);
            FixturePortal portal0 = scene.GetPortalList().OfType<FixturePortal>().First();

            FixturePortal portal1 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.6f));
            FloatPortal portal2 = new FloatPortal(scene);
            //Make sure this portal isn't sitting on top of the float portal linked to portal0.
            portal2.SetTransform(new Transform2(new Vector2(5, 0)));
            portal1.Linked = portal2;
            portal2.Linked = portal1;
            FixtureData userData = FixtureExt.GetData(ground.Body.FixtureList[0]);
            PortalCommon.UpdateWorldTransform(scene);
            userData.ProcessChanges();

            int parentCount = 0;
            foreach (Fixture f in ground.Body.FixtureList)
            {
                userData = FixtureExt.GetData(f);
                if (userData.IsPortalParentless() == false)
                {
                    parentCount++;
                }
            }
            Assert.IsTrue(parentCount == 3);
        }

        [TestMethod]
        public void PortalParentTest4()
        {
            Actor ground = null;
            Scene scene = CreateSceneWithPortal(out ground);
            FixturePortal portal0 = scene.GetPortalList().OfType<FixturePortal>().First();

            FixturePortal portal1 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.6f));
            FloatPortal portal2 = new FloatPortal(scene);
            //Make sure portal2 isn't sitting on top of the float portal linked to portal0.
            portal2.SetTransform(new Transform2(new Vector2(5, 0)));
            portal1.Linked = portal2;
            portal2.Linked = portal1;
            FixtureData userData = FixtureExt.GetData(ground.Body.FixtureList[0]);
            PortalCommon.UpdateWorldTransform(scene);
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
        }

        [TestMethod]
        public void PortalParentTest5()
        {
            Actor ground = null;
            Scene scene = CreateSceneWithPortal(out ground);
            FixturePortal portal0 = scene.GetPortalList().OfType<FixturePortal>().First();

            FixturePortal portal1 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.6f));
            FloatPortal portal2 = new FloatPortal(scene);
            //Make sure portal2 isn't sitting on top of the float portal linked to portal0.
            portal2.SetTransform(new Transform2(new Vector2(5, 0)));
            portal1.Linked = portal2;
            portal2.Linked = portal1;
            FixtureData userData = FixtureExt.GetData(ground.Body.FixtureList[0]);
            PortalCommon.UpdateWorldTransform(scene);
            userData.ProcessChanges();


            FixtureData userData0 = FixtureExt.GetData(ground.Body.FixtureList[0]);
            Assert.IsFalse(userData0.PartOfPortal(portal0));
            Assert.IsFalse(userData0.PartOfPortal(portal1));

            FixtureData userData1 = FixtureExt.GetData(ground.Body.FixtureList[1]);
            Assert.IsTrue(userData1.PartOfPortal(portal0));
            Assert.IsFalse(userData1.PartOfPortal(portal1));

            FixtureData userData2 = FixtureExt.GetData(ground.Body.FixtureList[2]);
            Assert.IsTrue(userData2.PartOfPortal(portal0));
            Assert.IsTrue(userData2.PartOfPortal(portal1));

            FixtureData userData3 = FixtureExt.GetData(ground.Body.FixtureList[3]);
            Assert.IsFalse(userData3.PartOfPortal(portal0));
            Assert.IsTrue(userData3.PartOfPortal(portal1));
        }

        [TestMethod]
        public void PortalParentTest6()
        {
            Scene scene = new Scene();
            Actor ground = CreateGround(scene);

            FixturePortal portal0 = new FixturePortal(scene, ground, new PolygonCoord(1, 0.5f));
            FixturePortal portal1 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.6f));
            portal0.Linked = portal0;
            portal1.Linked = portal1;
            FixtureData userData = FixtureExt.GetData(ground.Body.FixtureList[0]);
            PortalCommon.UpdateWorldTransform(scene);
            userData.ProcessChanges();

            Assert.IsFalse(userData.PartOfPortal(portal0));
            Assert.IsFalse(userData.PartOfPortal(portal1));

            Assert.IsTrue(ground.Body.FixtureList.Count == 5);
            for (int i = 1; i < ground.Body.FixtureList.Count; i++)
            {
                FixtureData childUserData = FixtureExt.GetData(ground.Body.FixtureList[i]);
                if (i < 3)
                {
                    Assert.IsFalse(childUserData.PartOfPortal(portal0));
                    Assert.IsTrue(childUserData.PartOfPortal(portal1));
                }
                else
                {
                    Assert.IsTrue(childUserData.PartOfPortal(portal0));
                    Assert.IsFalse(childUserData.PartOfPortal(portal1));
                }
            }
        }

        public Vector2[][] GetPortalFixtures(Actor ground)
        {
            FixtureData userData = FixtureExt.GetData(ground.Body.FixtureList[0]);
            userData.ProcessChanges();

            Vector2[][] verticeArray = new Vector2[userData.FixtureChildren.Count][];
            for (int i = 0; i < verticeArray.Length; i++)
            {
                verticeArray[i] = Vector2Ext.ToOtk(((PolygonShape)userData.FixtureChildren[i].Shape).Vertices);
            }
            return verticeArray;
        }

        public void ProcessChangesAssert(Actor ground)
        {
            FixtureData userData = FixtureExt.GetData(ground.Body.FixtureList[0]);
            userData.ProcessChanges();

            Vector2[][] verticeArray = GetPortalFixtures(ground);
            Vector2[][] verticeExpected = new Vector2[][]
            {
                new Vector2[] { new Vector2(1.3f, 1.98f), new Vector2(2f, 2f), new Vector2(1.3f, 2f) },
                new Vector2[] { new Vector2(0.3f, 1.98f), new Vector2(0.3f, 2f), new Vector2(-0.3f, 2f), new Vector2(-0.3f, 1.98f) },
                new Vector2[] { new Vector2(-1.3f, 2f), new Vector2(-2, 2), new Vector2(-1.3f, 1.98f) },
            };

            Assert.IsTrue(verticeArray.Length == verticeExpected.Length);
            for (int i = 0; i < verticeArray.Length; i++)
            {
                Assert.IsTrue(MathExt.IsIsomorphic(verticeArray[i], verticeExpected[i], (first, second) => (first - second).Length < 0.0001, true));
            }
        }

        [TestMethod]
        public void ProcessChangesTest0()
        {
            Scene scene = new Scene();
            Actor ground = new Actor(scene, PolygonFactory.CreateRectangle(4, 4));

            FixturePortal portal0 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.3f));
            FixturePortal portal1 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.7f));
            /*portal0.Linked = portal0;
            portal1.Linked = portal1;*/
            Portal.SetLinked(portal0, portal1);

            PortalCommon.UpdateWorldTransform(scene);
            ProcessChangesAssert(ground);
        }

        [TestMethod]
        public void ProcessChangesTest1()
        {
            Scene scene = new Scene();
            Actor ground = new Actor(scene, PolygonFactory.CreateRectangle(4, 4));

            FixturePortal portal0 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.3f));
            FixturePortal portal1 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.7f));
            portal0.Linked = portal0;
            portal1.Linked = portal1;

            portal0.MirrorX = true;
            PortalCommon.UpdateWorldTransform(scene);
            ProcessChangesAssert(ground);
        }

        [TestMethod]
        public void ProcessChangesTest2()
        {
            Scene scene = new Scene();
            Actor ground = new Actor(scene, PolygonFactory.CreateRectangle(4, 4));

            FixturePortal portal0 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.3f));
            FixturePortal portal1 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.7f));
            portal0.Linked = portal0;
            portal1.Linked = portal1;

            portal1.MirrorX = true;
            PortalCommon.UpdateWorldTransform(scene);
            ProcessChangesAssert(ground);
        }

        [TestMethod]
        public void ProcessChangesTest3()
        {
            Scene scene = new Scene();
            Actor ground = new Actor(scene, PolygonFactory.CreateRectangle(4, 4));

            FixturePortal portal0 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.3f));
            FixturePortal portal1 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.7f));
            portal0.Linked = portal0;
            portal1.Linked = portal1;

            portal0.MirrorX = true;
            portal0.Size = -1;
            PortalCommon.UpdateWorldTransform(scene);
            ProcessChangesAssert(ground);
        }

        [TestMethod]
        public void ProcessChangesTest4()
        {
            Scene scene = new Scene();
            Actor ground = new Actor(scene, PolygonFactory.CreateRectangle(4, 4));

            FixturePortal portal0 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.3f));
            FixturePortal portal1 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.7f));
            portal0.Linked = portal0;
            portal1.Linked = portal1;

            portal1.MirrorX = true;
            portal1.Size = -1;
            PortalCommon.UpdateWorldTransform(scene);
            ProcessChangesAssert(ground);
        }

        [TestMethod]
        public void ProcessChangesTest5()
        {
            Scene scene = new Scene();
            Actor ground = new Actor(scene, PolygonFactory.CreateRectangle(4, 4));

            FixturePortal portal0 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.3f));
            FixturePortal portal1 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.7f));
            portal0.Linked = portal0;
            portal1.Linked = portal1;

            portal0.MirrorX = true;
            portal0.Size = -1;
            portal1.MirrorX = true;
            portal1.Size = -1;
            PortalCommon.UpdateWorldTransform(scene);
            ProcessChangesAssert(ground);
        }

        [TestMethod]
        public void ProcessChangesTest6()
        {
            Scene scene = new Scene();
            Actor ground = new Actor(scene, PolygonFactory.CreateRectangle(4, 4));

            FixturePortal portal0 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.3f));
            FixturePortal portal1 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.7f));
            portal0.Linked = portal0;
            portal1.Linked = portal1;

            portal0.MirrorX = true;
            portal1.MirrorX = true;
            portal1.Size = -1;
            PortalCommon.UpdateWorldTransform(scene);
            ProcessChangesAssert(ground);
        }

        [TestMethod]
        public void ProcessChangesTest7()
        {
            Scene scene = new Scene();
            Actor ground = new Actor(scene, PolygonFactory.CreateRectangle(4, 4));

            FixturePortal portal0 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.3f));
            FixturePortal portal1 = new FixturePortal(scene, ground, new PolygonCoord(0, 0.7f));
            portal0.Linked = portal0;
            portal1.Linked = portal1;

            portal0.MirrorX = true;
            portal1.MirrorX = true;
            portal0.Size = -1;
            PortalCommon.UpdateWorldTransform(scene);
            ProcessChangesAssert(ground);
        }
    }
}
