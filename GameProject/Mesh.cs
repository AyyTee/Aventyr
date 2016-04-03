using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using OpenTK;

namespace Game
{
    [DataContract]
    public class Mesh : IMesh
    {
        [DataMember]
        public List<Vertex> Vertices = new List<Vertex>();
        [DataMember]
        public List<int> Indices = new List<int>();

        public Mesh()
        {
        }

        public Mesh(IList<Vertex> vertices, IList<int> indices)
        {
            Vertices = vertices.ToList();
            Indices = indices.ToList();
        }

        /// <summary>
        /// Adds a vertex and returns the index of that vertex.
        /// </summary>
        public int AddVertex(Vertex v)
        {
            Vertices.Add(v);
            return Vertices.Count - 1;
        }

        /// <summary>
        /// Adds multiple vertices and returns the index of the first vertex added.
        /// </summary>
        public int AddVertexRange(IList<Vertex> v)
        {
            int count = Vertices.Count;
            Vertices.AddRange(v);
            return count;
        }

        public void AddTriangle(int i0, int i1, int i2)
        {
            Indices.Add(i0);
            Indices.Add(i1);
            Indices.Add(i2);
        }

        public void AddTriangle(Triangle triangle)
        {
            int index = AddVertexRange(triangle.Vertices);
            Indices.AddRange(new[] { index, index + 1, index + 2 });
        }

        public void AddTriangleRange(IList<Triangle> triangle)
        {
            for (int i = 0; i < triangle.Count; i++)
            {
                AddTriangle(triangle[i]);
            }
        }

        public void Transform(Matrix4 transform)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = Vertices[i].Transform(transform);
            }
        }

        public List<Vertex> GetVertices()
        {
            return Vertices;
        }

        public List<int> GetIndices()
        {
            Debug.Assert(Indices.Count == 0 || Indices.Max() < Vertices.Count());
            return Indices;
        }

        public IMesh ShallowClone()
        {
            return new Mesh(Vertices, Indices);
        }
    }
}
