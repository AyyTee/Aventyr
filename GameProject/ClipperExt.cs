using ClipperLib;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class ClipperExt
    {
        const double SCALE_FACTOR = 1 << 20;

        public static IntPoint ConvertToIntPoint(Vector2 v)
        {
            return new IntPoint(v.X * SCALE_FACTOR, v.Y * SCALE_FACTOR);
        }

        public static Vector2 ConvertToVector2(IntPoint point)
        {
            return new Vector2((float)(point.X / SCALE_FACTOR), (float)(point.Y / SCALE_FACTOR));
        }

        public static Vector2[] ConvertToVector2(List<IntPoint> point)
        {
            Vector2[] polygon = new Vector2[point.Count];
            for (int i = 0; i < point.Count; i++)
            {
                polygon[i] = ConvertToVector2(point[i]);
            }
            return polygon;
        }

        public static List<IntPoint> ConvertToIntPoint(Vector2[] vertices)
        {
            var path = new List<IntPoint>();
            for (int i = 0; i < vertices.Length; i++)
            {
                path.Add(ConvertToIntPoint(vertices[i]));
            }
            return path;
        }
    }
}
