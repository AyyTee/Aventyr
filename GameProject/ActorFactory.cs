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
            /*Transform2D t = entity.GetTransform();
            t.Position = transform.Position;
            t.Rotation = transform.Rotation;
            entity.SetTransform(t);*/
            entity.IsPortalable = true;
            entity.AddModel(ModelFactory.CreatePlane(transform.Scale));

            /*Actor actor = new Actor(entity.Scene, BodyExt.CreateBody(entity.Scene.World));
            //actor.SetTransform(transform);
            entity.SetParent(actor);
            BodyExt.SetUserData(actor.Body, actor);
            var vertices = new FarseerPhysics.Common.Vertices();
            vertices.Add(new Xna.Vector2(-0.5f, 0.5f));
            vertices.Add(new Xna.Vector2(0.5f, 0.5f));
            vertices.Add(new Xna.Vector2(0.5f, -0.5f));
            vertices.Add(new Xna.Vector2(-0.5f, -0.5f));
            FixtureExt.CreateFixture(actor.Body, new PolygonShape(vertices, 1));

            */

            Body body = BodyFactory.CreateRectangle(entity.Scene.World, transform.Scale.X, transform.Scale.Y, 1);
            entity.Scene.World.ProcessChanges();
            body.Position = Vector2Ext.ConvertToXna(transform.Position);
            Actor actor = new Actor(entity.Scene, body);
            body.BodyType = BodyType.Dynamic;
            entity.SetParent(actor);

            FixtureUserData userData = FixtureExt.SetUserData(body.FixtureList[0]);

            Transform2 t = new Transform2();
            t.Position = transform.Position;
            t.Rotation = transform.Rotation;
            actor.SetTransform(t);
            return actor;
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
            vertices = MathExt.SetHandedness(vertices, false);
            Poly2Tri.Polygon polygon = PolygonFactory.CreatePolygon(vertices);

            entity.AddModel(ModelFactory.CreatePolygon(polygon));

            Xna.Vector2 vPos = Vector2Ext.ConvertToXna(transform.Position);

            List<FarseerPhysics.Common.Vertices> vList = new List<FarseerPhysics.Common.Vertices>();


            Body body = BodyExt.CreateBody(entity.Scene.World);
            //body.Position = Vector2Ext.ConvertToXna(transform.Position);
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

            Actor actor = new Actor(entity.Scene, body);
            Transform2 t = new Transform2();
            t.Position = transform.Position;
            t.Rotation = transform.Rotation;
            actor.SetTransform(t);
            entity.SetParent(actor);

            return actor;
        }
    }
}
