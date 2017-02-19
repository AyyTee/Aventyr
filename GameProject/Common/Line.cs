using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Game.Serialization;
using OpenTK;
using MathHelper = OpenTK.MathHelper;

namespace Game.Common
{
    [DebuggerDisplay("Line {this[0]}, {this[1]}")]
    public class Line : IShallowClone<Line>
    {
        public double Length => Delta.Length;
        readonly Vector2d[] _vertices = new Vector2d[2];
        public Vector2d Delta => this[1] - this[0];
        public Vector2d Center => (this[1] + this[0]) / 2;

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

        public Line(ICollection<Vector2d> line)
        {
            Debug.Assert(line.Count == 2);
            _vertices = line.ToArray();
        }

        public Line(Vector2d start, double direction, double length)
            : this(start, start + new Vector2d(Math.Cos(direction), Math.Sin(direction)) * length)
        {
        }
        #endregion

        /// <summary>
        /// Returns whether a point is left or right of this line.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="ignoreEdgeCase">Whether or not to treat points exactly on the line as to the right of it instead.</param>
        public Side GetSideOf(Vector2d point, bool ignoreEdgeCase = true)
        {
            double p = (this[1].X - this[0].X) * (point.Y - this[0].Y) - (this[1].Y - this[0].Y) * (point.X - this[0].X);
            if (p > 0)
            {
                return Side.Left;
            }
            if (p == 0 && !ignoreEdgeCase)
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
        public bool IsInsideFov(Vector2d viewPoint, Vector2d v)
        {
            //Check if the lookPoint is on the opposite side of the line from the viewPoint.
            if (GetSideOf(viewPoint) == GetSideOf(v))
            {
                return false;
            }
            //Check if the lookPoint is within the Fov angles.
            double angle0 = MathExt.AngleVector(_vertices[0] - viewPoint);
            double angle1 = MathExt.AngleVector(_vertices[1] - viewPoint);
            double angleDiff = MathExt.AngleDiff(angle0, angle1);
            double angleLook = MathExt.AngleVector(v - viewPoint);
            double angleLookDiff = MathExt.AngleDiff(angle0, angleLook);
            return Math.Abs(angleDiff) >= Math.Abs(angleLookDiff) && Math.Sign(angleDiff) == Math.Sign(angleLookDiff);
        }

        /// <summary>
        /// Check if a line is at least partially inside the Fov of this line.
        /// </summary>
        public bool IsInsideFov(Vector2d viewPoint, Line line)
        {
            //Check if there is an intersection between the two lines.
            if (MathExt.LineLineIntersect(this, line, true) != null)
            {
                return true;
            }
            //Check if there is an intersection between the first Fov line and line.
            IntersectCoord intersect0 = MathExt.LineLineIntersect(new Line(_vertices[0], 2 * _vertices[0] - viewPoint), line, false);
            if (intersect0.First >= 0 && intersect0.Last >= 0 && intersect0.Last < 1)
            {
                return true;
            }
            //Check if there is an intersection between the second Fov line and line.
            IntersectCoord intersect1 = MathExt.LineLineIntersect(new Line(_vertices[1], 2 * _vertices[1] - viewPoint), line, false);
            if (intersect1.First >= 0 && intersect1.Last >= 0 && intersect1.Last < 1)
            {
                return true;
            }

            //Check if the lookPoint is within the Fov angles.
            double angle0 = MathExt.AngleVector(_vertices[0] - viewPoint);
            double angle1 = MathExt.AngleVector(_vertices[1] - viewPoint);
            double angleDiff = MathExt.AngleDiff(angle0, angle1);
            double angleLook = MathExt.AngleVector(line[0] - viewPoint);
            double angleLookDiff = MathExt.AngleDiff(angle0, angleLook);
            double angleLook2 = MathExt.AngleVector(line[1] - viewPoint);
            double angleLookDiff2 = MathExt.AngleDiff(angle0, angleLook2);
            //check if the first point is in the Fov
            if (Math.Abs(angleDiff) >= Math.Abs(angleLookDiff) && Math.Sign(angleDiff) == Math.Sign(angleLookDiff))
            {
                if (GetSideOf(viewPoint) != GetSideOf(line[0]))
                {
                    return true;
                }
            }
            //check if the second point is in the Fov
            if (Math.Abs(angleDiff) >= Math.Abs(angleLookDiff2) && Math.Sign(angleDiff) == Math.Sign(angleLookDiff2))
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
        /// <param name="v"></param>
        /// <param name="isSegment"></param>
        /// <returns></returns>
        public double NearestT(Vector2d v, bool isSegment)
        {
            Vector2d vDelta = _vertices[1] - _vertices[0];
            double t = ((v.X - _vertices[0].X) * vDelta.X + (v.Y - _vertices[0].Y) * vDelta.Y) / (Math.Pow(vDelta.X, 2) + Math.Pow(vDelta.Y, 2));
            if (isSegment)
            {
                t = MathHelper.Clamp(t, 0, 1);
            }
            return t;
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
            var p = new Line(_vertices[0], (_vertices[1] - _vertices[0]).PerpendicularLeft + _vertices[0]);
            if (normalize)
            {
                p.Normalize();
            }
            return p;
        }

        public Line GetPerpendicularRight(bool normalize = true)
        {
            var p = new Line(_vertices[0], (_vertices[1] - _vertices[0]).PerpendicularRight + _vertices[0]);
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
