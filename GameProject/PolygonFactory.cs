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
            PolygonPoint[] polygonPoints = new PolygonPoint[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                polygonPoints[i] = new PolygonPoint(vertices[i].X, vertices[i].Y);
            }
            Polygon polygon = new Polygon(polygonPoints);

            TextWriter console = Console.Out;
            Console.SetOut(Controller.Log);
            try
            {
                P2T.Triangulate(polygon);
            }
            catch
            {
                Console.SetOut(console);
                Trace.TraceWarning("Polygon failed to triangulate.");
                return null;
            }
            Console.SetOut(console);

            for (int i = 0; i < polygon.Points.Count; i++)
            {
                for (int j = 0; j < vertices.Length; j++)
                {
                    if (Vector2Ext.ConvertTo(polygon.Points[i]) == vertices[j])
                    {
                        break;
                    }
                    if (j == vertices.Length - 1)
                    {
                        Debug.Assert(false);
                    }
                }
            }
            return polygon;
        }
    }
}
