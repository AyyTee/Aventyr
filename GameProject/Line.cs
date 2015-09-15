using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Diagnostics;

namespace Game
{
    public class Line
    {
        public enum Side
        {
            IsLeftOf,
            IsRightOf,
            IsNeither
        }
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
        /// Returns whether a point is left or right of the line
        /// </summary>
        public Side GetSideOf(Vector2 point)
        {
            if ((Vertices[1].X - Vertices[0].X) * (point.Y - Vertices[0].Y) - (Vertices[1].Y - Vertices[0].Y) * (point.X - Vertices[0].X) > 0)
            {
                return Side.IsLeftOf;
            }
            return Side.IsRightOf;
        }

        /// <summary>
        /// returns whether a line is left, right, or inbetween the line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public Side GetSideOf(Line line)
        {
            Side side0 = GetSideOf(line.Vertices[0]);
            Side side1 = GetSideOf(line.Vertices[1]);
            if (side0 == side1)
            {
                return side0;
            }
            return Side.IsNeither;
        }

        public bool IsInsideFOV(Vector2 viewPoint, Vector2 lookPoint)
        {
            //check if the lookPoint is on the opposite side of the line from the viewPoint
            if (GetSideOf(viewPoint) == GetSideOf(lookPoint))
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
            if (GetSideOf(viewPoint) == GetSideOf(lookLine.Vertices[0]) && GetSideOf(viewPoint) == GetSideOf(lookLine.Vertices[1]))
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

        /// <summary>
        /// Checks if the line intersects the edges of a polygon.  This does not test if the line is contained in the polygon
        /// </summary>
        /// <param name="polygon">A closed polygon</param>
        /// <returns>An intersection point</returns>
        public IntersectPoint Intersects(Vector2[] polygon)
        {
            IntersectPoint point;
            for (int i0 = 0; i0 < polygon.Length; i0++)
            {
                int i1 = (i0 + 1) % polygon.Length;
                point = MathExt.LineIntersection(Vertices[0], Vertices[1], polygon[i0], polygon[i1], true);
                if (point.Exists)
                {
                    return point;
                }
            }
            point = new IntersectPoint();
            point.Exists = false;
            return point;
        }

        /// <summary>
        /// Checks if the line is at least partially contained in a polygon
        /// </summary>
        /// <param name="polygon">A closed polygon</param>
        /// <returns></returns>
        public bool IsInsideOfPolygon(Vector2[] polygon)
        {
            if (MathExt.IsPointInPolygon(polygon, Vertices[0]))
            {
                return true;
            }
            if (MathExt.IsPointInPolygon(polygon, Vertices[1]))
            {
                return true;
            }
            return Intersects(polygon).Exists;
        }

        public float PointDistance(Vector2 point, bool isSegment)
        {
            Vector2 V;
            Vector2 VDelta = Vertices[1] - Vertices[0];
            if ((VDelta.X == 0) && (VDelta.Y == 0))
            {
                V = Vertices[0];
            }
            else
            {
                float t = ((point.X - Vertices[0].X) * VDelta.X + (point.Y - Vertices[0].Y) * VDelta.Y) / (float)(Math.Pow(VDelta.X, 2) + Math.Pow(VDelta.Y, 2));
                Debug.Assert(float.IsNaN(t) == false);
                if (isSegment) { t = (float)Math.Min(Math.Max(0, t), 1); }
                V = Vertices[0] + Vector2.Multiply(VDelta, t);
            }
            float distance = (point - V).Length;
            Debug.Assert(distance >= 0);
            return distance;
        }

        /// <summary>
        /// Swaps the start and end vertice for the line.
        /// </summary>
        public void Reverse()
        {
            Vector2 temp = Vertices[0];
            Vertices[0] = Vertices[1];
            Vertices[1] = temp;
        }

        public void Transform(Matrix4 transformMatrix)
        {
            Vertices = VectorExt2.Transform(Vertices, transformMatrix);
        }
    }
}
