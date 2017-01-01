using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;
using FarseerPhysics.Collision.Shapes;
using Game.Common;
using Game.Physics;
using Game.Portals;

namespace UnitTest
{
    [TestClass]
    public class FixtureExtTests
    {
        [TestMethod]
        public void GetFixtureEdgeCoordTest0()
        {
            Scene scene = new Scene();
            Vector2[] vertices = new Vector2[] {
                new Vector2(-1, -1),
                new Vector2(1, -1),
                new Vector2(1, 1),
                new Vector2(-1, 1)
            };
            Actor actor = new Actor(scene, vertices);
            PolygonCoord polyCoord = new PolygonCoord(3, 0.4f);
            FixtureCoord fixtureCoord = FixtureExt.GetFixtureEdgeCoord(actor, polyCoord);

            Assert.IsTrue(fixtureCoord.EdgeT == polyCoord.EdgeT);
            Assert.IsTrue(PolygonExt.GetTransform(vertices, polyCoord) == PolygonExt.GetTransform(fixtureCoord));
        }

        public Vector2[] GetVertices()
        {
            return new Vector2[] {
                new Vector2(-1, -1),
                new Vector2(1, -1),
                new Vector2(1, 1),
                new Vector2(-0.6f, 0.1f),
                new Vector2(-1, 1)
            };
        }

        [TestMethod]
        public void GetFixtureEdgeCoordTest1()
        {
            Scene scene = new Scene();
            Vector2[] vertices = GetVertices();
            Actor actor = new Actor(scene, vertices);
            PolygonCoord polyCoord = new PolygonCoord(4, 0.4f);
            FixtureCoord fixtureCoord = FixtureExt.GetFixtureEdgeCoord(actor, polyCoord);

            Assert.IsTrue(fixtureCoord.EdgeT == polyCoord.EdgeT);
            Assert.IsTrue(PolygonExt.GetTransform(vertices, polyCoord) == PolygonExt.GetTransform(fixtureCoord));
        }

        [TestMethod]
        public void GetFixtureEdgeCoordTest2()
        {
            Scene scene = new Scene();
            Vector2[] vertices = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
            Actor actor = new Actor(scene, vertices);
            actor.SetTransform(new Transform2(new Vector2(), 1, 0, true));
            PolygonCoord polyCoord = new PolygonCoord(0, 0f);
            FixtureCoord fixtureCoord = FixtureExt.GetFixtureEdgeCoord(actor, polyCoord);

            Transform2 expected = PolygonExt.GetTransform(actor.GetWorldVertices(), polyCoord);
            Transform2 result = PolygonExt.GetTransform(fixtureCoord);
            Assert.IsTrue(expected.AlmostEqual(result));
        }

        [TestMethod]
        public void GetFixtureEdgeCoordTest3()
        {
            Scene scene = new Scene();
            Vector2[] vertices = GetVertices();
            Actor actor = new Actor(scene, vertices);
            actor.SetTransform(new Transform2(new Vector2(), 1, 0, true));
            PolygonCoord polyCoord = new PolygonCoord(4, 0.4f);
            FixtureCoord fixtureCoord = FixtureExt.GetFixtureEdgeCoord(actor, polyCoord);

            Assert.IsTrue(PolygonExt.GetTransform(vertices, polyCoord) == PolygonExt.GetTransform(fixtureCoord));
        }

        [TestMethod]
        public void GetFixtureEdgeCoordTest4()
        {
            Scene scene = new Scene();
            Vector2[] vertices = GetVertices();
            Actor actor = new Actor(scene, vertices);
            actor.SetTransform(new Transform2(new Vector2(), -1, 0, true));
            PolygonCoord polyCoord = new PolygonCoord(4, 0.4f);
            FixtureCoord fixtureCoord = FixtureExt.GetFixtureEdgeCoord(actor, polyCoord);

            Assert.IsTrue(PolygonExt.GetTransform(vertices, polyCoord) == PolygonExt.GetTransform(fixtureCoord));
        }

        [TestMethod]
        public void GetWorldPointsTest0()
        {
            Scene scene = new Scene();
            Vector2[] vertices = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(2.2f, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
            Actor actor = new Actor(scene, vertices);
            PortalCommon.UpdateWorldTransform(scene);

            Vector2[] fixtureVertices = Vector2Ext.ToOtk(((PolygonShape)actor.Body.FixtureList[0].Shape).Vertices);
            Assert.IsTrue(MathExt.IsIsomorphic(actor.GetWorldVertices(), fixtureVertices));
        }

        [TestMethod]
        public void GetWorldPointsTest1()
        {
            Scene scene = new Scene();
            Vector2[] vertices = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(2.2f, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
            Actor actor = new Actor(scene, vertices);
            actor.SetTransform(new Transform2(new Vector2(4, 5.5f)));
            PortalCommon.UpdateWorldTransform(scene);
            scene.World.ProcessChanges();

            Vector2[] fixtureVertices = FixtureExt.GetWorldPoints(actor.Body.FixtureList[0]);
            Assert.IsTrue(MathExt.IsIsomorphic(actor.GetWorldVertices(), fixtureVertices));
        }

        [TestMethod]
        public void GetWorldPointsTest2()
        {
            Scene scene = new Scene();
            Vector2[] vertices = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(2.2f, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
            Actor actor = new Actor(scene, vertices);
            actor.SetTransform(new Transform2(new Vector2(4.2f, -5.5f), 2.2f, -2f));
            PortalCommon.UpdateWorldTransform(scene);
            scene.World.ProcessChanges();

            Vector2[] fixtureVertices = FixtureExt.GetWorldPoints(actor.Body.FixtureList[0]);
            Assert.IsTrue(MathExt.IsIsomorphic(actor.GetWorldVertices(), fixtureVertices));
        }

        [TestMethod]
        public void GetWorldPointsTest3()
        {
            Scene scene = new Scene();
            Vector2[] vertices = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(2.2f, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
            Actor actor = new Actor(scene, vertices);
            actor.SetTransform(new Transform2(new Vector2(4.2f, -5.5f), -2f, -2f, true));
            PortalCommon.UpdateWorldTransform(scene);
            scene.World.ProcessChanges();

            Vector2[] fixtureVertices = FixtureExt.GetWorldPoints(actor.Body.FixtureList[0]);
            Assert.IsTrue(MathExt.IsIsomorphic(actor.GetWorldVertices(), fixtureVertices, (item0, item1) => (item0 - item1).Length < 0.001f));
        }
    }
}
