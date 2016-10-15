using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Xna = Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics.Joints;
using Game.Physics;
using OpenTK;
using Game.Portals;
using Game;

namespace UnitTest
{
    [TestClass]
    public class PortalJointTests
    {
        public const double Delta = 0.0001f;

        public void AssertPortalJoint(Body enterBody, Body exitBody, IPortal portalEnter)
        {
            Portal.Enter(portalEnter, enterBody);
            Assert.IsTrue(Vector2Ext.ToOtk(exitBody.Position - enterBody.Position).Length < Delta);
            Assert.IsTrue(Vector2Ext.ToOtk(exitBody.LinearVelocity - enterBody.LinearVelocity).Length < Delta);
            Assert.AreEqual(exitBody.Rotation, enterBody.Rotation, Delta);
            Assert.AreEqual(exitBody.AngularVelocity, enterBody.AngularVelocity, Delta);
        }

        [TestMethod]
        public void PortalJointTest0()
        {
            Scene scene = new Scene();
            FloatPortal portal0 = new FloatPortal(scene);
            FloatPortal portal1 = new FloatPortal(scene);
            portal0.SetTransform(new Transform2(new Vector2(), 1, 0, true));
            portal1.SetTransform(new Transform2(new Vector2(), 1, 0));
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

        [TestMethod]
        public void PortalJointTest1()
        {
            Scene scene = new Scene();
            FloatPortal portal0 = new FloatPortal(scene);
            FloatPortal portal1 = new FloatPortal(scene);
            portal0.SetTransform(new Transform2(new Vector2(), 1, 0, true));
            portal1.SetTransform(new Transform2(new Vector2(10, 0), 1, 0));
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

        [TestMethod]
        public void PortalJointTest2()
        {
            Scene scene = new Scene();
            FloatPortal portal0 = new FloatPortal(scene);
            FloatPortal portal1 = new FloatPortal(scene);
            portal0.SetTransform(new Transform2(new Vector2(), 1, 0, true));
            portal1.SetTransform(new Transform2(new Vector2(10, 0), 1, 3.5f));
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

        [TestMethod]
        public void PortalJointTest3()
        {
            Scene scene = new Scene();
            FloatPortal portal0 = new FloatPortal(scene);
            FloatPortal portal1 = new FloatPortal(scene);
            portal0.SetTransform(new Transform2(new Vector2(-5, 43), 1, -3f, true));
            portal1.SetTransform(new Transform2(new Vector2(10, 9.3f), 1, 2.5f));
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

        [TestMethod]
        public void PortalJointTest4()
        {
            Scene scene = new Scene();
            FloatPortal portal0 = new FloatPortal(scene);
            FloatPortal portal1 = new FloatPortal(scene);
            portal0.SetTransform(new Transform2(new Vector2(-5, 43), 1.4f, -3f));
            portal1.SetTransform(new Transform2(new Vector2(10, 9.3f), -1.9f, 2.5f));
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
    }
}
