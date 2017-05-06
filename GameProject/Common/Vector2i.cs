using OpenTK;
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

        /// <summary>
        /// Returns the ratio of X to Y.
        /// </summary>
        /// <param name="Vector2i"></param>
        /// <returns></returns>
        public double XRatio => X / (double)Y;

        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vector2i Add(Vector2i Vector2i) => new Vector2i(X + Vector2i.X, Y + Vector2i.Y);
        public Vector2i Subtract(Vector2i Vector2i) => new Vector2i(X - Vector2i.X, Y - Vector2i.Y);
        public Vector2i Multiply(Vector2i Vector2i) => new Vector2i(X * Vector2i.X, Y * Vector2i.Y);
        public Vector2i Multiply(int value) => new Vector2i(X * value, Y * value);
        public Vector2i Divide(int value) => new Vector2i(X / value, Y / value);
        public Vector2i Divide(Vector2i Vector2i) => new Vector2i(X / Vector2i.X, Y / Vector2i.Y);
        public Vector2i Negate() => new Vector2i(-X, -Y);

        public static Vector2i operator +(Vector2i p0, Vector2i p1) => p0.Add(p1);
        public static Vector2i operator -(Vector2i p0, Vector2i p1) => p0.Subtract(p1);
        public static Vector2i operator -(Vector2i p0) => p0.Negate();
        public static Vector2i operator *(Vector2i p0, Vector2i p1) => p0.Multiply(p1);
        public static Vector2i operator *(Vector2i p0, int p1) => p0.Multiply(p1);
        public static Vector2i operator *(int p0, Vector2i p1) => p1.Multiply(p0);
        public static Vector2i operator /(Vector2i p0, int p1) => p0.Divide(p1);
        public static Vector2i operator /(Vector2i p0, Vector2i p1) => p0.Divide(p1);
        public static bool operator ==(Vector2i p0, Vector2i p1) => p0.Equals(p1);
        public static bool operator !=(Vector2i p0, Vector2i p1) => !p0.Equals(p1);
        public static explicit operator Vector2i(System.Drawing.Size size) => new Vector2i(size.Width, size.Height);
        public static explicit operator Vector2(Vector2i Vector2i) => new Vector2(Vector2i.X, Vector2i.Y);

        public override bool Equals(object obj)
        {
            if (obj is Vector2i v)
            {
                return X == v.X && Y == v.Y;
            }
            return false;
        }

        public override int GetHashCode() => base.GetHashCode() ^ X ^ Y;
    }
}
