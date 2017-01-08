using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game.Portals;
using Game;
using OpenTK;
using System.Linq;
using Game.Common;
using Game.Physics;

namespace GameTests
{
    [TestClass]
    public class SimulationStepTests
    {
        #region Step tests
        [TestMethod]
        public void StepTest0()
        {
            Scene scene = new Scene();
            Portalable p = new Portalable(scene);
            Transform2 start = new Transform2(new Vector2(1, 5), 2.3f, 3.9f);
            Transform2 velocity = Transform2.CreateVelocity(new Vector2(-3, 4), 23, 0.54f);
            p.SetTransform(start);
            p.SetVelocity(velocity);
            PortalCommon.UpdateWorldTransform(new IPortalCommon[] { p });
            SimulationStep.Step(new IPortalCommon[] { p }, new IPortal[0], 1, null);

            Assert.IsTrue(p.GetTransform().AlmostEqual(start.Add(velocity)));
        }

        /// <summary>
        /// Portal and portalable shouldn't collide so the result should be the same as in StepTest0.
        /// </summary>
        [TestMethod]
        public void StepTest1()
        {
            Scene scene = new Scene();
            Portalable p = new Portalable(scene);
            Transform2 start = new Transform2(new Vector2(1, 5), 2.3f, 3.9f);
            Transform2 velocity = Transform2.CreateVelocity(new Vector2(-3, 4), 23, 0.54f);
            p.SetTransform(start);
            p.SetVelocity(velocity);

            //Scene scene = new Scene();
            FloatPortal portal = new FloatPortal(scene);
            PortalCommon.UpdateWorldTransform(new IPortalCommon[] { p, portal });
            SimulationStep.Step(new IPortalCommon[] { p }, new IPortal[] { portal }, 1, null);

            Assert.IsTrue(p.GetTransform().AlmostEqual(start.Add(velocity)));
        }

