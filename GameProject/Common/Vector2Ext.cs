using OpenTK;
using Poly2Tri;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xna = Microsoft.Xna.Framework;

namespace Game
{
    public class Vector2Ext
    {
        const float EQUALITY_EPSILON = 0.0001f;

        public static float Cross(Vector2 v0, Vector2 v1)
        {
            return v0.X * v1.Y - v0.Y * v1.X;
        }

        public static double Cross(Vector2d v0, Vector2d v1)
        {
            return v0.X * v1.Y - v0.Y * v1.X;
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
                vList[i] = Vector2Ext.Transform(vectors[i], matrix);
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

        public static Vector2 ToOtk(Point2D v)
        {
            return new Vector2(v.Xf, v.Yf);
        }

        public static Vector2 ToOtk(System.Drawing.Size v)
        {
            return new Vector2(v.Width, v.Height);
        }

        public static Vector2 ToOtk(Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }

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
                vNew[i] = ToOtk(v[i]);
            }
            return vNew;
        }

        public static Vector2 ToOtk(TriangulationPoint v)
        {
            return new Vector2((float)v.X, (float)v.Y);
        }

        public static Vector2 ToOtk(PolygonPoint v)
        {
            return new Vector2((float)v.X, (float)v.Y);
        }

        public static Vector2 ToOtk(Xna.Vector2 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static Vector2[] ToOtk(Xna.Vector2[] v)
        {
            Debug.Assert(v != null);
            Vector2[] vNew = new Vector2[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vNew[i] = ToOtk(v[i]);
            }
            return vNew;
        }

        public static Xna.Vector2 ToXna(Vector2 v)
        {
            return new Xna.Vector2(v.X, v.Y);
        }

        public static Xna.Vector2[] ToXna(Vector2[] v)
        {
            Debug.Assert(v != null);
            Xna.Vector2[] vNew = new Xna.Vector2[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vNew[i] = ToXna(v[i]);
            }
            return vNew;
        }

        public static List<Xna.Vector2> ToXna(List<Vector2> v)
        {
            Debug.Assert(v != null);
            List<Xna.Vector2> vNew = new List<Xna.Vector2>();
            for (int i = 0; i < v.Count; i++)
            {
                vNew.Add(ToXna(v[i]));
            }
            return vNew;
        }

        public static Xna.Vector2 ToXna(TriangulationPoint v)
        {
            return new Xna.Vector2((float)v.X, (float)v.Y);
        }

        public static bool IsNaN(Vector2 v)
        {
            return double.IsNaN(v.X) || double.IsNaN(v.Y);
        }

        public static bool IsNaN(Vector2d v)
        {
            return double.IsNaN(v.X) || double.IsNaN(v.Y);
        }

        public static bool IsReal(Vector2 v)
        {
            return !IsNaN(v) && !float.IsPositiveInfinity(v.X) && 
                !float.IsNegativeInfinity(v.X) && 
                !float.IsPositiveInfinity(v.Y) && 
                !float.IsNegativeInfinity(v.Y);
        }

        public static bool IsReal(Vector2d v)
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
            return AlmostEqual(v0, v1, EQUALITY_EPSILON);
        }

        public static bool AlmostEqual(Vector2d v0, Vector2d v1, double delta)
        {
            return Math.Abs(v0.X - v1.X) <= delta && Math.Abs(v0.X - v1.X) <= delta;
        }

        public static bool AlmostEqual(Vector2d v0, Vector2d v1)
        {
            return AlmostEqual(v0, v1, EQUALITY_EPSILON);
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
