using OpenTK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    /// <summary>
    /// Immutable class that represents a triangle in 3 dimensions.
    /// </summary>
    public class Triangle : IShallowClone<Triangle>
    {
        public Vertex V0 { get { return Vertices[0]; } }
        public Vertex V1 { get { return Vertices[1]; } }
        public Vertex V2 { get { return Vertices[2]; } }
        public readonly ReadOnlyCollection<Vertex> Vertices;
        public const int VERTEX_COUNT = 3;

        public Triangle(Vertex v0, Vertex v1, Vertex v2)
        {
            Vertices = new ReadOnlyCollection<Vertex>(new[] { v0, v1, v2 });
        }

        public Triangle ShallowClone()
        {
            return new Triangle(this[0], this[1], this[2]);
        }

        public Vertex this[int index]
        {
            get
            {
                return Vertices[index];
            }
        }

        /// <summary>
        /// Creates a clone of this triangle with the order of the vertices reversed.
        /// </summary>
        public Triangle Reverse()
        {
            return new Triangle(this[2], this[1], this[0]);
        }

        public Triangle Transform(Matrix4 transform)
        {
            return new Triangle(
                this[0].Transform(transform),
                this[1].Transform(transform),
                this[2].Transform(transform)
                );
        }

        /// <summary>
        /// Acts the same as equals but allows for vertices to be in a different order as long as handedness is preserved.
        /// </summary>
        public static bool IsIsomorphic(Triangle t0, Triangle t1)
        {
            if (((object)t0) == null && ((object)t1) == null)
            {
                return true;
            }
            else if (((object)t0) == null || ((object)t1) == null)
            {
                return false;
            }

            if (t0[0].Equals(t1[0]) && t0[1].Equals(t1[1]) && t0[2].Equals(t1[2]))
            {
                return true;
            }
            else if (t0[0].Equals(t1[1]) && t0[1].Equals(t1[2]) && t0[2].Equals(t1[0]))
            {
                return true;
            }
            else if (t0[0].Equals(t1[2]) && t0[1].Equals(t1[0]) && t0[2].Equals(t1[1]))
            {
                return true;
            }
            return false;
        }

        public static bool Equals(Triangle t0, Triangle t1)
        {
            if (((object)t0) == null && ((object)t1) == null)
            {
                return true;
            }
            else if (((object)t0) == null || ((object)t1) == null)
            {
                return false;
            }

            if (t0[0].Equals(t1[0]) && t0[1].Equals(t1[1]) && t0[2].Equals(t1[2]))
            {
                Debug.Assert(t0.GetHashCode() == t1.GetHashCode());
                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is Triangle)
            {
                return Equals(this, (Triangle)obj);
            }
            return false;
        }

        public bool Equals(Triangle triangle)
        {
            return Equals(this, triangle);
        }

        public override int GetHashCode()
        {
            return this[0].GetHashCode() ^ this[1].GetHashCode() ^ this[2].GetHashCode();
        }

        public static bool operator ==(Triangle t0, Triangle t1)
        {
            return t0.Equals(t1);
        }

        public static bool operator !=(Triangle t0, Triangle t1)
        {
            return !t0.Equals(t1);
        }
    }
}
