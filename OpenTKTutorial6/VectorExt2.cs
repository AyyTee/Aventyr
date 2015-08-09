using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    public class VectorExt2
    {
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
                vList[i] = VectorExt2.Transform(vectors[i], matrix);
            }
            return vList;
        }

        public static List<Vector2> Transform(List<Vector2> vectors, Matrix4 matrix)
        {
            List<Vector2> vList = new List<Vector2>();
            foreach (Vector2 v in vectors)
            {
                vList.Add(VectorExt2.Transform(v, matrix));
            }
            return vList;
        }
    }
}
