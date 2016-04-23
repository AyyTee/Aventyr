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
    public static class PortalPlacer
    {
        private const float RayCastMargin = 0.0001f;

        public static bool PortalPlace(FixturePortal portal, Line ray)
        {
            FixtureEdgeCoord intersection = RayCast(portal.Scene.World, ray);
            if (intersection != null)
            {
                intersection = GetValid(intersection, portal);
                if (intersection != null)
                {
                    portal.SetFixtureParent(intersection);
                    return true;
                }
            }
            return false;
        }

        public static FixtureEdgeCoord RayCast(World world, Line ray)
        {
            Vector2 rayBegin = ray[0];
            Vector2 rayEnd = ray[1];
            if (rayBegin != rayEnd)
            {
                List<FixtureEdgeCoord> intersections = new List<FixtureEdgeCoord>();
                IntersectPoint intersectLast = new IntersectPoint();
                world.RayCast(
                    delegate(Fixture fixture, Xna.Vector2 point, Xna.Vector2 normal, float fraction)
                    {
                        Vector2 rayIntersect = Vector2Ext.ConvertTo(point);
                        rayIntersect = rayIntersect + (rayIntersect - rayBegin).Normalized() * RayCastMargin;
                        if (FixtureExt.GetUserData(fixture).IsPortalParentless() == false)
                        {
                            return -1;
                        }
                        switch (fixture.Shape.ShapeType)
                        {
                            case ShapeType.Polygon:
                                PolygonShape shape = (PolygonShape)fixture.Shape;
                                Vector2[] vertices = Vector2Ext.ConvertTo(shape.Vertices);
                                var transform = new FarseerPhysics.Common.Transform();
                                fixture.Body.GetTransform(out transform);
                                Matrix4 matTransform = Matrix4Ext.ConvertTo(transform);
                                vertices = Vector2Ext.Transform(vertices, matTransform);
                                for (int i = 0; i < vertices.Count(); i++)
                                {
                                    int i0 = i;
                                    int i1 = (i + 1) % vertices.Count();
                                    IntersectPoint intersect = MathExt.LineLineIntersect(
                                        new Line(vertices[i0], vertices[i1]),
                                        new Line(rayBegin, rayIntersect),
                                        true);
                                    if (intersect.Exists)
                                    {
                                        if (!EdgeIsValid(fixture, i))
                                        {
                                            break;
                                        }
                                        //ignore edges facing away
                                        Line rayLine = new Line(vertices[i0], vertices[i1]); 
                                        if (rayLine.GetSideOf(rayBegin) != rayLine.GetSideOf(rayLine[0] + rayLine.GetNormal()))
                                        {
                                            break;
                                        }

                                        intersectLast = intersect;
                                        intersections.Add(new FixtureEdgeCoord(fixture, i, (float)intersect.TFirst));
                                        break;
                                    }
                                    Debug.Assert(i + 1 < vertices.Count(), "Intersection edge was not found in shape.");
                                }
                                break;
                            case ShapeType.Circle:
                                break;
                        }
                        return fraction;
                    },
                    Vector2Ext.ConvertToXna(rayBegin),
                    Vector2Ext.ConvertToXna(rayEnd));
                var sortedIntersections = intersections.OrderBy(item => (rayBegin - item.GetPosition()).Length);
                if (sortedIntersections.Count() > 0)
                {
                    return sortedIntersections.ToArray()[0];
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a valid FixtureEdgeCoord for a portal location (which could be the same position), or null if none exists.
        /// </summary>
        public static FixtureEdgeCoord GetValid(FixtureEdgeCoord intersection, float portalSize)
        {
            Line edge = intersection.GetWorldEdge();
            if (!EdgeValidLength(edge.Length, portalSize))
            {
                return null;
            }
            float portalSizeT = (portalSize + FixturePortal.EdgeMargin * 2) / edge.Length;
            float portalT = MathHelper.Clamp(intersection.EdgeT, portalSizeT / 2, 1 - portalSizeT / 2);
            FixtureEdgeCoord intersectValid = new FixtureEdgeCoord(intersection.Fixture, intersection.EdgeIndex, portalT);
            return intersectValid;
        }

        /// <summary>
        /// Returns a valid FixtureEdgeCoord for a portal location (which could be the same position), or null if none exists.
        /// </summary>
        public static FixtureEdgeCoord GetValid(FixtureEdgeCoord intersection, FixturePortal portal)
        {
            return GetValid(intersection, portal.GetWorldTransform().Size);
        }

        /// <summary>
        /// Checks if an edge can have a portal placed on it.  This does not account for size of edge.
        /// </summary>
        public static bool EdgeIsValid(Fixture fixture, int edgeIndex)
        {
            Debug.Assert(fixture.UserData != null);
            //interior edges are not valid
            FixtureUserData userData = FixtureExt.GetUserData(fixture);
            if (!userData.EdgeIsExterior[edgeIndex])
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if an edge can have a portal placed on it.
        /// </summary>
        public static bool EdgeIsValid(Fixture fixture, int edgeIndex, float portalSize)
        {
            if (EdgeIsValid(fixture, edgeIndex))
            {
                switch (fixture.Shape.ShapeType)
                {
                    case ShapeType.Polygon:
                        PolygonShape polygon = (PolygonShape)fixture.Shape;
                        Line edge = new Line(polygon.Vertices[edgeIndex], polygon.Vertices[(edgeIndex+1) % polygon.Vertices.Count]);
                        return EdgeValidLength(edge.Length, portalSize);
                    default:
                        Debug.Assert(false, fixture.Shape.ShapeType.ToString() + " is not supported.");
                        break;
                }
            }
            return false;
        }

        private static bool EdgeValidLength(float edgeLength, float portalSize)
        {
            return edgeLength > portalSize + FixturePortal.EdgeMargin * 2;
        }
    }
}
