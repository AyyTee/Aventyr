using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class ReadOnlyMesh
    {
        private struct Indices
        {
            readonly int[] indices;
            public Indices(int i0, int i1, int i2)
            {
                indices = new int[3] {i0, i1, i2};
            }
        }

        public readonly ReadOnlyCollection<Vertex> Vertices;
        public readonly ReadOnlyCollection<Triangle> Triangles;

        public ReadOnlyMesh(Triangle[] Triangles)
        {
            List<Vertex> vertices = new List<Vertex>();
            List<Triangle> triangles = new List<Triangle>();

            for (int i = 0; i < Triangles.Length; i++)
            {
                int[] triangle = new int[3];
                for (int j = 0; j < 3; j++)
                {
                    Vertex v = Triangles[i][0].ShallowClone();
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
            }

            Vertices = new ReadOnlyCollection<Vertex>(vertices);
        }
    }
}
