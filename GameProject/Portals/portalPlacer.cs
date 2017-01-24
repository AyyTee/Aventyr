using System;
using OpenTK;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Game.Common;

namespace Game.Portals
{
    public static class PortalPlacer
    {
        /// <summary>
        /// Attach a portal to the nearest valid wall edge along a ray.
        /// </summary>
        /// <param name="portal">Portal that will potentially be attached to a wall edge.</param>
        /// <param name="ray">Portal ray cast. ray[0] is the beginning point and ray[1] is the end point.</param>
        public static IEnumerable<WallCoord> PortalPlace(FixturePortal portal, LineF ray)
        {
            return RayCast(portal.Scene, ray);
        }

        public static IEnumerable<WallCoord> RayCast(IScene scene, LineF ray)
        {
            var wallCoord = new List<WallCoord>();
            foreach (IWall wall in scene.GetAll().OfType<IWall>())
            {
                var vertices = wall.GetWorldVertices();
                for (int i = 0; i < vertices.Count; i++)
                {
                    int iNext = (i + 1) % vertices.Count;
                    IntersectCoord coord = MathExt.LineLineIntersect(ray, new LineF(vertices[i], vertices[iNext]), true);
                    if (coord.Exists)
                    {
                        wallCoord.Add(new WallCoord(wall, i, (float) coord.Last));
                    }
                }
            }
            return wallCoord.OrderBy(item => (PolygonExt.GetTransform(item.Wall.GetWorldVertices(), item).Position - ray[0]).Length);
        }

        /// <summary>
        /// Returns a new WallCoord that represents a valid position for a portal.  Returns null if the portal is too large.
        /// </summary>
        /// <param name="coord">Initial portal position.</param>
        /// <param name="portalSize">Size of portal.</param>
        public static WallCoord AdjustCoord(WallCoord coord, float portalSize)
        {
            LineF edge = PolygonExt.GetEdge(coord.Wall.GetWorldVertices(), coord);
            return EdgeValid(coord, portalSize)
                ? new WallCoord(coord.Wall, coord.EdgeIndex, GetValidT(coord.EdgeT, edge, portalSize))
                : null;
        }

        static float GetValidT(float t, LineF edge, float portalSize)
        {
            Debug.Assert(EdgeValidLength(edge.Length, portalSize));
            float portalSizeT = (portalSize + FixturePortal.EdgeMargin * 2) / edge.Length;
            return MathHelper.Clamp(t, portalSizeT / 2, 1 - portalSizeT / 2);
        }

        public static bool EdgeValid(WallCoord coord, float portalSize)
        {
            LineF edge = PolygonExt.GetEdge(coord.Wall.GetWorldVertices(), coord);
            return EdgeValidLength(edge.Length, portalSize);
        }

        /// <summary>
        /// Return whether world space edge is long enough to fit a portal.
        /// </summary>
        static bool EdgeValidLength(float edgeLength, float portalSize)
        {
            return edgeLength > portalSize + FixturePortal.EdgeMargin * 2;
        }

        public static WallCoord GetNearestPortalableEdge(IList<IWall> walls, Vector2 point, float maxRadius, float portalSize)
        {
            WallCoord wallCoord = null;
            double distanceMin = -1;
            for (int i = 0; i < walls.Count; i++)
            {
                IList<Vector2> vertices = walls[i].GetWorldVertices();
                for (int edgeIndex = 0; edgeIndex < walls[i].Vertices.Count; edgeIndex++)
                {
                    int edgeIndexNext = (edgeIndex + 1) % vertices.Count;
                    LineF edge = new LineF(vertices[edgeIndex], vertices[edgeIndexNext]);
                    if (!EdgeValidLength(edge.Length, portalSize))
                    {
                        continue;
                    }
                    Vector2 nearPos = edge.Nearest(point, true);
                    float portalT = edge.NearestT(point, true);
                    portalT = GetValidT(portalT, edge, portalSize);
                    Vector2 portalPos = edge.Lerp(portalT);
                    double distance = (portalPos - point).Length;
                    if (maxRadius < (nearPos - point).Length)
                    {
                        continue;
                    }
                    if (distanceMin == -1 || distance < distanceMin)
                    {
                        wallCoord = new WallCoord(walls[i], edgeIndex, portalT);
                        distanceMin = distance;
                    }
                }
            }
            return wallCoord;
        }
    }
}
