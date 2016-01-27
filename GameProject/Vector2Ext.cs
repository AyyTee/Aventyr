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
        public static float Cross(Vector2 v0, Vector2 v1)
        {
            return v0.X * v1.Y - v0.Y * v1.X;
        }
        public static Vector2 Transform(Vector2 vector, Matrix4 matrix)
        {
            Vector3 v = Vector3.Transform(new Vector3(vector.X, vector.Y, 1), matrix);
            return new Vector2(v.X, v.Y);
        }

        public static Vector2[] Transform(Vector2[] vectors, Matrix2 matrix)
        {
            Vector2[] vList = new Vector2[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                vList[i] = MathExt.Matrix2Mult(vectors[i], matrix);
            }
            return vList;
        }

        public static List<Vector2> Transform(List<Vector2> vectors, Matrix2 matrix)
        {
            List<Vector2> vList = new List<Vector2>();
            foreach (Vector2 v in vectors)
            {
                vList.Add(MathExt.Matrix2Mult(v, matrix));
            }
            return vList;
        }

        public static Vector2[] Transform(Vector2[] vectors, Matrix4 matrix)
        {
            Vector2[] vList = new Vector2[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                vList[i] = Vector2Ext.Transform(vectors[i], matrix);
            }
            return vList;
        }

        public static List<Vector2> Transform(List<Vector2> vectors, Matrix4 matrix)
        {
            List<Vector2> vList = new List<Vector2>();
            foreach (Vector2 v in vectors)
            {
                vList.Add(Vector2Ext.Transform(v, matrix));
            }
            return vList;
        }

        public static Vector2 ConvertTo(Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static Vector2[] ConvertTo(FarseerPhysics.Common.Vertices v)
        {
            Vector2[] vList = new Vector2[v.Count];
            for (int i = 0; i < vList.Length; i++)
            {
                vList[i] = new Vector2(v[i].X, v[i].Y);
            }
            return vList;
        }

        public static Vector2[] ConvertTo(Vector3[] v)
        {
            Vector2[] vNew = new Vector2[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vNew[i] = ConvertTo(v[i]);
            }
            return vNew;
        }

        public static Vector2 ConvertTo(TriangulationPoint v)
        {
            return new Vector2((float)v.X, (float)v.Y);
        }

        public static Vector2 ConvertTo(PolygonPoint v)
        {
            return new Vector2((float)v.X, (float)v.Y);
        }

        public static Vector2 ConvertTo(Xna.Vector2 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static Vector2[] ConvertTo(Xna.Vector2[] v)
        {
            Vector2[] vNew = new Vector2[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vNew[i] = ConvertTo(v[i]);
            }
            return vNew;
        }

        public static Xna.Vector2 ConvertToXna(Vector2 v)
        {
            return new Xna.Vector2(v.X, v.Y);
        }

        public static Xna.Vector2[] ConvertToXna(Vector2[] v)
        {
            Xna.Vector2[] vNew = new Xna.Vector2[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vNew[i] = ConvertToXna(v[i]);
            }
            return vNew;
        }

        public static Xna.Vector2 ConvertToXna(TriangulationPoint v)
        {
            return new Xna.Vector2((float)v.X, (float)v.Y);
        }

        public static bool IsNaN(Vector2 v)
        {
            return Double.IsNaN(v.X) || Double.IsNaN(v.Y);
        }

        public static bool IsReal(Vector2 v)
        {
            return !IsNaN(v) && !float.IsPositiveInfinity(v.X) && 
                !float.IsNegativeInfinity(v.X) && 
                !float.IsPositiveInfinity(v.Y) && 
                !float.IsNegativeInfinity(v.Y);
        }
    }
}
