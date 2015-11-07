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
            FixtureEdgeCoord intersection = RayCast(portal.Scene, ray);
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

        public static FixtureEdgeCoord RayCast(Scene scene, Line ray)
        {
            Vector2 rayBegin = ray.Vertices[0];
            Vector2 rayEnd = ray.Vertices[1];
            if (rayBegin != rayEnd)
            {
                List<FixtureEdgeCoord> intersections = new List<FixtureEdgeCoord>();
                IntersectPoint intersectLast = new IntersectPoint();
                scene.PhysWorld.RayCast(
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
                                {
                                    PolygonShape shape = (PolygonShape)fixture.Shape;
                                    Vector2[] vertices = Vector2Ext.ConvertTo(shape.Vertices);
                                    var transform = new FarseerPhysics.Common.Transform();
                                    fixture.Body.GetTransform(out transform);
                                    Matrix4 matTransform = MatrixExt4.ConvertTo(transform);
                                    vertices = Vector2Ext.Transform(vertices, matTransform);
                                    for (int i = 0; i < vertices.Count(); i++)
                                    {
                                        int i0 = i;
                                        int i1 = (i + 1) % vertices.Count();
                                        IntersectPoint intersect = MathExt.LineIntersection(
                                            vertices[i0],
                                            vertices[i1],
                                            rayBegin,
                                            rayIntersect,
                                            true);
                                        if (intersect.Exists)
                                        {
                                            if (!EdgeIsValid(fixture, i))
                                            {
                                                break;
                                            }
                                            //ignore edges facing away
                                            Line rayLine = new Line(vertices[i0], vertices[i1]); 
                                            if (rayLine.GetSideOf(rayBegin) != rayLine.GetSideOf(rayLine.Vertices[0] + rayLine.GetNormal()))
                                            {
                                                break;
                                            }

                                            intersectLast = intersect;
                                            intersections.Add(new FixtureEdgeCoord(fixture, i, (float)intersect.T));
                                            break;
                                        }
                                        Debug.Assert(i + 1 < vertices.Count(), "Intersection edge was not found in shape.");
                                    }
                                    break;
                                }
                            case ShapeType.Circle:
                                {
                                    break;
                                }
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
        /// Given a intersection point, this returns a valid intersection point (which could be the same position), or null if none exists.
        /// </summary>
        /// <param name="intersection"></param>
        /// <param name="portal"></param>
        /// <returns></returns>
        public static FixtureEdgeCoord GetValid(FixtureEdgeCoord intersection, FixturePortal portal)
        {
            Line portalLine = new Line(portal.GetWorldVerts());
            float portalSize = portalLine.Length;
            Line edge = intersection.GetWorldEdge();
            float portalSizeT = portalSize / edge.Length + FixturePortal.EdgeMargin * 2;
            if (portalSizeT > 1)
            {
                return null;
            }
            float portalT = intersection.EdgeT;
            portalT = Math.Max(portalT, portalSizeT / 2);
            portalT = Math.Min(portalT, 1 - portalSizeT / 2);
            FixtureEdgeCoord intersectValid = new FixtureEdgeCoord(intersection.Fixture, intersection.EdgeIndex, portalT);
            return intersectValid;
        }

        /// <summary>
        /// Checks if an edge can have a portal placed on it.  This does not account for size of edge.
        /// </summary>
        /// <param name="fixture"></param>
        /// <param name="edgeIndex"></param>
        /// <returns></returns>
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
    }
}
