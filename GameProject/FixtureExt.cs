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

        public static FixtureEdgeCoord[] GetFixtureCircleIntersections(World world, Vector2 point, float radius)
        {
            List<Fixture> potentials = new List<Fixture>();
            var box = new FarseerPhysics.Collision.AABB(new Xna.Vector2(point.X, point.Y), radius * 2, radius * 2);
            world.QueryAABB(delegate(Fixture fixture)
            {
                potentials.Add(fixture);
                return false;
            }, ref box);

            List<FixtureEdgeCoord> collisions = new List<FixtureEdgeCoord>();
            foreach (Fixture f in potentials)
            {
                Xna.Vector2 relativePoint = f.Body.GetLocalPoint(new Xna.Vector2(point.X, point.Y));
                switch (f.ShapeType)
                {
                    /*case ShapeType.Circle:
                        CircleShape circle = (CircleShape)f.Shape;
                        if ((circle.Position - relativePoint).Length() <= radius + circle.Radius)
                        {

                            //collisions.Add(f);
                        }
                        break;*/

                    case ShapeType.Polygon:
                        PolygonShape polygon = (PolygonShape)f.Shape;
                        for (int i = 0; i < polygon.Vertices.Count; i++)
                        {
                            int iNext = (i + 1) % polygon.Vertices.Count;
                            Line edge = new Line(polygon.Vertices[i], polygon.Vertices[iNext]);
                            IntersectPoint[] intersects = MathExt.GetLineCircleIntersections(new Vector2(relativePoint.X, relativePoint.Y), radius, edge, true);
                            for (int j = 0; i < intersects.Length; i++)
                            {
                                collisions.Add(new FixtureEdgeCoord(f, i, (float)intersects[j].T));
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

        public static FixtureEdgeCoord GetNearestPortalableEdge(World world, Vector2 point, float maxRadius, float portalSize)
        {
            List<Fixture> potentials = new List<Fixture>();
            var box = new FarseerPhysics.Collision.AABB(Vector2Ext.ConvertToXna(point), maxRadius * 2, maxRadius * 2);
            world.QueryAABB(delegate(Fixture fixture)
            {
                potentials.Add(fixture);
                return true;
            }, ref box);

            FixtureEdgeCoord nearest = null;
            foreach (Fixture f in potentials)
            {
                Debug.Assert(BodyExt.GetUserData(f.Body).LinkedEntity.Transform.Position == Vector2Ext.ConvertTo(f.Body.Position));
                Vector2 localPoint = Vector2Ext.ConvertTo(f.Body.GetLocalPoint(new Xna.Vector2(point.X, point.Y)));
                Vector2 localPoint2 = Vector2Ext.Transform(point, FixtureExt.GetUserData(f).Entity.Transform.GetWorldMatrix().Inverted());
                //Debug.Assert(localPoint == localPoint2);
                switch (f.ShapeType)
                {
                    case ShapeType.Polygon:
                        PolygonShape polygon = (PolygonShape)f.Shape;
                        for (int i = 0; i < polygon.Vertices.Count; i++)
                        {
                            int iNext = (i + 1) % polygon.Vertices.Count;
                            //check that the line can have a FixturePortal on it
                            if (!PortalPlacer.EdgeIsValid(f, i, portalSize))
                            {
                                continue;
                            }
                            Line edge = new Line(polygon.Vertices[i], polygon.Vertices[iNext]);
                            Vector2 v = edge.Nearest(localPoint, true);
                            float vDist = (v - localPoint).Length;
                            if ((nearest == null && vDist <= maxRadius) ||
                                (nearest != null && vDist < (nearest.GetPosition() - localPoint).Length))
                            {
                                nearest = new FixtureEdgeCoord(f, i, edge.NearestT(localPoint, true));
                            }
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            if (nearest != null)
            {
                nearest = PortalPlacer.GetValid(nearest, portalSize);
            }
            return nearest;
        }
    }
}
