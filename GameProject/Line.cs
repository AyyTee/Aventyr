using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Game
{
    public class Line
    {
        public Vector2[] Vertices = new Vector2[2];

        public Line(Vector2 lineStart, Vector2 lineEnd)
        {
            Vertices[0] = lineStart;
            Vertices[1] = lineEnd;
        }

        public Line(Vector2[] line)
        {
            Vertices = line;
        }

        /// <summary>
        /// Returns whether a point is left of the line.
        /// </summary>
        public bool PointIsLeft(Vector2 Point)
        {
            return (Vertices[1].X - Vertices[0].X) * (Point.Y - Vertices[0].Y) - (Vertices[1].Y - Vertices[0].Y) * (Point.X - Vertices[0].X) > 0;
        }
    }
}
