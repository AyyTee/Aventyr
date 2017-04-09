using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class SizeEx
    {
        /// <summary>
        /// Returns the ratio of width to height.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static double WidthRatio(this Size size)
        {
            return size.Width / (double)size.Height;
        }
    }
}
