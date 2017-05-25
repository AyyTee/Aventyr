using System.Collections.ObjectModel;
using System.Diagnostics;
using Game.Serialization;
using OpenTK;
using System.Collections.Generic;
using Game.Common;

namespace Game.Models
{
    /// <summary>
    /// Immutable class that represents a triangle in 3 dimensions.
    /// </summary>
    public class Triangle : IShallowClone<Triangle>
    {
        public Vertex V0 => Vertices[0];
        public Vertex V1 => Vertices[1];
        public Vertex V2 => Vertices[2];
        public readonly ReadOnlyCollection<Vertex> Vertices;
        public const int VertexCount = 3;

        public Triangle(Vertex v0, Vertex v1, Vertex v2)
        {
            Vertices = new ReadOnlyCollection<Vertex>(new[] { v0, v1, v2 });
        }

        public Triangle ShallowClone() => new Triangle(this[0], this[1], this[2]);

        public Vertex this[int index] => Vertices[index];

        /// <summary>
        /// Creates a clone of this triangle with the order of the vertices reversed.
        /// </summary>
        public Triangle Reverse() => new Triangle(this[2], this[1], this[0]);

        public Triangle Transform(Matrix4 transform)
        {
            return new Triangle(
                this[0].Transform(transform),
                this[1].Transform(transform),
                this[2].Transform(transform)
                );
        }

        /// <summary>
        /// Acts the same as equals but allows for vertices to be in a different order (but not reverse order).
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

        /// <summary>
        /// Bisects a 3d triangle with a plane perpendicular to the xy-plane.
        /// </summary>
        /// <param name="this">Triangle to bisect.</param>
        /// <param name="bisector">Bisection plane defined by a line on the xy-plane.</param>
        /// <param name="keepSide">Which side of the bisector to not remove from the triangle.</param>
        /// <returns>Triangles not removed by the bisection.  Will either be 0,1,2 triangles.</returns>
        public Triangle[] Bisect(LineF bisector, Side keepSide = Side.Left)
        {
            Debug.Assert(bisector != null);
            Debug.Assert(keepSide != Side.Neither);
            Vector2[] vertices = {
                new Vector2(this[0].Position.X, this[0].Position.Y),
                new Vector2(this[1].Position.X, this[1].Position.Y),
                new Vector2(this[2].Position.X, this[2].Position.Y)
            };

            var keep = new List<Vertex>();
            int intersectCount = 0;
            for (int i = 0; i < VertexCount; i++)
            {
                Side side = bisector.GetSideOf(vertices[i], false);
                if (side == keepSide || side == Side.Neither)
                {
                    keep.Add(this[i]);
                }
                var edge = new LineF(vertices[i], vertices[(i + 1) % VertexCount]);
                IntersectCoord intersect = MathEx.LineLineIntersect(edge, bisector, false);
                if (intersect != null && intersect.First > 0 && intersect.First < 1)
                {
                    intersectCount++;
                    Debug.Assert(intersectCount <= 2);
                    int index = (i + 1) % VertexCount;
                    keep.Add(Vertex.Lerp(this[i], this[index], (float)intersect.First));
                }
            }

            Debug.Assert(keep.Count <= 4);
            if (keep.Count == 3)
            {
                return new[]
                {
                    new Triangle(keep[0], keep[1], keep[2])
                };
            }
            if (keep.Count == 4)
            {
                return new[]
                {
                    new Triangle(keep[0], keep[1], keep[2]),
                    new Triangle(keep[0], keep[2], keep[3])
                };
            }
            return new Triangle[0];
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

        public bool Equals(Triangle triangle) => Equals(this, triangle);

        public override int GetHashCode() => this[0].GetHashCode() ^ this[1].GetHashCode() ^ this[2].GetHashCode();

        public static bool operator ==(Triangle t0, Triangle t1) => t0.Equals(t1);

        public static bool operator !=(Triangle t0, Triangle t1) => !t0.Equals(t1);
    }
}
