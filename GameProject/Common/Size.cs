using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Common
{
    public struct Size
    {
        public int Width, Height;

        /// <summary>
        /// Returns the ratio of width to height.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public double WidthRatio => Width / (double)Height;

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Size Add(Size Size)
        {
            return new Size(Width + Size.Width, Height + Size.Height);
        }

        public Size Subtract(Size Size)
        {
            return new Size(Width - Size.Width, Height - Size.Height);
        }

        public Size Multiply(Size Size)
        {
            return new Size(Width * Size.Width, Height * Size.Height);
        }

        

        public static Size operator +(Size p0, Size p1)
        {
            return p0.Add(p1);
        }

        public static Size operator -(Size p0, Size p1)
        {
            return p0.Subtract(p1);
        }

        public static Size operator *(Size p0, Size p1)
        {
            return p0.Multiply(p1);
        }

        public static explicit operator Size(Point point)
        {
            return new Size(point.X, point.Y);
        }

        public static explicit operator Point(Size size)
        {
            return new Point(size.Width, size.Height);
        }

        public static explicit operator Size(System.Drawing.Size size)
        {
            return new Size(size.Width, size.Height);
        }

        public static explicit operator Vector2(Size size)
        {
            return new Vector2(size.Width, size.Height);
        }
    }
}
