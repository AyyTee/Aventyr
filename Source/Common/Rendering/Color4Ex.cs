using Game.Common;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    public static class Color4Ex
    {
        public static Color4 Lerp(this Color4 start, Color4 end, float t)
        {
            return ((end.ToVector() - start.ToVector()) * t + start.ToVector()).ToColor();
        }

        /// <summary>
        /// Returns a vector with its XYZW components mapped to color's RGBA components.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Vector4 ToVector(this Color4 color) => new Vector4(color.R, color.G, color.B, color.A);

        public static Color4 AddRgb(this Color4 color, float r, float g, float b) => new Color4(color.R + r, color.G + g, color.B + b, color.A);
    }
}
