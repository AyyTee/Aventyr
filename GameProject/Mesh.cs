using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Mesh : IMesh
    {
        public List<Vertex> Vertices = new List<Vertex>();
        public List<int> Indices = new List<int>();

        public Mesh()
        {
        }

        public List<Vertex> GetVertices()
        {
            return Vertices;
        }

        public List<int> GetIndices()
        {
            Debug.Assert(Indices.Max() < Vertices.Count() - 1);
            return Indices;
        }
    }
}
