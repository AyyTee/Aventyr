using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;
using FarseerPhysics.Collision.Shapes;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class ActorTests
    {
        [TestMethod]
        public void SetTransformTest0()
        {
            Scene scene = new Scene();
            Vector2[] vertices = new Vector2[] {
                new Vector2(-1, -1),
                new Vector2(1, -1),
                new Vector2(1, 1),
            };
            Actor actor = new Actor(scene, vertices);
            Transform2 t = new Transform2(new Vector2(1, 2), 2, 4.23f, true);
            actor.SetTransform(t);

            Assert.IsTrue(actor.GetTransform() == t);
        }

        [TestMethod]
        public void SetTransformTest1()
        {
            Scene scene = new Scene();
            Vector2[] vertices = new Vector2[] {
                new Vector2(-1, -1),
                new Vector2(1, -1),
                new Vector2(1, 1),
            };
            Actor actor = new Actor(scene, vertices);
            Transform2 t = new Transform2(new Vector2(1, 2), 2, 4.23f, true);
            actor.SetTransform(t);

            List<Vector2> fixture = new List<Vector2>(Vector2Ext.Transform(vertices, Matrix4.CreateScale(new Vector3(t.Scale))));
            PolygonExt.SetInterior(fixture);
            PolygonShape polygon = (PolygonShape)actor.Body.FixtureList[0].Shape;

            for (int i = 0; i < fixture.Count; i++)
            {
                Assert.IsTrue(fixture[i].X == polygon.Vertices[i].X && fixture[i].Y == polygon.Vertices[i].Y);
            }
        }
    }
}
