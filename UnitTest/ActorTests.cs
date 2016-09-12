using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;
using FarseerPhysics.Collision.Shapes;
using System.Collections.Generic;
using Game.Portals;

namespace UnitTest
{
    [TestClass]
    public class ActorTests
    {
        public const float EQUALITY_EPSILON = 0.0001f;

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
            fixture = PolygonExt.SetNormals(fixture);
            PolygonShape polygon = (PolygonShape)actor.Body.FixtureList[0].Shape;

            for (int i = 0; i < fixture.Count; i++)
            {
                Assert.AreEqual(fixture[i].X, polygon.Vertices[i].X, EQUALITY_EPSILON);
                Assert.AreEqual(fixture[i].Y, polygon.Vertices[i].Y, EQUALITY_EPSILON);
            }
        }

        /// <summary>
        /// If local vertices are interior then the world verticies should also be interior even when the actor is mirrored.
        /// </summary>
        [TestMethod]
        public void GetWorldVerticesTest0()
        {
            Vector2[] vertices = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1)
            };
            vertices = PolygonExt.SetNormals(vertices);
            Scene scene = new Scene();
            Actor actor = new Actor(scene, vertices);
            PortalCommon.UpdateWorldTransform(scene);

            actor.SetTransform(new Transform2(new Vector2(), 1, 0, false));
            Assert.IsTrue(PolygonExt.IsInterior(actor.GetWorldVertices()));

            actor.SetTransform(new Transform2(new Vector2(), 1, 0, true));
            Assert.IsTrue(PolygonExt.IsInterior(actor.GetWorldVertices()));

            actor.SetTransform(new Transform2(new Vector2(), -1, 0, false));
            Assert.IsTrue(PolygonExt.IsInterior(actor.GetWorldVertices()));

            actor.SetTransform(new Transform2(new Vector2(), -1, 0, true));
            Assert.IsTrue(PolygonExt.IsInterior(actor.GetWorldVertices()));
        }
    }
}
