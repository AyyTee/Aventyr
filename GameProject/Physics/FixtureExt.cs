using ClipperLib;
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
        const float ERROR_MARGIN = 0.0001f;

        public static FixtureUserData SetUserData(Fixture fixture)
        {
            FixtureUserData userData = new FixtureUserData(fixture);
            fixture.UserData = userData;
            return userData;
        }

        public static FixtureUserData GetUserData(Fixture fixture)
        {
            Debug.Assert(fixture != null);
            Debug.Assert(fixture.UserData != null);
            return (FixtureUserData)fixture.UserData;
        }

        public static Fixture CreateFixture(Body body, Shape shape)
        {
            Fixture fixture = body.CreateFixture(shape);
            SetUserData(fixture);
            return fixture;
        }

        public static FixtureCoord GetFixtureEdgeCoord(IActor actor, IPolygonCoord coord)
        {
            Debug.Assert(actor.Body != null);
            Debug.Assert(coord != null);

            FixtureCoord fixtureCoord = coord as FixtureCoord;
            if (fixtureCoord != null)
            {
                Debug.Assert(actor == fixtureCoord.Actor);
                return fixtureCoord;
            }

            List<Vector2> fixtureContour = ActorExt.GetFixtureContour(actor);
            
            Line edge = PolygonExt.GetEdge(fixtureContour, coord);
            Debug.Assert(
                edge[0] == fixtureContour[coord.EdgeIndex] &&
                edge[1] == fixtureContour[(coord.EdgeIndex + 1) % fixtureContour.Count]
                );

            if (!PolygonExt.IsInterior(fixtureContour))
            {
                edge.Reverse();
            }

            foreach (Fixture f in actor.Body.FixtureList)
            {
                switch (f.Shape.ShapeType)
                {
                    case ShapeType.Polygon:
                        PolygonShape polygon = (PolygonShape)f.Shape;
                        for (int i = 0; i < polygon.Vertices.Count; i++)
                        {
                            int iNext = (i + 1) % polygon.Vertices.Count;
                            if ((edge[0] - Vector2Ext.ConvertTo(polygon.Vertices[i])).Length < ERROR_MARGIN && 
                                (edge[1] - Vector2Ext.ConvertTo(polygon.Vertices[iNext])).Length < ERROR_MARGIN)
                            {
                                //float edgeT = PolygonExt.IsInterior(fixtureContour) ? coord.EdgeT : 1 - coord.EdgeT;
                                float edgeT = coord.EdgeT;
                                return new FixtureCoord(f, i, edgeT);
                            }
                        }
                        break;
                    default:
                        Debug.Fail("Cannot currently handle shapes other than polygons.");
                        break;
                }
            }
            Debug.Fail("Could not find FixtureEdgeCoord.");
            return null;
        }

        /// <summary>
        /// Returns Fixture that FixturePortal is attached to, or null if none exists.
        /// </summary>
        public static Fixture GetFixturePortalParent(FixturePortal portal)
        {
            if (portal.Parent == null)
            {
                return null;
            }
            IActor parent = portal.Parent as IActor;
            if (parent != null)
            {
                return GetFixtureEdgeCoord(parent, portal.Position).Fixture;
            }
            return null;
        }

        public static FixtureCoord[] GetFixtureCircleIntersections(World world, Vector2 point, float radius)
        {
            List<Fixture> potentials = new List<Fixture>();
            var box = new FarseerPhysics.Collision.AABB(new Xna.Vector2(point.X, point.Y), radius * 2, radius * 2);
            world.QueryAABB(delegate (Fixture fixture)
            {
                potentials.Add(fixture);
                return false;
            }, ref box);

            List<FixtureCoord> collisions = new List<FixtureCoord>();
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
                                collisions.Add(new FixtureCoord(f, i, (float)intersects[j].TFirst));
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
