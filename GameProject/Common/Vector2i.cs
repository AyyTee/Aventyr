using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Common
{
    public struct Vector2i
    {
        public int X, Y;

        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vector2i Add(Vector2i point)
        {
            return new Vector2i(X + point.X, Y + point.Y);
        }

        public Vector2i Subtract(Vector2i point)
        {
            return new Vector2i(X - point.X, Y - point.Y);
        }

        public Vector2i Multiply(Vector2i point)
        {
            return new Vector2i(X * point.X, Y * point.Y);
        }

        public static Vector2i operator +(Vector2i p0, Vector2i p1)
        {
            return p0.Add(p1);
        }

        public static Vector2i operator -(Vector2i p0, Vector2i p1)
        {
            return p0.Subtract(p1);
        }

        public static Vector2i operator *(Vector2i p0, Vector2i p1)
        {
            return p0.Multiply(p1);
        }
    }
}
