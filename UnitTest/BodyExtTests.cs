using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using Game.Physics;
using Game.Portals;
using OpenTK;
using static Game.BodyExt;

namespace UnitTest
{
    [TestClass]
    public class BodyExtTests
    {
        public const double Delta = 0.0005f;

        [TestMethod]
        public void GetLocalMassDataTest0()
        {
            Scene scene = new Scene();
            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(4, 1));

            MassData result = GetLocalMassData(actor.Body);

            Assert.AreEqual(actor.Mass, result.Mass);
            Assert.AreEqual(new Vector2(), result.Centroid);
        }

        [TestMethod]
        public void GetLocalMassDataTest1()
        {
            Scene scene = new Scene();
            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(4, 1));

            FloatPortal enter = new FloatPortal(scene);
            FloatPortal exit = new FloatPortal(scene);
            enter.Linked = exit;
            exit.Linked = enter;

            enter.SetTransform(new Transform2(new Vector2(1, 0)));
            exit.SetTransform(new Transform2(new Vector2(50, 0)));

            scene.Step();

            MassData result = GetLocalMassData(actor.Body);

            Assert.AreEqual(actor.Mass * 3.0f / 4.0f, result.Mass, Delta);
            Assert.IsTrue((new Vector2(-0.5f, 0) - result.Centroid).Length < Delta);
        }

        [TestMethod]
        public void GetLocalMassDataTest2()
        {
            Scene scene = new Scene();
            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(4, 1));

            FloatPortal enter = new FloatPortal(scene);
            FloatPortal exit = new FloatPortal(scene);
            enter.Linked = exit;
            exit.Linked = enter;

            enter.SetTransform(new Transform2(new Vector2(1, 0)));
            exit.SetTransform(new Transform2(new Vector2(50, 0)));

            scene.Step();

            MassData result = GetLocalMassData(GetData(actor.Body).BodyChildren[0].Body);

            Assert.AreEqual(actor.Mass / 4.0f, result.Mass, Delta);
            Assert.IsTrue((new Vector2(49.5f, 0) - result.Centroid).Length < Delta);
        }
    }
}
