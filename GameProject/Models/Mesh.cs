using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using OpenTK;

namespace Game.Models
{
    [DataContract]
    public class Mesh : IMesh
    {
        [DataMember]
        public List<Vertex> Vertices = new List<Vertex>();
        [DataMember]
        public List<int> Indices = new List<int>();

        public bool IsTransparent => Vertices.Any(item => item.Color.A < 1);

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

        /// <summary>
        /// Remove all identical vertices.
        /// </summary>
        public void RemoveDuplicates()
        {
            List<Vertex> unique = new HashSet<Vertex>(Vertices).ToList();
            List<int> indicesOld = Indices;
            Indices = new List<int>();
            for (int i = 0; i < indicesOld.Count; i++)
            {
                int index = unique.FindIndex(item => item == Vertices[i]);
                Indices.Add(index);
            }
            Vertices = unique;
        }

        public void Transform(Matrix4 transform)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = Vertices[i].Transform(transform);
            }
        }

        public List<Vertex> GetVertices() => Vertices;

        public List<int> GetIndices() => Indices;

        public IMesh ShallowClone() => new Mesh(Vertices, Indices);

        public void AddMesh(Mesh mesh)
        {
            Indices.AddRange(mesh.GetIndices().Select(item => item + Vertices.Count));
            Vertices.AddRange(mesh.GetVertices());
        }
    }
}
