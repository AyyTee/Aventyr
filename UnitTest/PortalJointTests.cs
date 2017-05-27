using System;
using NUnit.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Xna = Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics.Joints;
using Game.Physics;
using OpenTK;
using Game.Portals;
using Game;
using Game.Common;

namespace GameTests
{
    [TestFixture]
    public class PortalJointTests
    {
        public const double Delta = 0.0001f;

        public void AssertPortalJoint(Body enterBody, Body exitBody, IPortal portalEnter)
        {
            Portal.Enter(portalEnter, enterBody);
            Assert.IsTrue((exitBody.Position - enterBody.Position).Length() < Delta);
            Assert.IsTrue((exitBody.LinearVelocity - enterBody.LinearVelocity).Length() < Delta);
            Assert.AreEqual(exitBody.Rotation, enterBody.Rotation, Delta);
            Assert.AreEqual(exitBody.AngularVelocity, enterBody.AngularVelocity, Delta);
        }

        [Test]
        public void PortalJointTest0()
        {
            Scene scene = new Scene();
            FloatPortal portal0 = new FloatPortal(scene);
            FloatPortal portal1 = new FloatPortal(scene);
            portal0.SetTransform(new Transform2(new Vector2(), 0, 1, true));
            portal1.SetTransform(new Transform2(new Vector2(), 0, 1));
            portal0.Linked = portal1;
            portal1.Linked = portal0;
            PortalCommon.UpdateWorldTransform(scene);

            World world = new World(new Xna.Vector2(0, 0f));
            Body body0 = Factory.CreateBox(world, new Vector2(1, 2));
            Body body1 = Factory.CreateBox(world, new Vector2(1, 2));
            body0.IgnoreCollisionWith(body1);
            PortalJoint portalJoint = Factory.CreatePortalJoint(world, body0, body1, portal0);

            body0.ApplyLinearImpulse(new Xna.Vector2(1, 0), new Xna.Vector2(0, 0));
            world.Step(1 / (float)60);

            AssertPortalJoint(body0, body1, portal0);
        }

        [Test]
        public void PortalJointTest1()
        {
            Scene scene = new Scene();
            FloatPortal portal0 = new FloatPortal(scene);
            FloatPortal portal1 = new FloatPortal(scene);
            portal0.SetTransform(new Transform2(new Vector2(), 0, 1, true));
            portal1.SetTransform(new Transform2(new Vector2(10, 0), 0, 1));
            portal0.Linked = portal1;
            portal1.Linked = portal0;
            PortalCommon.UpdateWorldTransform(scene);

            World world = new World(new Xna.Vector2(0, 0f));
            Body body0 = Factory.CreateBox(world, new Vector2(1, 2));
            Body body1 = Factory.CreateBox(world, new Vector2(1, 2));
            Portal.Enter(portal0, body1);

            PortalJoint portalJoint = Factory.CreatePortalJoint(world, body0, body1, portal0);

            world.Step(1 / (float)60);

            AssertPortalJoint(body0, body1, portal0);
        }

        [Test]
        public void PortalJointTest2()
        {
            Scene scene = new Scene();
            FloatPortal portal0 = new FloatPortal(scene);
            FloatPortal portal1 = new FloatPortal(scene);
            portal0.SetTransform(new Transform2(new Vector2(), 0, 1, true));
            portal1.SetTransform(new Transform2(new Vector2(10, 0), 3.5f, 1));
            portal0.Linked = portal1;
            portal1.Linked = portal0;
            PortalCommon.UpdateWorldTransform(scene);

            World world = new World(new Xna.Vector2(0, 0f));
            Body body0 = Factory.CreateBox(world, new Vector2(1, 2));
            Body body1 = Factory.CreateBox(world, new Vector2(1, 2));
            Portal.Enter(portal0, body1);

            PortalJoint portalJoint = Factory.CreatePortalJoint(world, body0, body1, portal0);

            world.Step(1 / (float)60);

            AssertPortalJoint(body0, body1, portal0);
        }

        [Test]
        public void PortalJointTest3()
        {
            Scene scene = new Scene();
            FloatPortal portal0 = new FloatPortal(scene);
            FloatPortal portal1 = new FloatPortal(scene);
            portal0.SetTransform(new Transform2(new Vector2(-5, 43), -3f, 1, true));
            portal1.SetTransform(new Transform2(new Vector2(10, 9.3f), 2.5f, 1));
            portal0.Linked = portal1;
            portal1.Linked = portal0;
            PortalCommon.UpdateWorldTransform(scene);

            World world = new World(new Xna.Vector2(0, 0f));
            Body body0 = Factory.CreateBox(world, new Vector2(1, 2));
            Body body1 = Factory.CreateBox(world, new Vector2(1, 2));
            Portal.Enter(portal0, body1);

            PortalJoint portalJoint = Factory.CreatePortalJoint(world, body0, body1, portal0);

            world.Step(1 / (float)60);

            AssertPortalJoint(body0, body1, portal0);
        }

        [Test]
        public void PortalJointTest4()
        {
            Scene scene = new Scene();
            FloatPortal portal0 = new FloatPortal(scene);
            FloatPortal portal1 = new FloatPortal(scene);
            portal0.SetTransform(new Transform2(new Vector2(-5, 43), -3f, 1.4f));
            portal1.SetTransform(new Transform2(new Vector2(10, 9.3f), 2.5f, -1.9f));
            portal0.Linked = portal1;
            portal1.Linked = portal0;
            PortalCommon.UpdateWorldTransform(scene);

            World world = new World(new Xna.Vector2(0, 0f));
            Body body0 = Factory.CreateBox(world, new Vector2(1, 2));
            Body body1 = Factory.CreateBox(world, new Vector2(1, 2));
            Portal.Enter(portal0, body1);

            PortalJoint portalJoint = Factory.CreatePortalJoint(world, body0, body1, portal0);

            world.Step(1 / (float)60);

            AssertPortalJoint(body0, body1, portal0);
        }

        [Test]
        public void ChangeCentroidTest0()
        {
            Scene scene = new Scene();
            FloatPortal portal0 = new FloatPortal(scene);
            FloatPortal portal1 = new FloatPortal(scene);
            portal0.SetTransform(new Transform2(new Vector2(0, 0), (float)Math.PI / 2, 1));
            portal1.SetTransform(new Transform2(new Vector2(10, 0), (float)Math.PI / 2, 2));
            Portal.SetLinked(portal0, portal1);
            PortalCommon.UpdateWorldTransform(scene);

            World world = new World(new Xna.Vector2(0, 0f));
            Body body0 = Factory.CreateBox(world, new Vector2(1, 2));
            Body body1 = Factory.CreateBox(world, new Vector2(1, 2));

            Xna.Vector2 startPos = new Xna.Vector2(0, 1);
            body0.Position = startPos;
            body1.Position = startPos;
            Portal.Enter(portal0, body1);

            PortalJoint portalJoint = Factory.CreatePortalJoint(world, body0, body1, portal0);

            for (int i = 0; i < 10; i++)
            {
                body0.LocalCenter += new Xna.Vector2(0, 0.1f);
                body1.LocalCenter += new Xna.Vector2(0, 0.1f);

                world.Step(1 / (float)60);
                Assert.IsTrue(body0.Position == startPos);
            }
        }
    }
}
