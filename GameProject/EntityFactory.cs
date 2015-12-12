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
    public static class EntityFactory
    {
        public static Entity CreateEntityBox(Scene scene, Vector2 position)
        {
            return CreateEntityBox(scene, new Transform2D(position));
        }

        public static Entity CreateEntityBox(Scene scene, Transform2D transform)
        {
            Debug.Assert(scene != null);
            Entity entity = new Entity(scene);
            return CreateEntityBox(entity, transform);
        }

        public static Entity CreateEntityBox(Entity entity, Vector2 position)
        {
            return CreateEntityBox(entity, new Transform2D(position));
        }

        public static Entity CreateEntityBox(Entity entity, Transform2D transform)
        {
            Transform2D t = entity.GetTransform();
            t.Position = transform.Position;
            t.Rotation = transform.Rotation;
            entity.SetTransform(t);
            entity.IsPortalable = true;
            entity.Models.Add(ModelFactory.CreatePlane(transform.Scale));

            Debug.Assert(entity.Body == null);
            Body body = BodyFactory.CreateRectangle(entity.Scene.World, transform.Scale.X, transform.Scale.Y, 1);
            entity.Scene.World.ProcessChanges();
            body.Position = Vector2Ext.ConvertToXna(transform.Position);
            entity.SetBody(body);
            body.BodyType = BodyType.Dynamic;

            FixtureUserData userData = new FixtureUserData(body.FixtureList[0]);

            FixtureExt.SetUserData(body.FixtureList[0], userData);
            return entity;
        }

        public static Entity CreateEntityPolygon(Scene scene, Vector2 position, Vector2[] vertices)
        {
            return CreateEntityPolygon(scene, new Transform2D(position), vertices);
        }

        public static Entity CreateEntityPolygon(Scene scene, Transform2D transform, Vector2[] vertices)
        {
            Debug.Assert(scene != null);
            Entity entity = new Entity(scene, transform);
            return CreateEntityPolygon(entity, transform, vertices);
        }

        public static Entity CreateEntityPolygon(Entity entity, Transform2D transform, Vector2[] vertices)
        {
            Transform2D t = entity.GetTransform();
            t.Position = transform.Position;
            t.Rotation = transform.Rotation;
            entity.SetTransform(t);
            vertices = MathExt.SetHandedness(vertices, false);
            Poly2Tri.Polygon polygon = PolygonFactory.CreatePolygon(vertices);

            entity.Models.Add(ModelFactory.CreatePolygon(polygon));

            Xna.Vector2 vPos = Vector2Ext.ConvertToXna(t.Position);

            List<FarseerPhysics.Common.Vertices> vList = new List<FarseerPhysics.Common.Vertices>();

            Debug.Assert(entity.Body == null);
            Body body = BodyExt.CreateBody(entity.Scene.World);
            body.Position = Vector2Ext.ConvertToXna(transform.Position);
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
                FixtureUserData userData = new FixtureUserData(fixture);
                FixtureExt.SetUserData(fixture, userData);
                for (int j = 0; j < polygon.Triangles[i].Neighbors.Count(); j++)
                {
                    userData.EdgeIsExterior[j] = polygon.Triangles[i].EdgeIsConstrained[(j + 2) % 3];
                }
            }

            entity.SetBody(body);

            return entity;
        }
    }
}
