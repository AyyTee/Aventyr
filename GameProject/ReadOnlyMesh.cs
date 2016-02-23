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
            #region constructors
            public Indices(int i0, int i1, int i2)
            {
                indices = new int[3] {i0, i1, i2};
            }

            public Indices(int[] indices)
            {
                this.indices = new int[3] 
                {
                    indices[0],
                    indices[1],
                    indices[2]
                };
            }
            #endregion
            public int this[int index] { get { return indices[index]; } }
        }

        public readonly ReadOnlyCollection<Vertex> Vertices;
        public readonly List<int> Triangles;

        public ReadOnlyMesh(Triangle[] triangles)
        {
            List<Vertex> vertices = new List<Vertex>();
            List<Indices> indices = new List<Indices>();

            for (int i = 0; i < triangles.Length; i++)
            {
                int[] triangle = new int[3];
                for (int j = 0; j < 3; j++)
                {
                    Vertex v = triangles[i][0].ShallowClone();
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
                Triangles.AddRange(triangle);
            }

            Vertices = new ReadOnlyCollection<Vertex>(vertices);
        }

        public int[] GetTriangles()
        {
            return Triangles.ToArray();
        }
    }
}
