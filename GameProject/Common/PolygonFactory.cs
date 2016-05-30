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
        public static Poly2Tri.Polygon CreatePolygon(IList<Vector2> vertices)
        {
            Poly2Tri.Polygon polygon = GetPolygon(vertices);
            if (Triangulate(polygon))
            {
                return polygon;
            }
            return null;
        }

        public static List<Poly2Tri.Polygon> CreatePolygon(List<List<IntPoint>> paths)
        {
            List<List<IntPoint>> holes = new List<List<IntPoint>>(paths);
            Dictionary<List<IntPoint>, Poly2Tri.Polygon> contourMap = new Dictionary<List<IntPoint>, Poly2Tri.Polygon>();
            List<Poly2Tri.Polygon> polygons = new List<Poly2Tri.Polygon>();
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
                    Poly2Tri.Polygon polygon = GetPolygon(p);
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
                        Poly2Tri.Polygon polygon = contourMap[contours[i]];
                        polygon.AddHole(GetPolygon(contours[i]));
                        break;
                    }
                }
            }
            foreach (Poly2Tri.Polygon p in polygons)
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

        public static List<Poly2Tri.Polygon> CreatePolygon(PolyTree polyTree)
        {
            List<Poly2Tri.Polygon> polyList = new List<Poly2Tri.Polygon>();
            for (int i = 0; i < polyTree.Childs.Count; i++)
            {
                polyList.AddRange(CreatePolygon(polyTree.Childs[i]));
            }
            return polyList;
        }

        private static List<Poly2Tri.Polygon> CreatePolygon(PolyNode polyNode)
        {
            List<Poly2Tri.Polygon> polyList = new List<Poly2Tri.Polygon>();
            Debug.Assert(polyNode.IsOpen == false);
            Debug.Assert(polyNode.IsHole == false);
            Poly2Tri.Polygon polygon = GetPolygon(polyNode.Contour);
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

        private static Poly2Tri.Polygon GetPolygon(List<IntPoint> vertices)
        {
            HashSet<Vector2> points = new HashSet<Vector2>();
            List<PolygonPoint> polygonPoints = new List<PolygonPoint>();
            for (int i = 0; i < vertices.Count(); i++)
            {
                Vector2 v = ClipperConvert.ToVector2(vertices[i]);
                polygonPoints.Add(new PolygonPoint(v.X, v.Y));
                Debug.Assert(points.Add(v));
            }
            return new Poly2Tri.Polygon(polygonPoints);
        }

        private static Poly2Tri.Polygon GetPolygon(IList<Vector2> vertices)
        {
            HashSet<Vector2> points = new HashSet<Vector2>();
            List<PolygonPoint> polygonPoints = new List<PolygonPoint>();
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 v = vertices[i];
                polygonPoints.Add(new PolygonPoint(v.X, v.Y));
                Debug.Assert(points.Add(v));
            }
            return new Poly2Tri.Polygon(polygonPoints);
        }

        /// <summary>Triangulate a polygon and return whether it was successful.</summary>
        private static bool Triangulate(Poly2Tri.Polygon polygon)
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
            return CreateLineWidth(line, width, width);
        }

        public static Vector2[] CreateLineWidth(Line line, float widthStart, float widthEnd)
        {
            Debug.Assert(widthStart > 0 && widthEnd > 0, "Line must have positive width.");
            Vector2 offsetStart = (line[0] - line[1]).PerpendicularLeft.Normalized() * widthStart / 2;
            Vector2 offsetEnd = (line[0] - line[1]).PerpendicularLeft.Normalized() * widthEnd / 2;

            Vector2[] lineWidth = new Vector2[] {
                line[0] - offsetStart,
                line[0] + offsetStart,
                line[1] + offsetEnd,
                line[1] - offsetEnd
            };
            Debug.Assert(PolygonExt.IsInterior(lineWidth));
            return lineWidth;
        }

        public static Vector2[] CreateRectangle()
        {
            return CreateRectangle(1, 1);
        }

        public static Vector2[] CreateRectangle(float width, float height)
        {
            return CreateRectangle(width, height, new Vector2());
        }

        public static Vector2[] CreateRectangle(float width, float height, Vector2 origin)
        {
            Vector2[] rectangle = new Vector2[] {
                new Vector2(width/2, height/2) + origin,
                new Vector2(-width/2, height/2) + origin,
                new Vector2(-width/2, -height/2) + origin,
                new Vector2(width/2, -height/2) + origin
            };
            Debug.Assert(PolygonExt.IsInterior(rectangle));
            return rectangle;
        }

        public static Vector2[] CreateNGon(int sides, float scale, Vector2 origin)
        {
            Debug.Assert(sides >= 3);
            Vector2[] vertices = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                double angle = i / (MathExt.TAU * sides);
                float x, y;
                x = (float)Math.Cos(angle);
                y = (float)Math.Sin(angle);
                vertices[i] = new Vector2(x, y) * scale + origin;
            }
            Debug.Assert(PolygonExt.IsInterior(vertices));
            return vertices;
        }
    }
}
