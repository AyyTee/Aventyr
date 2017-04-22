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

        public Size Add(Size size) => new Size(Width + size.Width, Height + size.Height);
        public Size Subtract(Size size) => new Size(Width - size.Width, Height - size.Height);
        public Size Multiply(Size size) => new Size(Width * size.Width, Height * size.Height);
        public Size Multiply(int value) => new Size(Width * value, Height * value);
        public Size Divide(int value) => new Size(Width / value, Height / value);
        public Size Divide(Size size) => new Size(Width / size.Width, Height / size.Height);
        public Size Negate() => new Size(-Width, -Height);

        public static Size operator +(Size p0, Size p1) => p0.Add(p1);
        public static Size operator -(Size p0, Size p1) => p0.Subtract(p1);
        public static Size operator -(Size p0) => p0.Negate();
        public static Size operator *(Size p0, Size p1) => p0.Multiply(p1);
        public static Size operator *(Size p0, int p1) => p0.Multiply(p1);
        public static Size operator *(int p0, Size p1) => p1.Multiply(p0);
        public static Size operator /(Size p0, int p1) => p0.Divide(p1);
        public static Size operator /(Size p0, Size p1) => p0.Divide(p1);
        public static explicit operator Size(Vector2i point) => new Size(point.X, point.Y);
        public static explicit operator Vector2i(Size size) => new Vector2i(size.Width, size.Height);
        public static explicit operator Size(System.Drawing.Size size) => new Size(size.Width, size.Height);
        public static explicit operator Vector2(Size size) => new Vector2(size.Width, size.Height);
    }
}
