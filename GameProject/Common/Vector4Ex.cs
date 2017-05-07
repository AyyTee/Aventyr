using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Common
{
    public static class Vector4Ex
    {
        /// <summary>
        /// Returns a color with its RGBA components mapped to vectors's XYZW components.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color4 ToColor(this Vector4 vector) => new Color4(vector.X, vector.Y, vector.Z, vector.W);
    }
}
