using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using Poly2Tri;
using Vector2 = OpenTK.Vector2;
using Vector3 = OpenTK.Vector3;
using Xna = Microsoft.Xna.Framework;

namespace Game.Common
{
    public static class Vector2Ex
    {
        const float EqualityEpsilon = 0.0001f;

        public static float Cross(Vector2 v0, Vector2 v1)
        {
            return v0.X * v1.Y - v0.Y * v1.X;
        }

        public static double Cross(Vector2d v0, Vector2d v1)
        {
            return v0.X * v1.Y - v0.Y * v1.X;
        }

        public static Vector2 LengthDir(double length, double direction)
        {
            return new Vector2(
                (float)(Math.Cos(direction) * length),
                (float)(Math.Sin(direction) * length)
                );
        }

        public static Vector2[] Scale(Vector2[] vectors, float scalar)
        {
            Debug.Assert(vectors != null);
            Vector2[] vList = new Vector2[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                vList[i] = vectors[i] * scalar;
            }
            return vList;
        }

        /// <summary>
        /// Get projection of this vector onto v1.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        public static Vector2 Project(this Vector2 v0, Vector2 v1)
        {
            var normal = v1.Normalized();
            return normal * Vector2.Dot(v0, normal);
        }

        /// <summary>
        /// Get projection of this vector onto v1.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        public static Vector2d Project(this Vector2d v0, Vector2d v1)
        {
            var normal = v1.Normalized();
            return normal * Vector2d.Dot(v0, normal);
        }

        /// <summary>
        /// Reflects this vector across a normal.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static Vector2 Mirror(this Vector2 v, Vector2 normal)
        {
            return v - 2 * (v - Project(v, normal));
        }

        /// <summary>
        /// Reflects this vector across a normal.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static Vector2d Mirror(this Vector2d v, Vector2d normal)
        {
            return v - 2 * (v - Project(v, normal));
        }

        public static Vector2 Round(this Vector2 vector, Vector2 roundBy)
        {
            var v = Vector2.Divide(vector, roundBy);
            return new Vector2((float)Math.Round(v.X), (float)Math.Round(v.Y)) * roundBy;
        }

        public static Vector2d Round(this Vector2d vector, Vector2d roundBy)
        {
            var v = Vector2d.Divide(vector, roundBy);
            return new Vector2d(Math.Round(v.X), Math.Round(v.Y)) * roundBy;
        }

        public static Vector2d Lerp(this Vector2d start, Vector2d end, double T) => start * (1 - T) + end * T;

        public static Vector2 Lerp(this Vector2 start, Vector2 end, float T) => start * (1 - T) + end * T;

        public static Vector2 Transform(Vector2 vector, Matrix4 matrix)
        {
            Vector3 v = Vector3.Transform(new Vector3(vector.X, vector.Y, 1), matrix);
            return new Vector2(v.X, v.Y);
        }

        public static Vector2 Transform(Vector2 vector, Matrix2 matrix)
        {
            return new Vector2(
                vector.X * matrix.M11 + vector.Y * matrix.M21, 
                vector.X * matrix.M12 + vector.Y * matrix.M22);
        }

        public static Vector2[] Transform(Vector2[] vectors, Matrix2 matrix)
        {
            Debug.Assert(vectors != null);
            Vector2[] vList = new Vector2[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                vList[i] = Transform(vectors[i], matrix);
            }
            return vList;
        }

        public static List<Vector2> Transform(IEnumerable<Vector2> vectors, Matrix2 matrix)
        {
            Debug.Assert(vectors != null);
            List<Vector2> vList = new List<Vector2>();
            foreach (Vector2 v in vectors)
            {
                vList.Add(Transform(v, matrix));
            }
            return vList;
        }

        public static Vector2[] Transform(Vector2[] vectors, Matrix4 matrix)
        {
            Debug.Assert(vectors != null);
            Vector2[] vList = new Vector2[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                vList[i] = Transform(vectors[i], matrix);
            }
            return vList;
        }

        public static List<Vector2> Transform(IEnumerable<Vector2> vectors, Matrix4 matrix)
        {
            Debug.Assert(vectors != null);
            List<Vector2> vList = new List<Vector2>();
            foreach (Vector2 v in vectors)
            {
                vList.Add(Transform(v, matrix));
            }
            return vList;
        }

