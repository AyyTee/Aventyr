using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using OpenTK;
//using Poly2Tri.Triangulation.Polygon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xna = Microsoft.Xna.Framework;

namespace Game
{
    public static class ActorFactory
    {
        public static Actor CreateEntityBox(Scene scene, Vector2 position)
        {
            return CreateEntityBox(scene, new Transform2(position));
        }

        public static Actor CreateEntityBox(Scene scene, Transform2 transform)
        {
            Debug.Assert(scene != null);
            Entity entity = new Entity(scene);
            return CreateEntityBox(entity, transform);
        }

        public static Actor CreateEntityBox(Entity entity, Vector2 position)
        {
            return CreateEntityBox(entity, new Transform2(position));
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

            //FixtureUserData userData = FixtureExt.SetUserData(body.FixtureList[0]);

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
            List<Vector2> verticesCopy = ActorExt.GetFixtureContour(vertices, transform);//new List<Vector2>(vertices);
            MathExt.SetHandedness(verticesCopy, false);
            
            //verticesCopy = (List<Vector2>)Vector2Ext.Transform(verticesCopy, Matrix4.CreateScale(new Vector3(transform.Scale)));
            Poly2Tri.Polygon polygon = PolygonFactory.CreatePolygon(verticesCopy);

            List<FarseerPhysics.Common.Vertices> vList = new List<FarseerPhysics.Common.Vertices>();

            //Body body = BodyExt.CreateBody(world);
            BodyExt.SetTransform(body, transform);

            for (int i = 0; i < polygon.Triangles.Count; i++)
            {
                var v1 = new FarseerPhysics.Common.Vertices();

                for (int j = 0; j < polygon.Triangles[i].Points.Count(); j++)
                {
                    v1.Add(Vector2Ext.ConvertToXna(polygon.Triangles[i].Points[j]));
                }

                vList.Add(v1);
                PolygonShape shape = new PolygonShape(v1, 1);
                Fixture fixture = body.CreateFixture(shape);
                FixtureUserData userData = FixtureExt.SetUserData(fixture);
                for (int j = 0; j < polygon.Triangles[i].Neighbors.Count(); j++)
                {
                    userData.EdgeIsExterior[j] = polygon.Triangles[i].EdgeIsConstrained[(j + 2) % 3];
                }
            }
        }
    }
}
