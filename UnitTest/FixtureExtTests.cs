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
            Assert.IsTrue(PolygonExt.GetTransform(vertices, polyCoord) == fixtureCoord.GetTransform());
        }

        [TestMethod]
        public void GetFixtureEdgeCoordTest1()
        {
            Scene scene = new Scene();
            Vector2[] vertices = new Vector2[] {
                new Vector2(-1, -1),
                new Vector2(1, -1),
                new Vector2(1, 1),
                new Vector2(-0.6f, 0.1f),
                new Vector2(-1, 1)
            };
            Actor actor = new Actor(scene, vertices);
            PolygonCoord polyCoord = new PolygonCoord(4, 0.4f);
            FixtureCoord fixtureCoord = FixtureExt.GetFixtureEdgeCoord(actor, polyCoord);

            Assert.IsTrue(fixtureCoord.EdgeT == polyCoord.EdgeT);
            Assert.IsTrue(PolygonExt.GetTransform(vertices, polyCoord) == fixtureCoord.GetTransform());
        }
    }
}
