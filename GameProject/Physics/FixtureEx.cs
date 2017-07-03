using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Game.Common;
using Game.Portals;
using Vector2 = OpenTK.Vector2;
using Xna = Microsoft.Xna.Framework;
using Game.Rendering;

namespace Game.Physics
{
    public static class FixtureEx
    {
        const float ErrorMargin = 0.0001f;

        public static FixtureData SetData(Fixture fixture)
        {
            FixtureData userData = new FixtureData(fixture);
            fixture.UserData = userData;
            return userData;
        }

        public static FixtureData GetData(Fixture fixture)
        {
            DebugEx.Assert(fixture != null);
            DebugEx.Assert(fixture.UserData != null);
            return (FixtureData)fixture.UserData;
        }

        public static Fixture CreateFixture(Body body, Shape shape)
        {
            Fixture fixture = body.CreateFixture(shape);
            SetData(fixture);
            return fixture;
        }

        public static FixtureCoord GetFixtureEdgeCoord(Actor actor, IPolygonCoord coord)
        {
            DebugEx.Assert(actor.Body != null);
            DebugEx.Assert(coord != null);

            FixtureCoord fixtureCoord = coord as FixtureCoord;
            if (fixtureCoord != null)
            {
                DebugEx.Assert(actor == fixtureCoord.Actor);
                return fixtureCoord;
            }

            List<Vector2> fixtureContour = Actor.GetFixtureContour(actor);

            LineF edge = PolygonEx.GetEdge(fixtureContour, coord);
            DebugEx.Assert(
                edge[0] == fixtureContour[coord.EdgeIndex] &&
                edge[1] == fixtureContour[(coord.EdgeIndex + 1) % fixtureContour.Count]
                );

            if (!PolygonEx.IsInterior(fixtureContour))
            {
                edge = edge.Reverse();
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
                            if ((edge[0] - (Vector2)polygon.Vertices[i]).Length < ErrorMargin &&
                                (edge[1] - (Vector2)polygon.Vertices[iNext]).Length < ErrorMargin)
                            {
                                //float edgeT = PolygonExt.IsInterior(fixtureContour) ? coord.EdgeT : 1 - coord.EdgeT;
                                float edgeT = coord.EdgeT;
                                return new FixtureCoord(f, i, edgeT);
                            }
                        }
                        break;
                    default:
                        DebugEx.Fail("Cannot currently handle shapes other than polygons.");
                        break;
                }
            }
            DebugEx.Fail("Could not find FixtureEdgeCoord.");
            return null;
        }

        /// <summary>
        /// Returns the Fixture that FixturePortal is attached to, or null if none exists.
        /// </summary>
        public static Fixture GetFixtureAttached(FixturePortal portal)
        {
            if (portal == null || portal.Parent == null)
            {
                return null;
            }
            Actor parent = portal.Parent as Actor;
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
                            LineF edge = new LineF(polygon.Vertices[i], polygon.Vertices[iNext]);
                            IntersectCoord[] intersects = MathEx.LineCircleIntersect(new Vector2(relativePoint.X, relativePoint.Y), radius, edge, true);
                            for (int j = 0; i < intersects.Length; i++)
                            {
                                collisions.Add(new FixtureCoord(f, i, (float)intersects[j].First));
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

        public static Vector2[] GetWorldPoints(Fixture fixture)
        {
            PolygonShape shape = (PolygonShape)fixture.Shape;
            Vector2[] v = new Vector2[shape.Vertices.Count];
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = (Vector2)fixture.Body.GetWorldPoint(shape.Vertices[i]);
            }
            return v;
        }

        public static Vector2 GetCenterLocal(Fixture fixture)
        {
            PolygonShape shape = (PolygonShape)fixture.Shape;
            var vertices = shape.Vertices;
            return new Vector2(
                vertices.Average(vert => vert.X), 
                vertices.Average(vert => vert.Y));
        }

        public static Vector2 GetCenterWorld(Fixture fixture)
        {
            PolygonShape shape = (PolygonShape)fixture.Shape;
            var vertices = shape.Vertices;
            return
                (Vector2)fixture.Body.GetWorldPoint(new Xna.Vector2(
                vertices.Average(vert => vert.X),
                vertices.Average(vert => vert.Y)));
        }

        public static List<IPortal> GetPortalCollisions(Fixture fixture, IList<IPortal> portals, bool ignoreAttachedPortals = true)
        {
            Vector2[] vertices = GetWorldPoints(fixture);
            List<IPortal> collisions = 
                Portal.GetCollisions(
                    BodyEx.GetLocalOrigin(fixture.Body), 
                    vertices, 
                    portals.Where(item => !ignoreAttachedPortals || GetFixtureAttached(item as FixturePortal) != fixture)
                        .OfType<IPortalRenderable>()
                        .ToList())
                .OfType<IPortal>()
                .ToList();

            if (ignoreAttachedPortals)
            {
                var attached = GetData(fixture).GetPortalChildren();
                //We don't exclude attached portals that this fixture's body is travelling through.
                attached.Where(item => item != BodyEx.GetData(fixture.Body).BodyParent.Portal.Linked);
                return collisions.Except(attached).ToList();
            }
            return collisions;
        }
    }
}
