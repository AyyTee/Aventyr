using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public struct Triangle
    {
        public readonly Vertex[] Vertices;

        public Triangle(Vertex v0, Vertex v1, Vertex v2)
        {
            Vertices = new Vertex[3];
            Vertices[0] = v0;
            Vertices[1] = v1;
            Vertices[2] = v2;
        }

        /// <summary>
        /// Clone triangle and referenced vertices.
        /// </summary>
        public Triangle DeepClone()
        {
            return new Triangle(this[0].ShallowClone(), this[1].ShallowClone(), this[2].ShallowClone());
        }

        public Vertex this[int index]
        {
            get
            {
                return Vertices[index];
            }
        }
    }
}
