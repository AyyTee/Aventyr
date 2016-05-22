using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class ReadOnlyMesh : IMesh
    {
        public readonly ReadOnlyCollection<Vertex> Vertices;
        public readonly ReadOnlyCollection<int> Indices;

        public ReadOnlyMesh(IMesh mesh)
            : this(mesh.GetVertices(), mesh.GetIndices())
        {
        }

        public ReadOnlyMesh(IList<Vertex> vertices, IList<int> indices)
        {
            Vertices = new ReadOnlyCollection<Vertex>(vertices);
            Indices = new ReadOnlyCollection<int>(indices);
            Debug.Assert(Indices.Count == 0 || Indices.Max() < Vertices.Count);
            Debug.Assert(Indices.Count % 3 == 0);
        }

        public ReadOnlyMesh(IEnumerable<Triangle> triangles)
        {
            List<Vertex> vertices = new List<Vertex>();
            List<int> triangleIndices = new List<int>();

            foreach (Triangle t in triangles)
            {
                int[] triangle = new int[3];
                for (int j = 0; j < 3; j++)
                {
                    Vertex v = t[0].ShallowClone();
                    int index = vertices.FindIndex(item => v.Equals(item));
                    if (index != -1)
                    {
                        triangle[j] = index;
                    }
                    else
                    {
                        triangle[j] = vertices.Count - 1;
                    }
                }
                triangleIndices.AddRange(triangle);
            }

            Vertices = new ReadOnlyCollection<Vertex>(vertices);
            Indices = new ReadOnlyCollection<int>(triangleIndices);
        }

        public List<Vertex> GetVertices()
        {
            return new List<Vertex>(Vertices);
        }

        public List<int> GetIndices()
        {
            return new List<int>(Indices);
        }

        public IMesh ShallowClone()
        {
            return new ReadOnlyMesh(Vertices, Indices);
        }
    }
}
