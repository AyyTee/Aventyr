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

        public bool IsInsideFOV(Vector2 viewPoint, Vector2 lookPoint)
        {
            //check if the lookPoint is on the opposite side of the line from the viewPoint
            if (PointIsLeft(viewPoint) == PointIsLeft(lookPoint))
            {
                return false;
            }
            //check if the lookPoint is within the FOV angles
            double Angle0 = MathExt.AngleVector(Vertices[0] - viewPoint);
            double Angle1 = MathExt.AngleVector(Vertices[1] - viewPoint);
            double AngleDiff = MathExt.AngleDiff(Angle0, Angle1);
            double AngleLook = MathExt.AngleVector(lookPoint - viewPoint);
            double AngleLookDiff = MathExt.AngleDiff(Angle0, AngleLook);
            if (Math.Abs(AngleDiff) >= Math.Abs(AngleLookDiff) && Math.Sign(AngleDiff) == Math.Sign(AngleLookDiff))
            {
                return true;
            }
            return false;
        }

        public bool IsInsideFOV(Vector2 viewPoint, Line lookLine)
        {
            //check if the lookPoint is on the opposite side of the line from the viewPoint
            if (PointIsLeft(viewPoint) == PointIsLeft(lookLine.Vertices[0]) && PointIsLeft(viewPoint) == PointIsLeft(lookLine.Vertices[1]))
            {
                return false;
            }
            //check if the lookPoint is within the FOV angles
            double Angle0 = MathExt.AngleVector(Vertices[0] - viewPoint);
            double Angle1 = MathExt.AngleVector(Vertices[1] - viewPoint);
            double AngleDiff = MathExt.AngleDiff(Angle0, Angle1);
            double AngleLook = MathExt.AngleVector(lookLine.Vertices[0] - viewPoint);
            double AngleLookDiff = MathExt.AngleDiff(Angle0, AngleLook);
            double AngleLook2 = MathExt.AngleVector(lookLine.Vertices[1] - viewPoint);
            double AngleLookDiff2 = MathExt.AngleDiff(Angle0, AngleLook2);
            //check if the first point is in the FOV
            if (Math.Abs(AngleDiff) >= Math.Abs(AngleLookDiff) && Math.Sign(AngleDiff) == Math.Sign(AngleLookDiff))
            {
                return true;
            }
            //check if the second point is in the FOV
            if (Math.Abs(AngleDiff) >= Math.Abs(AngleLookDiff2) && Math.Sign(AngleDiff) == Math.Sign(AngleLookDiff2))
            {
                return true;
            }
            //check if the two points are on opposite sides of the FOV
            if (Math.Sign(AngleLookDiff) != Math.Sign(AngleLookDiff2))
            {
                return true;
            }
            return false;
        }

        public IntersectPoint Intersects(Line line, bool segmentOnly)
        {
            return MathExt.LineIntersection(Vertices[0], Vertices[1], line.Vertices[0], line.Vertices[1], segmentOnly);
        }

        public IntersectPoint Intersects(Vector2[] polygon)
        {
            for (int i0 = 0; i0 < polygon.Length; i0++)
            {
                int i1 = (i0 + 1) % polygon.Length;
                return MathExt.LineIntersection(Vertices[0], Vertices[1], polygon[i0], polygon[i1], true);
            }
            IntersectPoint point = new IntersectPoint();
            point.Exists = false;
            return point;
        }
    }
}