        [TestMethod]
        public void StepTest2()
        {
            Scene scene = new Scene();
            Portalable p = new Portalable(scene);
            Transform2 start = new Transform2(new Vector2(0, 0));
            Transform2 velocity = Transform2.CreateVelocity(new Vector2(3, 0));
            p.SetTransform(start);
            p.SetVelocity(velocity);

            //Scene scene = new Scene();
            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(1, 0)));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(10, 10)));

            enter.Linked = exit;
            exit.Linked = enter;
            PortalCommon.UpdateWorldTransform(new IPortalCommon[] { p, enter,  exit });
            SimulationStep.Step(new IPortalCommon[] { p, enter, exit }, new IPortal[] { enter, exit }, 1, null);

            Assert.IsTrue(p.GetTransform().Position == new Vector2(8, 10));
        }

        [TestMethod]
        public void StepTest3()
        {
            Scene scene = new Scene();
            Portalable p = new Portalable(scene);
            Transform2 start = new Transform2(new Vector2(0, 0));
            Transform2 velocity = Transform2.CreateVelocity(new Vector2(3, 0));
            p.SetTransform(start);
            p.SetVelocity(velocity);

            //Scene scene = new Scene();
            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(1, 0)));
            enter.SetVelocity(Transform2.CreateVelocity(new Vector2(1, 0)));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(10, 10)));

            enter.Linked = exit;
            exit.Linked = enter;

            PortalCommon.UpdateWorldTransform(new IPortalCommon[] { p, enter, exit });
            SimulationStep.Step(new IPortalCommon[] { p, enter, exit }, new IPortal[] { enter, exit }, 1, null);

            /*Assert.IsTrue(p.WorldTransform.Position == new Vector2(9, 10));
            Assert.IsTrue(p.WorldVelocity.Position == new Vector2(-2, 0));*/
            Assert.IsTrue(p.GetTransform().Position == new Vector2(9, 10));
            Assert.IsTrue(p.GetVelocity().Position == new Vector2(-2, 0));
        }

        [TestMethod]
        public void StepTest4()
        {
            Scene scene = new Scene();
            Portalable p = new Portalable(scene);
            Transform2 start = new Transform2(new Vector2(0, 0));
            Transform2 velocity = Transform2.CreateVelocity(new Vector2(3, 0));
            p.SetTransform(start);
            p.SetVelocity(velocity);

            //Scene scene = new Scene();
            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(1, 0)));
            enter.SetVelocity(Transform2.CreateVelocity(new Vector2(1, 0)));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(10, 10)));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(10, 0)));

            enter.Linked = exit;
            exit.Linked = enter;

            PortalCommon.UpdateWorldTransform(new IPortalCommon[] { p, enter, exit });
            SimulationStep.Step(new IPortalCommon[] { p, enter, exit }, new IPortal[] { enter, exit }, 1, null);

            Assert.IsTrue(p.GetTransform().Position == new Vector2(19, 10));
            Assert.IsTrue(p.GetVelocity().Position == new Vector2(8, 0));
        }

        [TestMethod]
        public void StepTest5()
        {
            Scene scene = new Scene();

            Actor p = new Actor(scene, PolygonFactory.CreateRectangle(2, 2));
            Transform2 start = new Transform2(new Vector2(0, 0));
            Transform2 velocity = Transform2.CreateVelocity(new Vector2(3, 0));
            p.SetTransform(start);
            p.SetVelocity(velocity);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(1, 0)));
            enter.SetVelocity(Transform2.CreateVelocity(new Vector2(1, 0)));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(10, 10)));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(10, 0)));

            enter.Linked = exit;
            exit.Linked = enter;

            FixturePortal child = new FixturePortal(scene, p, new PolygonCoord(0, 0.5f));

            PortalCommon.UpdateWorldTransform(new IPortalCommon[] { p, enter, exit, child });
            SimulationStep.Step(scene.GetAll().OfType<IPortalCommon>(), scene.GetAll().OfType<IPortal>(), 1, null);

            Assert.IsTrue(p.GetTransform().Position == new Vector2(19, 10));
            Assert.IsTrue(p.GetVelocity().Position == new Vector2(8, 0));
        }

        [TestMethod]
        public void StepTest6()
        {
            Scene scene = new Scene();

            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(2, 2));
            actor.SetTransform(new Transform2(new Vector2(1, 1)));
            actor.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 3)));
            Entity entity = new Entity(scene);
            entity.SetParent(actor);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(1, 2), 1, (float)Math.PI/2));
            //enter.SetVelocity(Transform2.CreateVelocity(new Vector2(1, 0)));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(10, 10)));
            //exit.SetVelocity(Transform2.CreateVelocity(new Vector2(10, 0)));

            enter.Linked = exit;
            exit.Linked = enter;

            PortalCommon.UpdateWorldTransform(scene);
            SimulationStep.Step(scene.GetAll().OfType<IPortalCommon>(), scene.GetAll().OfType<IPortal>(), 1, null);

            Transform2 expected = new Transform2(new Vector2(8, 10), 1, (float)Math.PI / 2, true);
            Assert.IsTrue(actor.WorldTransform.AlmostEqual(expected));
            Assert.IsTrue(entity.WorldTransform.AlmostEqual(expected));
            Assert.IsTrue(entity.GetTransform().EqualsValue(new Transform2()));
            Assert.IsTrue(entity.GetVelocity().EqualsValue(Transform2.CreateVelocity()));
            Assert.IsTrue(actor.GetTransform().EqualsValue(actor.WorldTransform));
        }

        [TestMethod]
        public void StepTest7()
        {
            Scene scene = new Scene();

            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(2, 2));
            actor.SetTransform(new Transform2(new Vector2(1, 1)));
            actor.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 3)));
            Entity entity = new Entity(scene);
            entity.SetParent(actor);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(1, 2), 1, (float)Math.PI / 2));
            //enter.SetVelocity(Transform2.CreateVelocity(new Vector2(1, 0)));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(10, 10)));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(10, 0)));

            enter.Linked = exit;
            exit.Linked = enter;

            PortalCommon.UpdateWorldTransform(scene);
            SimulationStep.Step(scene.GetAll().OfType<IPortalCommon>(), scene.GetAll().OfType<IPortal>(), 1, null);

            Assert.IsTrue(entity.GetTransform().EqualsValue(new Transform2()));
            Assert.IsTrue(entity.GetVelocity().EqualsValue(Transform2.CreateVelocity()));

            Assert.IsTrue(actor.GetTransform().EqualsValue(actor.WorldTransform));
            Assert.IsTrue(actor.GetVelocity().EqualsValue(actor.WorldVelocity));
        }

        [TestMethod]
        public void StepTest8()
        {
            Scene scene = new Scene();

            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(2, 2));
            actor.SetTransform(new Transform2(new Vector2(1, 1)));
            Transform2 velocity = Transform2.CreateVelocity(new Vector2(0, 3));
            actor.SetVelocity(velocity);
            FixturePortal fixture = new FixturePortal(scene, actor, new PolygonCoord(0, 0.5f));

            /*FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(1, 2), 1, (float)Math.PI / 2));
            //enter.SetVelocity(Transform2.CreateVelocity(new Vector2(1, 0)));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(10, 10)));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(10, 0)));

            enter.Linked = exit;
            exit.Linked = enter;*/

            PortalCommon.UpdateWorldTransform(scene);

            Transform2 transformPrevious = fixture.WorldTransform.ShallowClone();
            Transform2 actorPrevious = actor.WorldTransform.ShallowClone();

            SimulationStep.Step(scene.GetAll().OfType<IPortalCommon>(), scene.GetAll().OfType<IPortal>(), 1, null);

            Transform2 expected = transformPrevious.Add(velocity);
            Assert.IsTrue(expected.AlmostEqual(fixture.WorldTransform));
        }

        /// <summary>
        /// Make sure that an Actor travelling through a portal attached to itself doesn't have the portal detach.
        /// </summary>
        [TestMethod]
        public void StepTest9()
        {
            Scene scene = new Scene();

            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(4, 1));
            actor.SetVelocity(Transform2.CreateVelocity(new Vector2(0.1f, 0)));

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(2, 0)));
            FixturePortal exit = new FixturePortal(scene, actor, new PolygonCoord(0, 0.5f));

            PortalCommon.UpdateWorldTransform(scene);
            SimulationStep.Step(scene.GetAll().OfType<IPortalCommon>(), scene.GetAll().OfType<IPortal>(), 1, null);

            Assert.IsTrue(PortalCommon.GetWorldTransform(exit) == exit.WorldTransform);
        }

        /*[TestMethod]
        public void StepTest10()
        {
            Scene scene = new Scene();
            scene.Gravity = new Vector2(0, -4.9f);

            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(1, 4));

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(0, -6), 1, (float)(Math.PI / 2)));
            FixturePortal exit = new FixturePortal(scene, actor, new PolygonCoord(1, 0.5f));
            Portal.SetLinked(enter, exit);

            PortalCommon.UpdateWorldTransform(scene);
            for (int i = 0; i < 500; i++)
            {
                scene.Step(1.0f / 60.0f);
            }
            

            Assert.IsTrue(PortalCommon.GetWorldTransform(exit) == exit.WorldTransform);
        }*/
        #endregion

        /// <summary>
        /// Test that as an object rotates, a child object remains in the same place relative to it and doesn't "drift".
        /// </summary>
        [TestMethod]
        public void WorldTransformMatchesLocalTransformTest0()
        {
            Scene scene = new Scene();
            NodePortalable node = new NodePortalable(scene);
            NodePortalable child = new NodePortalable(scene);
            child.SetParent(node);

            child.SetTransform(new Transform2(new Vector2(1, 0)));
            node.SetVelocity(Transform2.CreateVelocity(new Vector2(), (float)Math.PI));

            for (int i = 0; i < 60; i++)
            {
                scene.Step(1f / 60);
            }
            
            Transform2 expectedWorldTransform = child.GetTransform().Transform(node.GetTransform());
            Assert.IsTrue(expectedWorldTransform.AlmostEqual(child.GetWorldTransform(), 0.001f));
        }

        [TestMethod]
        public void PortalSelfEntryTest0()
        {
            Scene scene = new Scene();
            NodePortalable node = new NodePortalable(scene);
            FloatPortal portal = new FloatPortal(scene);
            FloatPortal portalChild = new FloatPortal(scene);
            portalChild.SetParent(node);
            portalChild.SetTransform(new Transform2(new Vector2(1, 1)));

            portal.SetTransform(new Transform2(new Vector2(0, -0.5f), 1, (float)Math.PI/2));
            Portal.SetLinked(portal, portalChild);

            node.SetVelocity(Transform2.CreateVelocity(new Vector2(0, -1)));

            scene.Step(1);

            Vector2d[] blah =
            {
                new Vector2d(),
            };

            Assert.AreEqual(node.GetWorldTransform().Rotation, Math.PI/2, 0.0001f);
        }
    }
}
