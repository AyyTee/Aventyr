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
            HashSet<Vector2> points = new HashSet<Vector2>();
            List<PolygonPoint> polygonPoints = new List<PolygonPoint>();
            for (int i = 0; i < vertices.Length; i++)
            {
                polygonPoints.Add(new PolygonPoint(vertices[i].X, vertices[i].Y));
                Debug.Assert(points.Add(vertices[i]));
            }
            Polygon polygon = new Polygon(polygonPoints);

            TextWriter console = Console.Out;
            Console.SetOut(Controller.Log);
            try
            {
                P2T.Triangulate(polygon);
                Console.SetOut(console);
                return polygon;
            }
            catch
            {
                Console.SetOut(console);
                Trace.TraceWarning("Polygon failed to triangulate.");
                return null;
            }
        }
    }
}
