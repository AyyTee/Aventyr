using System;
using NUnit.Framework;
using Game;
using OpenTK;
using FarseerPhysics.Collision.Shapes;
using System.Collections.Generic;
using Game.Portals;
using System.Linq;
using Game.Common;
using Game.Physics;
using Xna = Microsoft.Xna.Framework;

namespace GameTests
{
    [TestFixture]
    public class ActorTests
    {
        public const float EqualityEpsilon = 0.0001f;

        [Test]
        public void SetTransformTest0()
        {
            Scene scene = new Scene();
            Vector2[] vertices = new Vector2[] {
                new Vector2(-1, -1),
                new Vector2(1, -1),
                new Vector2(1, 1),
            };
            Actor actor = new Actor(scene, vertices);
            Transform2 t = new Transform2(new Vector2(1, 2), 4.23f, 2, true);
            actor.SetTransform(t);

            Assert.IsTrue(actor.GetTransform() == t);
        }

        [Test]
        public void SetTransformTest1()
        {
            Scene scene = new Scene();
            Vector2[] vertices = new Vector2[] {
                new Vector2(-1, -1),
                new Vector2(1, -1),
                new Vector2(1, 1),
            };
            Actor actor = new Actor(scene, vertices);
            Transform2 t = new Transform2(new Vector2(1, 2), 4.23f, 2, true);
            actor.SetTransform(t);

            List<Vector2> fixture = new List<Vector2>(Vector2Ex.Transform(vertices, Matrix4.CreateScale(new Vector3(t.Scale))));
            fixture = PolygonEx.SetNormals(fixture);
            PolygonShape polygon = (PolygonShape)actor.Body.FixtureList[0].Shape;

            for (int i = 0; i < fixture.Count; i++)
            {
                Assert.AreEqual(fixture[i].X, polygon.Vertices[i].X, EqualityEpsilon);
                Assert.AreEqual(fixture[i].Y, polygon.Vertices[i].Y, EqualityEpsilon);
            }
        }

        [Test]
        public void SetTransformTest2()
        {
        }

        /// <summary>
        /// If local vertices are interior then the world verticies should also be interior even when the actor is mirrored.
        /// </summary>
        [Test]
        public void GetWorldVerticesTest0()
        {
            Vector2[] vertices = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1)
            };
            vertices = PolygonEx.SetNormals(vertices);
            Scene scene = new Scene();
            Actor actor = new Actor(scene, vertices);
            PortalCommon.UpdateWorldTransform(scene);

            actor.SetTransform(new Transform2(new Vector2(), 0, 1, false));
            Assert.IsTrue(PolygonEx.IsInterior(actor.GetWorldVertices()));

            actor.SetTransform(new Transform2(new Vector2(), 0, 1, true));
            Assert.IsTrue(PolygonEx.IsInterior(actor.GetWorldVertices()));

            actor.SetTransform(new Transform2(new Vector2(), 0, -1, false));
            Assert.IsTrue(PolygonEx.IsInterior(actor.GetWorldVertices()));

