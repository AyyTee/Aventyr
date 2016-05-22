using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xna = Microsoft.Xna.Framework;

namespace Game
{
    public class Vector3Ext
    {
        public static Vector3[] Transform(Vector3[] vectors, Matrix4 matrix)
        {
            Vector3[] vList = new Vector3[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                vList[i] = Vector3.Transform(vectors[i], matrix);
            }
            return vList;
        }

        public static Vector3[] Transform(Vector3[] vectors, Matrix4d matrix)
        {
            Vector3[] vList = new Vector3[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                vList[i] = (Vector3)Vector3d.Transform(new Vector3d(vectors[i].X, vectors[i].Y, vectors[i].Z), matrix);
            }
            return vList;
        }

        public static Vector3 Transform(Vector3 vectors, Matrix4d matrix)
        {
            return (Vector3)Vector3d.Transform(new Vector3d(vectors.X, vectors.Y, vectors.Z), matrix);
        }

        public static List<Vector3> Transform(IEnumerable<Vector3> vectors, Matrix4 matrix)
        {
            List<Vector3> vList = new List<Vector3>();
            foreach (Vector3 v in vectors)
            {
                vList.Add(Vector3.Transform(v, matrix));
            }
            return vList;
        }

        public static Vector3 Transform(Vector3 vector, Matrix3 matrix)
        {
            Matrix4 mat = Matrix4.Identity;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    mat[i, j] = matrix[i, j];
                }
            }
            return Vector3.Transform(vector, mat);
        }

        public static Vector3 Transform(Vector3 vector, Matrix3d matrix)
        {
            Matrix4d mat = Matrix4d.Identity;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    mat[i, j] = matrix[i, j];
                }
            }
            return (Vector3)Vector3d.Transform(new Vector3d(vector.X, vector.Y, vector.Z), mat);
        }

        public static Xna.Vector2 ConvertToXna(Vector3 v)
        {
            return new Xna.Vector2(v.X, v.Y);
        }

        public static Xna.Vector2[] ConvertToXna(Vector3[] v)
        {
            Xna.Vector2[] vNew = new Xna.Vector2[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vNew[i] = ConvertToXna(v[i]);
            }
            return vNew;
        }

        public static bool IsNaN(Vector3 v)
        {
            return Double.IsNaN(v.X) || Double.IsNaN(v.Y) || Double.IsNaN(v.Z);
        }
    }
}
