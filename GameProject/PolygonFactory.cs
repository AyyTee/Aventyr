using OpenTK;
using Poly2Tri;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            try
            {
                P2T.Triangulate(polygon);
            }
            catch
            {
                Trace.TraceWarning("Polygon failed to triangulate.");
                return null;
            }
            return polygon;
        }
    }
}
