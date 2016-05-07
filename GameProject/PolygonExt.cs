using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class PolygonExt
    {
        public static Transform2 GetTransform(IList<Vector2> vertices, IPolygonCoord coord)
        {
            Transform2 transform = new Transform2();
            Line line = GetEdge(vertices, coord);
            transform.Position = line.Lerp(coord.EdgeT);
            transform.Rotation = -(float)MathExt.AngleVector(GetEdge(vertices, coord).GetNormal());
            return transform;
        }

        public static Line GetEdge(IList<Vector2> vertices, IPolygonCoord coord)
        {
            Debug.Assert(vertices.Count >= 1, "Polygon must have at least 1 vertex.");
            return new Line(vertices[coord.EdgeIndex], vertices[(coord.EdgeIndex + 1) % vertices.Count]);
        }

        /// <summary>
        /// Changes handedness so polygon edge normals face inward.
        /// </summary>
        public static void SetExterior(List<Vector2> polygon)
        {
            MathExt.SetHandedness(polygon, true);
        }

        /// <summary>
        /// Changes handedness so polygon edge normals face outward.
        /// </summary>
        public static void SetInterior(List<Vector2> polygon)
        {
            MathExt.SetHandedness(polygon, false);
        }

        public static bool IsInterior(IList<Vector2> polygon)
        {
            return !MathExt.IsClockwise(polygon);
        }
    }
}
