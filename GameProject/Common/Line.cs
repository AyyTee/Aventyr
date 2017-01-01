using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Diagnostics;
using Xna = Microsoft.Xna.Framework;

namespace Game
{
    [DebuggerDisplay("Line {this[0]}, {this[1]}")]
    public class Line : IShallowClone<Line>
    {
        public double Length { get { return Delta.Length; } }
        Vector2d[] _vertices = new Vector2d[2];
        public Vector2d Delta { get { return this[1] - this[0]; } }
        public Vector2d Center { get { return (this[1] + this[0]) / 2; } }
        public Vector2d this[int index]
        {
            get { return _vertices[index]; }
            set
            {
                Debug.Assert(Vector2Ext.IsReal(value));
                _vertices[index] = value;
            }
        }

        #region Constructors
        public Line()
        {
        }

        public Line(Vector2d lineStart, Vector2d lineEnd)
        {
            this[0] = lineStart;
            this[1] = lineEnd;
        }

        public Line(IList<Vector2d> line)
        {
            Debug.Assert(line.Count == 2);
            _vertices = line.ToArray();
        }

        public Line(Vector2d center, double rotation, double length)
        {
            Vector2d offset = new Vector2d((double)Math.Cos(rotation), (double)Math.Sin(rotation)) * length;
            this[0] = center + offset;
            this[1] = center - offset;
        }
        #endregion

        /// <summary>
        /// Returns whether a point is left or right of this line.
        /// </summary>
        /// <param name="ignoreEdgeCase">Whether or not to treat points exactly on the line as to the right of it instead.</param>
        public Side GetSideOf(Vector2d point, bool ignoreEdgeCase = true)
        {
            double p = (this[1].X - this[0].X) * (point.Y - this[0].Y) - (this[1].Y - this[0].Y) * (point.X - this[0].X);
            if (p > 0)
            {
                return Side.Left;
            }
            else if (p == 0 && !ignoreEdgeCase)
            {
                return Side.Neither;
            }
            return Side.Right;
        }

        /// <summary>
        /// Returns whether a line is left, right, or inbetween the line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public Side GetSideOf(Line line)
        {
            Side side0 = GetSideOf(line[0]);
            Side side1 = GetSideOf(line[1]);
            if (side0 == side1)
            {
                return side0;
            }
            return Side.Neither;
        }

        public Vector2d GetNormal()
        {
            return (_vertices[1] - _vertices[0]).PerpendicularRight.Normalized();
        }