        public static Vector2 Transform(Vector2 vector, Matrix4d matrix)
        {
            Vector3d v = Vector3d.Transform(new Vector3d(vector.X, vector.Y, 1), matrix);
            return new Vector2((float)v.X, (float)v.Y);
        }

        public static Vector2[] Transform(Vector2[] vectors, Matrix4d matrix)
        {
            Debug.Assert(vectors != null);
            Vector2[] vList = new Vector2[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                vList[i] = Transform(vectors[i], matrix);
            }
            return vList;
        }

        public static Vector2d Transform(Vector2d vector, Matrix4d matrix)
        {
            Vector3d v = Vector3d.Transform(new Vector3d(vector), matrix);
            return new Vector2d(v.X, v.Y);
        }

        public static Vector2d[] Transform(Vector2d[] vectors, Matrix4d matrix)
        {
            Debug.Assert(vectors != null);
            Vector2d[] vList = new Vector2d[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                vList[i] = Transform(vectors[i], matrix);
            }
            return vList;
        }

        public static Vector2 TransformVelocity(Vector2 vector, Matrix4 matrix)
        {
            Vector2[] v = {
                new Vector2(0, 0),
                vector
            };
            v = Transform(v, matrix);
            return v[1] - v[0];
        }

        public static Vector2 ToOtk(this Point2D v) => new Vector2(v.Xf, v.Yf);
        public static Vector2 ToOtk(this TriangulationPoint v) => new Vector2((float)v.X, (float)v.Y);
        public static Vector2 ToOtk(this PolygonPoint v) => new Vector2((float)v.X, (float)v.Y);

        public static Vector2[] ToOtk(FarseerPhysics.Common.Vertices v)
        {
            Vector2[] vList = new Vector2[v.Count];
            for (int i = 0; i < vList.Length; i++)
            {
                vList[i] = new Vector2(v[i].X, v[i].Y);
            }
            return vList;
        }

        public static Vector2[] ToOtk(Vector3[] v)
        {
            Debug.Assert(v != null);
            Vector2[] vNew = new Vector2[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vNew[i] = v[i].ToVector2();
            }
            return vNew;
        }

        public static Xna.Vector2 ToXna(this TriangulationPoint v) => new Xna.Vector2((float)v.X, (float)v.Y);

        public static bool IsNaN(this Vector2 v) => double.IsNaN(v.X) || double.IsNaN(v.Y);
        public static bool IsNaN(this Vector2d v) => double.IsNaN(v.X) || double.IsNaN(v.Y);

        public static bool IsReal(this Vector2 v)
        {
            return !IsNaN(v) && !float.IsPositiveInfinity(v.X) && 
                !float.IsNegativeInfinity(v.X) && 
                !float.IsPositiveInfinity(v.Y) && 
                !float.IsNegativeInfinity(v.Y);
        }

        public static bool IsReal(this Vector2d v)
        {
            return !IsNaN(v) && !double.IsPositiveInfinity(v.X) &&
                !double.IsNegativeInfinity(v.X) &&
                !double.IsPositiveInfinity(v.Y) &&
                !double.IsNegativeInfinity(v.Y);
        }

        public static bool AlmostEqual(Vector2 v0, Vector2 v1, float delta)
        {
            return Math.Abs(v0.X - v1.X) <= delta && Math.Abs(v0.X - v1.X) <= delta;
        }

        public static bool AlmostEqual(Vector2 v0, Vector2 v1)
        {
            return AlmostEqual(v0, v1, EqualityEpsilon);
        }

        public static bool AlmostEqual(Vector2d v0, Vector2d v1, double delta)
        {
            return Math.Abs(v0.X - v1.X) <= delta && Math.Abs(v0.X - v1.X) <= delta;
        }

        public static bool AlmostEqual(Vector2d v0, Vector2d v1)
        {
            return AlmostEqual(v0, v1, EqualityEpsilon);
        }

        public static Vector2d[] ToDouble(Vector2[] v)
        {
            Vector2d[] vArray = new Vector2d[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vArray[i] = (Vector2d)v[i];
            }
            return vArray;
        }

        public static Vector2[] ToSingle(Vector2d[] v)
        {
            Vector2[] vArray = new Vector2[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vArray[i] = (Vector2)v[i];
            }
            return vArray;
        }
    }
}
