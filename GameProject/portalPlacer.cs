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
                    portal.SetPosition(intersection.Actor, new PolygonCoord(intersection.EdgeIndex, intersection.EdgeT));
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
                IntersectCoord intersectLast = new IntersectCoord();
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
                                    IntersectCoord intersect = MathExt.LineLineIntersect(
                                        new Line(vertices[i0], vertices[i1]),
                                        new Line(rayBegin, rayIntersect),
                                        true);
                                    if (intersect.Exists)
                                    {
                                        if (!FixtureEdgeIsValid(fixture, i))
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

        /*public static PolygonCoord GetValid(IPolygonCoord coord, float portalSize)
        {

        }*/

        private static float GetValidT(float t, Line edge, float portalSize)
        {
            Debug.Assert(EdgeValidLength(edge.Length, portalSize));
            float portalSizeT = (portalSize + FixturePortal.EdgeMargin * 2) / edge.Length;
            return MathHelper.Clamp(t, portalSizeT / 2, 1 - portalSizeT / 2);
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
        public static bool FixtureEdgeIsValid(Fixture fixture, int edgeIndex)
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
        public static bool FixtureEdgeIsValid(Fixture fixture, int edgeIndex, float portalSize)
        {
            if (FixtureEdgeIsValid(fixture, edgeIndex))
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

        /// <summary>
        /// Return whether world space edge is long enough to fit a portal.
        /// </summary>
        private static bool EdgeValidLength(float edgeLength, float portalSize)
        {
            return edgeLength > portalSize + FixturePortal.EdgeMargin * 2;
        }

        /*public PolygonCoord RayCast(IList<IPolygon> polygons, Line ray)
        {
            IPolygon polygonNearest = null;
            for (int i = 0; i < polygons.Count; i++)
            {
                List<PolygonCoord> points = MathExt.LinePolygonIntersect(ray, polygons[i]);
                //IntersectPoint p = points.Min(item => ((Vector2)item.Position - ray[0]).Length);
                for (int j = 0; j < points.Count; j++)
                {
                    double dist = (points[j].Position - (Vector2d)ray[0]).Length;
                    //if (dist < (pointNearest.Position - (Vector2d)ray[0]).Length)
                    {

                    }
                }
                if (p != null)
                {
                }
            }
        }*/
        public static Tuple<IWall,PolygonCoord> GetNearestPortalableEdge(IList<IWall> walls, Vector2 point, float maxRadius, float portalSize)
        {
            IWall wallNearest = null;
            PolygonCoord nearest = new PolygonCoord();
            double distanceMin = -1;
            for (int i = 0; i < walls.Count; i++)
            {
                IList<Vector2> vertices = walls[i].GetWorldVertices();
                for (int edgeIndex = 0; edgeIndex < walls[i].Vertices.Count; edgeIndex++)
                {
                    int edgeIndexNext = (edgeIndex + 1) % vertices.Count;
                    Line edge = new Line(vertices[edgeIndex], vertices[edgeIndexNext]);
                    if (!EdgeValidLength(edge.Length, portalSize))
                    {
                        continue;
                    }
                    //double distance = MathExt.PointLineDistance(point, edge, true);
                    float portalT = edge.NearestT(point, true);
                    portalT = GetValidT(portalT, edge, portalSize);
                    Vector2 portalPos = edge.Lerp(portalT);
                    double distance = (portalPos - point).Length;
                    if (maxRadius < distance)
                    {
                        continue;
                    }
                    if (distanceMin == -1 || distance < distanceMin)
                    {
                        nearest.EdgeIndex = edgeIndex;
                        nearest.EdgeT = edge.NearestT(point, true);
                        distanceMin = distance;
                        wallNearest = walls[i];
                    }
                }
            }
            return new Tuple<IWall, PolygonCoord>(wallNearest, nearest);
        }

        public static FixtureEdgeCoord GetNearestPortalableEdge(World world, Vector2 point, float maxRadius, float portalSize)
        {
            List<Fixture> potentials = new List<Fixture>();
            var box = new FarseerPhysics.Collision.AABB(Vector2Ext.ConvertToXna(point), maxRadius * 2, maxRadius * 2);
            world.QueryAABB(delegate (Fixture fixture)
            {
                potentials.Add(fixture);
                return true;
            }, ref box);

            FixtureEdgeCoord nearest = null;
            foreach (Fixture f in potentials)
            {
                Debug.Assert(BodyExt.GetUserData(f.Body).Actor.GetTransform().Position == Vector2Ext.ConvertTo(f.Body.Position));
                Vector2 localPoint = Vector2Ext.ConvertTo(f.Body.GetLocalPoint(new Xna.Vector2(point.X, point.Y)));
                switch (f.Shape.ShapeType)
                {
                    case ShapeType.Polygon:
                        PolygonShape polygon = (PolygonShape)f.Shape;
                        for (int i = 0; i < polygon.Vertices.Count; i++)
                        {
                            int iNext = (i + 1) % polygon.Vertices.Count;
                            //check that the line can have a FixturePortal on it
                            if (!PortalPlacer.FixtureEdgeIsValid(f, i, portalSize))
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
