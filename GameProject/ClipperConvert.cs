using ClipperLib;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class ClipperConvert
    {
        public const double SCALE_FACTOR = 1 << 20;

        public static Vector2[] ToVector2(List<IntPoint> point)
        {
            Vector2[] polygon = new Vector2[point.Count];
            for (int i = 0; i < point.Count; i++)
            {
                polygon[i] = ToVector2(point[i]);
            }
            return polygon;
        }

        public static Vector3[] ToVector3(List<IntPoint> point)
        {
            Vector3[] polygon = new Vector3[point.Count];
            for (int i = 0; i < point.Count; i++)
            {
                polygon[i] = ToVector3(point[i]);
            }
            return polygon;
        }

        public static List<IntPoint> ToIntPoint(Vector2[] vertices)
        {
            var path = new List<IntPoint>();
            for (int i = 0; i < vertices.Length; i++)
            {
                IntPoint point = ToIntPoint(vertices[i]);
                point.Z = i;
                path.Add(point);
            }
            return path;
        }

        public static List<IntPoint> ToIntPoint(Vector3[] vertices)
        {
            var path = new List<IntPoint>();
            for (int i = 0; i < vertices.Length; i++)
            {
                IntPoint point = ToIntPoint(vertices[i]);
                path.Add(point);
            }
            return path;
        }

        public static IntPoint ToIntPoint(Vector2 v)
        {
            return new IntPoint(v.X * SCALE_FACTOR, v.Y * SCALE_FACTOR);
        }

        public static IntPoint ToIntPoint(Vector3 v)
        {
            return new IntPoint(v.X * SCALE_FACTOR, v.Y * SCALE_FACTOR, v.Z);
        }

        public static Vector2 ToVector2(IntPoint point)
        {
            return new Vector2((float)(point.X / SCALE_FACTOR), (float)(point.Y / SCALE_FACTOR));
        }

        public static Vector3 ToVector3(IntPoint point)
        {
            return new Vector3((float)(point.X / SCALE_FACTOR), (float)(point.Y / SCALE_FACTOR), (float)point.Z);
        }
    }
}
