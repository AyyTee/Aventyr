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
        public static void SetUserData(Fixture fixture, FixtureUserData userData)
        {
            //Ugly solution to storing Game classes in a way that still works when deserializing the data.
            //This list is intended to only store one element.
            var a = new List<FixtureUserData>();
            fixture.UserData = a;
            a.Add(userData);
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
            Fixture fixture = new Fixture(body, shape);
            SetUserData(fixture, new FixtureUserData(fixture));
            return fixture;
        }

        /*public static Fixture CreatePortalFixture(Body body, Shape shape, FixturePortal portal)
        {
            Fixture fixture = CreateFixture(body, shape);
            GetUserData(fixture).PortalParents = portal;
            return fixture;
        }*/

        public static Fixture[] GetFixtureCircleIntersections(World world, Vector2 point, float radius)
        {
            List<Fixture> potentials = new List<Fixture>();
            var box = new FarseerPhysics.Collision.AABB(new Xna.Vector2(point.X, point.Y), radius * 2, radius * 2);
            world.QueryAABB(delegate(Fixture fixture)
            {
                potentials.Add(fixture);
                return false;
            }, ref box);

            List<Fixture> collisions = new List<Fixture>();
            foreach (Fixture f in potentials)
            {
                Xna.Vector2 relativePoint = f.Body.GetLocalPoint(new Xna.Vector2(point.X, point.Y));
                switch (f.ShapeType)
                {
                    case ShapeType.Circle:
                        CircleShape circle = (CircleShape)f.Shape;
                        if ((circle.Position - relativePoint).Length() <= radius + circle.Radius)
                        {
                            collisions.Add(f);
                        }
                        break;

                    case ShapeType.Polygon:
                        PolygonShape polygon = (PolygonShape)f.Shape;
                        for (int i = 0; i < polygon.Vertices.Count; i++)
                        {
                            int iNext = (i + 1) % polygon.Vertices.Count;
                            Line edge = new Line(polygon.Vertices[i], polygon.Vertices[iNext]);
                            IntersectPoint[] intersects = MathExt.GetLineCircleIntersections(new Vector2(relativePoint.X, relativePoint.Y), radius, edge, true);
                            if (intersects.Length > 0)
                            {
                                collisions.Add(f);
                                break;
                            }
                        }
                        break;

                    default:
                        Debug.Assert(false, f.ShapeType.ToString() + " has not been implemented.");
                        break;
                }
            }
            return collisions.ToArray();
        }
    }
}
