using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Game.Portals;
using OpenTK;
//using Poly2Tri.Triangulation.Polygon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xna = Microsoft.Xna.Framework;

namespace Game.Physics
{
    public static class Factory
    {
        public static Actor CreateEntityBox(Scene scene, Transform2 transform)
        {
            Debug.Assert(scene != null);
            Entity entity = new Entity(scene);
            return CreateEntityBox(entity, transform);
        }

        private static Actor CreateEntityBox(Entity entity, Transform2 transform)
        {
            entity.IsPortalable = true;
            entity.AddModel(ModelFactory.CreatePlane(transform.Scale));

            Vector2[] vertices = new Vector2[] {
                new Vector2(-0.5f, -0.5f),
                new Vector2(0.5f, -0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-0.5f, 0.5f)
            };
            Actor actor = new Actor(entity.Scene, vertices, transform);
            actor.Body.BodyType = BodyType.Dynamic;
            entity.SetParent(actor);

            Transform2 t = new Transform2();
            t.Position = transform.Position;
            t.Rotation = transform.Rotation;
            actor.SetTransform(t);
            return actor;
        }

        public static Body CreateBox(World world, Vector2 scale)
        {
            Debug.Assert(world != null);
            Body body = BodyExt.CreateBody(world);
            CreateBox(body, scale);
            return body;
        }

        public static void CreateBox(Body body, Vector2 scale)
        {
            Vector2[] vertices = new Vector2[] {
                new Vector2(-0.5f, -0.5f) * scale,
                new Vector2(0.5f, -0.5f) * scale,
                new Vector2(0.5f, 0.5f) * scale,
                new Vector2(-0.5f, 0.5f) * scale
            };
            CreatePolygon(body, new Transform2(), vertices);
        }

        public static Actor CreateEntityPolygon(Scene scene, Vector2 position, IList<Vector2> vertices)
        {
            return CreateEntityPolygon(scene, new Transform2(position), vertices);
        }

        public static Actor CreateEntityPolygon(Scene scene, Transform2 transform, IList<Vector2> vertices)
        {
            Debug.Assert(scene != null);
            Entity entity = new Entity(scene);
            return CreateEntityPolygon(entity, transform, vertices);
        }

        private static Actor CreateEntityPolygon(Entity entity, Transform2 transform, IList<Vector2> vertices)
        {
            Debug.Assert(entity != null);
            Debug.Assert(transform != null);
            Debug.Assert(vertices != null && vertices.Count >= 3);
            Body body = CreatePolygon(entity.Scene.World, transform, vertices);

            Actor actor = new Actor(entity.Scene, vertices, transform);
            Transform2 t = new Transform2();
            entity.SetParent(actor);

            return actor;
        }

        public static Body CreatePolygon(World world, Transform2 transform, IList<Vector2> vertices)
        {
            Debug.Assert(world != null);
            Body body = BodyExt.CreateBody(world);
            CreatePolygon(body, transform, vertices);
            return body;
        }

        public static void CreatePolygon(Body body, Transform2 transform, IList<Vector2> vertices)
        {
            Debug.Assert(body != null);
            Debug.Assert(transform != null);
            Debug.Assert(vertices != null && vertices.Count >= 3);
            List<Vector2> fixtureContour = Actor.GetFixtureContour(vertices, transform.Scale);
            fixtureContour = MathExt.SetWinding(fixtureContour, false);

            var convexList = PolygonExt.DecomposeConcave(fixtureContour);

            List<FarseerPhysics.Common.Vertices> vList = new List<FarseerPhysics.Common.Vertices>();

            BodyExt.SetTransform(body, transform);

            for (int i = convexList.Count - 1; i >= 0; i--)
            {
                int vertMax = FarseerPhysics.Settings.MaxPolygonVertices;
                int divs = 1 + convexList[i].Count / vertMax;
                if (divs < 2)
                {
                    continue;
                }
                int j = 1;
                while (j < convexList[i].Count)
                {
                    List<Vector2> list = new List<Vector2>();
                    list.Add(convexList[i][0]);
                    
                    list.AddRange(convexList[i].GetRange(j, Math.Min(convexList[i].Count - j, vertMax - 1)));
                    j += vertMax - 2;
                    convexList.Add(list);
                }
                convexList.RemoveAt(i);
            }

            for (int i = 0; i < convexList.Count; i++)
            {
                var v1 = new FarseerPhysics.Common.Vertices();
                v1.AddRange(Vector2Ext.ToXna(convexList[i]));

                vList.Add(v1);
                PolygonShape shape = new PolygonShape(v1, 1);
                Fixture fixture = body.CreateFixture(shape);
                FixtureData userData = FixtureExt.SetData(fixture);
            }
        }

        public static PortalJoint CreatePortalJoint(World world, Body parentBody, Body childBody, IPortal enterPortal)
        {
            PortalJoint portalJoint = new PortalJoint(parentBody, childBody, enterPortal);
            world.AddJoint(portalJoint);
            return portalJoint;
        }
    }
}
