using ClipperLib;
using OpenTK;
using Poly2Tri;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class PolygonFactory
    {

        public static Polygon CreatePolygon(Vector2[] vertices)
        {
            Polygon polygon = GetPolygon(vertices);
            if (Triangulate(polygon))
            {
                return polygon;
            }
            return null;
        }

        public static List<Polygon> CreatePolygon(List<List<IntPoint>> paths)
        {
            List<List<IntPoint>> holes = new List<List<IntPoint>>(paths);
            Dictionary<List<IntPoint>, Polygon> contourMap = new Dictionary<List<IntPoint>,Polygon>();
            List<Polygon> polygons = new List<Polygon>();
            foreach (List<IntPoint> p in paths)
            {
                if (p.Count == 0)
                {
                    holes.Remove(p);
                    continue;
                }
                Debug.Assert(p.Count != 1 && p.Count != 2, "Polygon is degenerate.");
                if (!MathExt.IsClockwise(p))
                {
                    Polygon polygon = GetPolygon(p);
                    polygons.Add(polygon);
                    holes.Remove(p);
                    contourMap.Add(p, polygon);
                }
            }
            foreach (List<IntPoint> p in holes)
            {
                List<List<IntPoint>> contours = contourMap.Keys.ToList();
                for (int i = 0; i < contours.Count; i++)
                {
                    if (IsHole(p, contours[i]))
                    {
                        Polygon polygon = contourMap[contours[i]];
                        polygon.AddHole(GetPolygon(contours[i]));
                        break;
                    }
                }
            }
            foreach (Polygon p in polygons)
            {
                Triangulate(p);
            }
            return polygons;
        }

        private static bool IsHole(List<IntPoint> hole, List<IntPoint> polygon)
        {
            for (int i = 0; i < hole.Count; i++)
            {
                int result = Clipper.PointInPolygon(hole[i], polygon);
                //if the point is on the edge then try another point
                if (result == -1)
                {
                    continue;
                }
                return result == 1;
            }
            throw new Exception("Invalid polygon, all vertices are collinear to another polygon.");
        }

        public static List<Polygon> CreatePolygon(PolyTree polyTree)
        {
            List<Polygon> polyList = new List<Polygon>();
            for (int i = 0; i < polyTree.Childs.Count; i++)
            {
                polyList.AddRange(CreatePolygon(polyTree.Childs[i]));
            }
            return polyList;
        }

        private static List<Polygon> CreatePolygon(PolyNode polyNode)
        {
            List<Polygon> polyList = new List<Polygon>();
            Debug.Assert(polyNode.IsOpen == false);
            Debug.Assert(polyNode.IsHole == false);
            Polygon polygon = GetPolygon(polyNode.Contour);
            for (int i = 0; i < polyNode.Childs.Count; i++)
            {
                if (polyNode.Childs[i].IsHole)
                {
                    polygon.AddHole(GetPolygon(polyNode.Childs[i].Contour));
                }
                else
                {
                    polyList.AddRange(CreatePolygon(polyNode.Childs[i]));
                }
            }
            if (Triangulate(polygon))
            {
                polyList.Add(polygon);
            }
            return polyList;
        }

        private static Polygon GetPolygon(List<IntPoint> vertices)
        {
            HashSet<Vector2> points = new HashSet<Vector2>();
            List<PolygonPoint> polygonPoints = new List<PolygonPoint>();
            for (int i = 0; i < vertices.Count(); i++)
            {
                Vector2 v = ClipperConvert.ToVector2(vertices[i]);
                polygonPoints.Add(new PolygonPoint(v.X, v.Y));
                Debug.Assert(points.Add(v));
            }
            return new Polygon(polygonPoints);
        }

        private static Polygon GetPolygon(Vector2[] vertices)
        {
            HashSet<Vector2> points = new HashSet<Vector2>();
            List<PolygonPoint> polygonPoints = new List<PolygonPoint>();
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 v = vertices[i];
                polygonPoints.Add(new PolygonPoint(v.X, v.Y));
                Debug.Assert(points.Add(v));
            }
            return new Polygon(polygonPoints);
        }

        /// <summary>Triangulate a polygon and return whether it was successful.</summary>
        private static bool Triangulate(Polygon polygon)
        {
            TextWriter console = Console.Out;
            Console.SetOut(Controller.Log);
            try
            {
                P2T.Triangulate(polygon);
                Console.SetOut(console);
                return true;
            }
            catch (Exception ex)
            {
                if (ex.Message != "Error marking neighbors -- t doesn't contain edge p1-p2!")
                {
                    Console.SetOut(console);
                    Trace.TraceWarning("Polygon failed to triangulate.");
                }
                return false;
            }
        }

        public static Vector2[] CreateLineWidth(Line line, float width)
        {
            return CreateLineWidth(line[0], line[1], width, width);
        }

        public static Vector2[] CreateLineWidth(Line line, float widthStart, float widthEnd)
        {
            return CreateLineWidth(line[0], line[1], widthStart, widthEnd);
        }

        public static Vector2[] CreateLineWidth(Vector2 vStart, Vector2 vEnd, float width)
        {
            return CreateLineWidth(vStart, vEnd, width, width);
        }

        public static Vector2[] CreateLineWidth(Vector2 vStart, Vector2 vEnd, float widthStart, float widthEnd)
        {
            Vector2 offsetStart = (vStart - vEnd).PerpendicularLeft.Normalized() * widthStart / 2;
            Vector2 offsetEnd = (vStart - vEnd).PerpendicularLeft.Normalized() * widthEnd / 2;
            return new Vector2[] {
                vStart + offsetStart,
                vStart - offsetStart,
                vEnd - offsetEnd,
                vEnd + offsetEnd
            };
        }
    }
}
