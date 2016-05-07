using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xna = Microsoft.Xna.Framework;

namespace Game
{
    public static class FixtureExt
    {
        public static FixtureUserData SetUserData(Fixture fixture)
        {
            //Debug.Assert(userData != null);
            //Ugly solution to storing Game classes in a way that still works when deserializing the data.
            //This list is intended to only store one element.
            FixtureUserData data = new FixtureUserData(fixture);
            var a = new List<FixtureUserData>();
            fixture.UserData = a;
            a.Add(data);
            return data;
        }

        public static FixtureUserData GetUserData(Fixture fixture)
        {
            Debug.Assert(fixture.UserData != null);
            FixtureUserData userData = ((List<FixtureUserData>)fixture.UserData)[0];
            Debug.Assert(userData != null);
            return userData;
        }

        public static Fixture CreateFixture(Body body, Shape shape)
        {
            Fixture fixture = body.CreateFixture(shape);
            SetUserData(fixture);
            return fixture;
        }

        public static FixtureEdgeCoord[] GetFixtureCircleIntersections(World world, Vector2 point, float radius)
        {
            List<Fixture> potentials = new List<Fixture>();
            var box = new FarseerPhysics.Collision.AABB(new Xna.Vector2(point.X, point.Y), radius * 2, radius * 2);
            world.QueryAABB(delegate (Fixture fixture)
            {
                potentials.Add(fixture);
                return false;
            }, ref box);

            List<FixtureEdgeCoord> collisions = new List<FixtureEdgeCoord>();
            foreach (Fixture f in potentials)
            {
                Xna.Vector2 relativePoint = f.Body.GetLocalPoint(new Xna.Vector2(point.X, point.Y));
                switch (f.Shape.ShapeType)
                {
                    case ShapeType.Polygon:
                        PolygonShape polygon = (PolygonShape)f.Shape;
                        for (int i = 0; i < polygon.Vertices.Count; i++)
                        {
                            int iNext = (i + 1) % polygon.Vertices.Count;
                            Line edge = new Line(polygon.Vertices[i], polygon.Vertices[iNext]);
                            IntersectCoord[] intersects = MathExt.LineCircleIntersect(new Vector2(relativePoint.X, relativePoint.Y), radius, edge, true);
                            for (int j = 0; i < intersects.Length; i++)
                            {
                                collisions.Add(new FixtureEdgeCoord(f, i, (float)intersects[j].TFirst));
                            }
                            /*if (intersects.Length > 0)
                            {
                                fixtureCollisions.Add(f);
                                break;
                            }*/
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            return collisions.ToArray();
        }
    }
}