        /// <summary>
        /// Check if a Vector2d is inside the Fov of this line.
        /// </summary>
        public bool IsInsideFOV(Vector2d viewPoint, Vector2d v)
        {
            //Check if the lookPoint is on the opposite side of the line from the viewPoint.
            if (GetSideOf(viewPoint) == GetSideOf(v))
            {
                return false;
            }
            //Check if the lookPoint is within the Fov angles.
            double Angle0 = MathExt.AngleVector(_vertices[0] - viewPoint);
            double Angle1 = MathExt.AngleVector(_vertices[1] - viewPoint);
            double AngleDiff = MathExt.AngleDiff(Angle0, Angle1);
            double AngleLook = MathExt.AngleVector(v - viewPoint);
            double AngleLookDiff = MathExt.AngleDiff(Angle0, AngleLook);
            if (Math.Abs(AngleDiff) >= Math.Abs(AngleLookDiff) && Math.Sign(AngleDiff) == Math.Sign(AngleLookDiff))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if a line is at least partially inside the Fov of this line.
        /// </summary>
        public bool IsInsideFOV(Vector2d viewPoint, Line line)
        {
            //Check if there is an intersection between the two lines.
            if (MathExt.LineLineIntersect(this, line, true).Exists)
            {
                return true;
            }
            //Check if there is an intersection between the first Fov line and line.
            IntersectCoord intersect0 = MathExt.LineLineIntersect(new Line(_vertices[0], 2 * _vertices[0] - viewPoint), line, false);
            if (intersect0.TFirst >= 0 && intersect0.TLast >= 0 && intersect0.TLast < 1)
            {
                return true;
            }
            //Check if there is an intersection between the second Fov line and line.
            IntersectCoord intersect1 = MathExt.LineLineIntersect(new Line(_vertices[1], 2 * _vertices[1] - viewPoint), line, false);
            if (intersect1.TFirst >= 0 && intersect1.TLast >= 0 && intersect1.TLast < 1)
            {
                return true;
            }

            //Check if the lookPoint is within the Fov angles.
            double Angle0 = MathExt.AngleVector(_vertices[0] - viewPoint);
            double Angle1 = MathExt.AngleVector(_vertices[1] - viewPoint);
            double AngleDiff = MathExt.AngleDiff(Angle0, Angle1);
            double AngleLook = MathExt.AngleVector(line[0] - viewPoint);
            double AngleLookDiff = MathExt.AngleDiff(Angle0, AngleLook);
            double AngleLook2 = MathExt.AngleVector(line[1] - viewPoint);
            double AngleLookDiff2 = MathExt.AngleDiff(Angle0, AngleLook2);
            //check if the first point is in the Fov
            if (Math.Abs(AngleDiff) >= Math.Abs(AngleLookDiff) && Math.Sign(AngleDiff) == Math.Sign(AngleLookDiff))
            {
                if (GetSideOf(viewPoint) != GetSideOf(line[0]))
                {
                    return true;
                }
            }
            //check if the second point is in the Fov
            if (Math.Abs(AngleDiff) >= Math.Abs(AngleLookDiff2) && Math.Sign(AngleDiff) == Math.Sign(AngleLookDiff2))
            {
                if (GetSideOf(viewPoint) != GetSideOf(line[1]))
                {
                    return true;
                }
            }
            return false;
        }

        public Line Translate(Vector2d offset)
        {
            return new Line(this[0] + offset, this[1] + offset);
        }

        public double GetOffset()
        {
            return MathExt.PointLineDistance(new Vector2d(), this, false);
        }

        /// <summary>
        /// Swaps the start and end vertice for the line.
        /// </summary>
        public Line Reverse()
        {
            return new Line(this[1], this[0]);
        }

        public Line Transform(Matrix4d transformMatrix)
        {
            return new Line(Vector2Ext.Transform(_vertices, transformMatrix));
        }

        public Vector2d Lerp(double t)
        {
            return MathExt.Lerp(_vertices[0], _vertices[1], t);
        }

        /// <summary>
        /// Returns the T value of the nearest point on this line to a vector.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public double NearestT(Vector2d v, bool isSegment)
        {
            Vector2d VDelta = _vertices[1] - _vertices[0];
            double t = ((v.X - _vertices[0].X) * VDelta.X + (v.Y - _vertices[0].Y) * VDelta.Y) / (Math.Pow(VDelta.X, 2) + Math.Pow(VDelta.Y, 2));
            if (isSegment)
            {
                t = MathHelper.Clamp(t, 0, 1);
            }
            return (double)t;        
            //return Vector2d.Dot(v - Vertices[0], Vertices[1] - Vertices[0]);
        }

        public Vector2d Nearest(Vector2d v, bool isSegment)
        {
            double t = NearestT(v, isSegment);
            return _vertices[0] + (_vertices[1] - _vertices[0]) * t;
        }

        public double Angle()
        {
            return MathExt.AngleLine(_vertices[0], _vertices[1]);
        }

        public Line ShallowClone()
        {
            return new Line(_vertices);
        }

        public Line GetPerpendicularLeft(bool normalize = true)
        {
            Line p = new Line(_vertices[0], (_vertices[1] - _vertices[0]).PerpendicularLeft + _vertices[0]);
            if (normalize)
            {
                p.Normalize();
            }
            return p;
        }

        public Line GetPerpendicularRight(bool normalize = true)
        {
            Line p = new Line(_vertices[0], (_vertices[1] - _vertices[0]).PerpendicularRight + _vertices[0]);
            if (normalize)
            {
                p.Normalize();
            }
            return p;
        }

        /// <summary>Normalize this Line by moving its end point.</summary>
        public void Normalize()
        {
            Vector2d normal = (_vertices[1] - _vertices[0]).Normalized();
            Debug.Assert(Vector2Ext.IsReal(normal), "Unable to normalize 0 length vector.");
            this[1] = normal + _vertices[0];
        }

        public static implicit operator Line(LineF line)
        {
            return new Line((Vector2d)line[0], (Vector2d)line[1]);
        }

        public static explicit operator LineF(Line line)
        {
            return new LineF(line[0], line[1]);
        }
    }
}
