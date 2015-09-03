using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
