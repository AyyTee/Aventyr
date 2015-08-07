using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Poly2Tri;

namespace Game
{
    /// <summary>
    /// A polygon with no self intersections and only one boundary (no holes or disconnected parts)
    /// </summary>
    /*public class PolygonSimple : Model
    {
        private List<Vector2> Vertices = new List<Vector2>();
        private List<Poly2Tri.Polygon> Triangles = new List<Poly2Tri.Polygon>();
        /// <summary>
        /// indicates whether the Vertices list no longer matches the Triangles list
        /// </summary>
        private bool isChanged = false;

        public PolygonSimple()
        {
        }

        public PolygonSimple(Vector2[] vertices)
        {
            AddVertices(vertices);
        }

        public void AddVertices(Vector2 vertex)
        {
            Vertices.Add(vertex);
            isChanged = true;
        }

        public void AddVertices(Vector2[] vertices)
        {
            Vertices.AddRange(vertices);
            isChanged = true;
        }

        public override Vector3[] GetVerts()
        {
            Vector3[] verts = new Vector3[Vertices.Count];
            for (int i = 0; i < Vertices.Count; i++)
            {
                verts[i] = new Vector3(Vertices[i].X, Vertices[i].Y, 0);
            }
            return verts;
        }

        public override Vector3[] GetColorData()
        {
            throw new NotImplementedException();
        }

        public override int[] GetIndices(int offset = 0)
        {
            Triangles[0].
        }

        public override Vector2[] GetTextureCoords()
        {
            throw new NotImplementedException();
        }

        private void Tesselate(List<int> Exteriors)
        {
            PolygonPoint[] verts = new PolygonPoint[Vertices.Count];
            Triangles.Clear();
            for (int i = 0; i < Vertices.Count(); i++)
            {
                verts[i] = new PolygonPoint(Vertices[i].X, Vertices[i].Y);
            }
            Poly2Tri.Polygon Poly = new Poly2Tri.Polygon(verts);
            Poly.
            Triangles.Add(Poly);
            P2T.Triangulate(Poly);
        }
    }*/
}
