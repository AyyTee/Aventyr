using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;

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
    }
}
