using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xna = Microsoft.Xna.Framework;

namespace Game
{
    public class VectorExt3
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

        public static List<Vector3> Transform(List<Vector3> vectors, Matrix4 matrix)
        {
            List<Vector3> vList = new List<Vector3>();
            foreach (Vector3 v in vectors)
            {
                vList.Add(Vector3.Transform(v, matrix));
            }
            return vList;
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
    }
}
