using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using Game.Common;
using Game.Physics;
using Game.Portals;
using OpenTK;
using static Game.Physics.BodyExt;

namespace GameTests
{
    [TestClass]
    public class BodyExtTests
    {
        public const double Delta = 0.0001f;

        [TestMethod]
        public void GetLocalMassDataTest0()
        {
            Scene scene = new Scene();
            scene.Gravity = new Vector2();
            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(4, 1));

            MassData result = GetLocalMassData(actor.Body);

            Assert.AreEqual(actor.GetMass(), result.Mass);
            Assert.AreEqual(new Vector2(), result.Centroid);
        }

        [TestMethod]
        public void GetLocalMassDataTest1()
        {
            Scene scene = new Scene();
            scene.Gravity = new Vector2();
            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(4, 1));

            FloatPortal enter = new FloatPortal(scene);
            FloatPortal exit = new FloatPortal(scene);
            Portal.SetLinked(enter, exit);

            enter.SetTransform(new Transform2(new Vector2(1, 0)));
            exit.SetTransform(new Transform2(new Vector2(50, 0)));

            scene.Step();

            MassData result = GetLocalMassData(actor.Body);

            Assert.AreEqual(actor.GetMass() * 3.0f / 4.0f, result.Mass, Delta);
            Assert.IsTrue((new Vector2(-0.5f, 0) - result.Centroid).Length < Delta);
        }

        [TestMethod]
        public void GetLocalMassDataTest2()
        {
            Scene scene = new Scene();
            scene.Gravity = new Vector2();
            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(4, 1));

            FloatPortal p0 = new FloatPortal(scene);
            FloatPortal p1 = new FloatPortal(scene);
            Portal.SetLinked(p0, p1);

            p0.SetTransform(new Transform2(new Vector2(1, 0)));
            p1.SetTransform(new Transform2(new Vector2(50, 0)));

            FloatPortal p2 = new FloatPortal(scene);
            FloatPortal p3 = new FloatPortal(scene);
            Portal.SetLinked(p2, p3);

            p2.SetTransform(new Transform2(new Vector2(-1, 0)));
            p3.SetTransform(new Transform2(new Vector2(-50, 0)));

            scene.Step();

            MassData result = GetLocalMassData(actor.Body);

            Assert.AreEqual(actor.GetMass() / 2.0f, result.Mass, Delta);
            Assert.IsTrue((new Vector2() - result.Centroid).Length < Delta);
        }

        [TestMethod]
        public void GetLocalMassDataTest3()
        {
            Scene scene = new Scene();
            scene.Gravity = new Vector2();
            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(4, 1));

            FloatPortal p0 = new FloatPortal(scene);
            FloatPortal p1 = new FloatPortal(scene);
            Portal.SetLinked(p0, p1);

            p0.SetTransform(new Transform2(new Vector2(1, 0)));
            p1.SetTransform(new Transform2(new Vector2(50, 0), 1, 0, true));

            scene.Step();

            MassData result = GetLocalMassData(actor.Body);

            Assert.AreEqual(actor.GetMass() * 3.0f / 4.0f, result.Mass, Delta);
            Assert.IsTrue((new Vector2(-0.5f, 0) - result.Centroid).Length < Delta);
        }

        [TestMethod]
        public void GetLocalMassDataTest4()
        {
            Scene scene = new Scene();
            scene.Gravity = new Vector2();
            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(4, 1));

            FloatPortal p0 = new FloatPortal(scene);
            FloatPortal p1 = new FloatPortal(scene);
            Portal.SetLinked(p0, p1);

            p0.SetTransform(new Transform2(new Vector2(1, 0)));
            p1.SetTransform(new Transform2(new Vector2(50, 0), 1, 0, true));

            scene.Step();

            MassData result = GetLocalMassData(GetData(actor.Body).BodyChildren[0].Body);

            Assert.AreEqual(actor.GetMass() / 4.0f, result.Mass, Delta);
            Assert.IsTrue((new Vector2(50.5f, 0) - result.Centroid).Length < Delta);
        }
    }
}
