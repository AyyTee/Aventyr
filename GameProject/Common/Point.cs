using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Common
{
    public struct Point
    {
        public int X, Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Point Add(Point point)
        {
            return new Point(X + point.X, Y + point.Y);
        }

        public Point Subtract(Point point)
        {
            return new Point(X - point.X, Y - point.Y);
        }

        public Point Multiply(Point point)
        {
            return new Point(X * point.X, Y * point.Y);
        }

        public static Point operator +(Point p0, Point p1)
        {
            return p0.Add(p1);
        }

        public static Point operator -(Point p0, Point p1)
        {
            return p0.Subtract(p1);
        }

        public static Point operator *(Point p0, Point p1)
        {
            return p0.Multiply(p1);
        }
    }
}