            actor.SetTransform(new Transform2(new Vector2(), 0, -1, true));
            Assert.IsTrue(PolygonEx.IsInterior(actor.GetWorldVertices()));
        }

        #region GetCentroid tests
        [Test]
        public void GetCentroidTest0()
        {
            Scene scene = new Scene();
            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(0.5f, 3f));
            Vector2 centroid = actor.GetCentroid();
            foreach (BodyData data in Tree<BodyData>.GetAll(BodyEx.GetData(actor.Body)))
            {
                data.Body.LocalCenter = actor.Body.GetLocalPoint((Xna.Vector2)centroid);
            }

            //LocalCenter and centroid should be the same since the actor is on the origin with no rotation.
            Assert.IsTrue(actor.Body.LocalCenter == (Xna.Vector2)centroid);
            Assert.IsTrue(centroid == new Vector2());
        }

        [Test]
        public void GetCentroidTest1()
        {
            Scene scene = new Scene();
            scene.Gravity = new Vector2();
            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(0.5f, 3f));

            FloatPortal enter = new FloatPortal(scene);
            FloatPortal exit = new FloatPortal(scene);
            Portal.SetLinked(enter, exit);
            enter.SetTransform(new Transform2(new Vector2(0, 1), (float)(Math.PI / 2), 1));
            exit.SetTransform(new Transform2(new Vector2(5, 0)));

            scene.Step(1 / 60f);

            Vector2 centroid = actor.GetCentroid();
            foreach (BodyData data in Tree<BodyData>.GetAll(BodyEx.GetData(actor.Body)))
            {
                data.Body.LocalCenter = actor.Body.GetLocalPoint((Xna.Vector2)centroid);
            }

            Assert.IsTrue((actor.Body.LocalCenter - (Xna.Vector2)centroid).Length() < 0.0001f);
            Assert.IsTrue((centroid - new Vector2()).Length < 0.0001f);
        }

        [Test]
        public void GetCentroidTest2()
        {
            Scene scene = new Scene();
            scene.Gravity = new Vector2();
            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(0.5f, 3f));

            FloatPortal enter = new FloatPortal(scene);
            FloatPortal exit = new FloatPortal(scene);
            Portal.SetLinked(enter, exit);
            enter.SetTransform(new Transform2(new Vector2(0, 1), (float)(Math.PI / 2), 1));
            exit.SetTransform(new Transform2(new Vector2(5, 0), size: 2));
             
            scene.Step();

            Vector2 centroid = actor.GetCentroid();
            foreach (BodyData data in Tree<BodyData>.GetAll(BodyEx.GetData(actor.Body)))
            {
                data.Body.LocalCenter = actor.Body.GetLocalPoint((Xna.Vector2)centroid);
            }

            Assert.IsTrue((actor.Body.LocalCenter - (Xna.Vector2)centroid).Length() < 0.0001f);
            Assert.IsTrue((centroid - new Vector2(0, 1.6959f)).Length < 0.0001f);
        }

        [Test]
        public void GetCentroidTest3()
        {
            Scene scene = new Scene();
            scene.Gravity = new Vector2();
            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(0.5f, 3f));

            FloatPortal enter = new FloatPortal(scene);
            FloatPortal exit = new FloatPortal(scene);
            Portal.SetLinked(enter, exit);
            enter.SetTransform(new Transform2(new Vector2(0, 1), (float)(Math.PI / 2), 1, true));
            exit.SetTransform(new Transform2(new Vector2(5, 0), size: 2));

            scene.Step();

            Vector2 centroid = actor.GetCentroid();
            foreach (BodyData data in Tree<BodyData>.GetAll(BodyEx.GetData(actor.Body)))
            {
                data.Body.LocalCenter = actor.Body.GetLocalPoint((Xna.Vector2)centroid);
            }

            Assert.IsTrue((actor.Body.LocalCenter - (Xna.Vector2)centroid).Length() < 0.0001f);
            Assert.IsTrue((centroid - new Vector2(0, 1.6959f)).Length < 0.0001f);
        }

        [Test]
        public void GetCentroidTest4()
        {
            Scene scene = new Scene();
            scene.Gravity = new Vector2();
            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(0.5f, 3f));
            Vector2 offset = new Vector2(2, 5);
            actor.SetTransform(new Transform2(offset));

            FloatPortal enter = new FloatPortal(scene);
            FloatPortal exit = new FloatPortal(scene);
            Portal.SetLinked(enter, exit);
            enter.SetTransform(new Transform2(new Vector2(0, 1) + offset, (float)(Math.PI / 2), 1, true));
            exit.SetTransform(new Transform2(new Vector2(5, 0) + offset, size: 2));

            scene.Step();

            Vector2 centroid = actor.GetCentroid();
            Assert.IsTrue((centroid - new Vector2(0, 1.6959f) - offset).Length < 0.001f);
        }
        #endregion

        public Vector2[] GetVertices()
        {
            return new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
        }

        public void GetFixtureContourAssert(Actor actor)
        {
            Vector2[] worldVertices = actor.GetWorldVertices().ToArray();
            List<Vector2> fixtureVertices = Actor.GetFixtureContour(actor);
            Assert.IsTrue(worldVertices.SequenceEqual(fixtureVertices));
        }

        /// <summary>
        /// Fixture contour and world vertices should be equal if rotation and position are 0.
        /// </summary>
        [Test]
        public void GetFixtureContourTest0()
        {
            Scene scene = new Scene();
            Actor actor = new Actor(scene, GetVertices());
            PortalCommon.UpdateWorldTransform(scene);
            GetFixtureContourAssert(actor);
        }
        [Test]
        public void GetFixtureContourTest1()
        {
            Scene scene = new Scene();
            Actor actor = new Actor(scene, GetVertices());
            actor.SetTransform(new Transform2(new Vector2(), size: 2.2f));
            PortalCommon.UpdateWorldTransform(scene);
            GetFixtureContourAssert(actor);
        }
        [Test]
        public void GetFixtureContourTest2()
        {
            Scene scene = new Scene();
            Actor actor = new Actor(scene, GetVertices());
            actor.SetTransform(new Transform2(new Vector2(), size: -3.2f));
            PortalCommon.UpdateWorldTransform(scene);
            GetFixtureContourAssert(actor);
        }
        [Test]
        public void GetFixtureContourTest3()
        {
            Scene scene = new Scene();
            Actor actor = new Actor(scene, GetVertices());
            actor.SetTransform(new Transform2(new Vector2(), 0, 2.2f, true));
            PortalCommon.UpdateWorldTransform(scene);
            GetFixtureContourAssert(actor);
        }
        [Test]
        public void GetFixtureContourTest4()
        {
            Scene scene = new Scene();
            Actor actor = new Actor(scene, GetVertices());
            actor.SetTransform(new Transform2(new Vector2(), 0, -2.2f, true));
            PortalCommon.UpdateWorldTransform(scene);
            GetFixtureContourAssert(actor);
        }
    }
}
