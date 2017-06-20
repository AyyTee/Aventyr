using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Game.Serialization;
using OpenTK;
using MathHelper = OpenTK.MathHelper;
using Vector2 = OpenTK.Vector2;
using Xna = Microsoft.Xna.Framework;

namespace Game.Common
{
    [DebuggerDisplay("LineF {this[0]}, {this[1]}")]
    public class LineF : IShallowClone<LineF>
    {
        public float Length => Delta.Length;
        readonly Vector2[] _vertices = new Vector2[2];
        public Vector2 Delta => this[1] - this[0];
        public Vector2 Center => (this[1] + this[0]) / 2;

        public Vector2 this[int index]
        {
            get { return _vertices[index]; }
            set
            {
                Debug.Assert(Vector2Ex.IsReal(value));
                _vertices[index] = value;
            }
        }

        #region Constructors
        public LineF()
        {
        }

        public LineF(Vector2 lineStart, Vector2 lineEnd)
        {
            this[0] = lineStart;
            this[1] = lineEnd;
        }

        public LineF(Vector2d lineStart, Vector2d lineEnd)
        {
            this[0] = new Vector2((float)lineStart.X, (float)lineStart.Y);
            this[1] = new Vector2((float)lineEnd.X, (float)lineEnd.Y);
        }

        public LineF(Xna.Vector2 lineStart, Xna.Vector2 lineEnd)
        {
            this[0] = (Vector2)lineStart;
            this[1] = (Vector2)lineEnd;
        }

        public LineF(IList<Vector2> line)
        {
            Debug.Assert(line.Count == 2);
            _vertices = line.ToArray();
        }

        public LineF(Vector2 start, float direction, float length)
            : this(start, start + (Vector2)MathEx.AngleToVectorReversed(direction) * length)
        {
        }
        #endregion

        /// <summary>
        /// Returns whether a point is left or right of this line.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="ignoreEdgeCase">Whether or not to treat points exactly on the line as to the right of it instead.</param>
        public Side GetSideOf(Vector2 point, bool ignoreEdgeCase = true)
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
        /// Returns whether a point is left or right of the line.
        /// </summary>
        public Side GetSideOf(Xna.Vector2 point)
        {
            return GetSideOf((Vector2)point);
        }

        /// <summary>
        /// Returns whether a line is left, right, or inbetween the line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public Side GetSideOf(LineF line)
        {
            Side side0 = GetSideOf(line[0]);
            Side side1 = GetSideOf(line[1]);
            if (side0 == side1)
            {
                return side0;
            }
            return Side.Neither;
        }

        public Vector2 GetNormal()
        {
            return (_vertices[1] - _vertices[0]).PerpendicularRight.Normalized();
        }

        /// <summary>
        /// Check if a Vector2 is inside the Fov of this line.
        /// </summary>
        public bool IsInsideFov(Vector2 viewPoint, Vector2 v)
        {
            //Check if the lookPoint is on the opposite side of the line from the viewPoint.
            if (GetSideOf(viewPoint) == GetSideOf(v))
            {
                return false;
            }
            //Check if the lookPoint is within the Fov angles.
            double angle0 = MathEx.VectorToAngleReversed(_vertices[0] - viewPoint);
            double angle1 = MathEx.VectorToAngleReversed(_vertices[1] - viewPoint);
            double angleDiff = MathEx.AngleDiff(angle0, angle1);
            double angleLook = MathEx.VectorToAngleReversed(v - viewPoint);
            double angleLookDiff = MathEx.AngleDiff(angle0, angleLook);
            if (Math.Abs(angleDiff) >= Math.Abs(angleLookDiff) && Math.Sign(angleDiff) == Math.Sign(angleLookDiff))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if a line is at least partially inside the Fov of this line.
        /// </summary>
        public bool IsInsideFov(Vector2 viewPoint, LineF line)
        {
            // Check if there is an intersection between the two lines.
            if (MathEx.LineLineIntersect(this, line, true) != null)
            {
                return true;
            }
            // Check if there is an intersection between the first Fov line and line.
            IntersectCoord intersect0 = MathEx.LineLineIntersect(new LineF(_vertices[0], 2 * _vertices[0] - viewPoint), line, false);
            if (intersect0 != null && intersect0.First >= 0 && intersect0.Last >= 0 && intersect0.Last < 1)
            {
                return true;
            }
            // Check if there is an intersection between the second Fov line and line.
            IntersectCoord intersect1 = MathEx.LineLineIntersect(new LineF(_vertices[1], 2 * _vertices[1] - viewPoint), line, false);
            if (intersect1 != null && intersect1.First >= 0 && intersect1.Last >= 0 && intersect1.Last < 1)
            {
                return true;
            }

            // Check if the lookPoint is within the Fov angles.
            double angle0 = MathEx.VectorToAngleReversed(_vertices[0] - viewPoint);
            double angle1 = MathEx.VectorToAngleReversed(_vertices[1] - viewPoint);
            double angleDiff = MathEx.AngleDiff(angle0, angle1);
            double angleLook = MathEx.VectorToAngleReversed(line[0] - viewPoint);
            double angleLookDiff = MathEx.AngleDiff(angle0, angleLook);
            double angleLook2 = MathEx.VectorToAngleReversed(line[1] - viewPoint);
            double angleLookDiff2 = MathEx.AngleDiff(angle0, angleLook2);
            // Check if the first point is in the Fov
            if (Math.Abs(angleDiff) >= Math.Abs(angleLookDiff) && Math.Sign(angleDiff) == Math.Sign(angleLookDiff))
            {
                if (GetSideOf(viewPoint) != GetSideOf(line[0]))
                {
                    return true;
                }
            }
            // Check if the second point is in the Fov
            if (Math.Abs(angleDiff) >= Math.Abs(angleLookDiff2) && Math.Sign(angleDiff) == Math.Sign(angleLookDiff2))
            {
                if (GetSideOf(viewPoint) != GetSideOf(line[1]))
                {
                    return true;
                }
            }
            return false;
        }

        public LineF Translate(Vector2 offset)
        {
            return new LineF(this[0] + offset, this[1] + offset);
        }

        public double GetOffset()
        {
            return MathEx.PointLineDistance(new Vector2(), this, false);
        }

        /// <summary>
        /// Swaps the start and end vertice for the line.
        /// </summary>
        public LineF Reverse()
        {
            return new LineF(this[1], this[0]);
        }

        public LineF Transform(Matrix4 transformMatrix)
        {
            return new LineF(Vector2Ex.Transform(_vertices, transformMatrix));
        }

        public Vector2 Lerp(float t)
        {
            return _vertices[0].Lerp(_vertices[1], t);
        }

        /// <summary>
        /// Returns the T value of the nearest point on this line to a vector.
        /// </summary>
        /// <returns></returns>
        public float NearestT(Vector2 v, bool isSegment)
        {
            Vector2 vDelta = _vertices[1] - _vertices[0];
            double t = ((v.X - _vertices[0].X) * vDelta.X + (v.Y - _vertices[0].Y) * vDelta.Y) / (Math.Pow(vDelta.X, 2) + Math.Pow(vDelta.Y, 2));
            if (isSegment)
            {
                t = MathHelper.Clamp(t, 0, 1);
            }
            return (float)t;
            //return Vector2.Dot(v - Vertices[0], Vertices[1] - Vertices[0]);
        }

        public float NearestT(Xna.Vector2 v, bool isSegment)
        {
            return NearestT((Vector2)v, isSegment);
        }

        public Vector2 Nearest(Vector2 v, bool isSegment)
        {
            float t = NearestT(v, isSegment);
            return _vertices[0] + (_vertices[1] - _vertices[0]) * t;
        }

        public Vector2 Nearest(Xna.Vector2 v, bool isSegment)
        {
            float t = NearestT(v, isSegment);
            return _vertices[0] + (_vertices[1] - _vertices[0]) * t;
        }

        public float Angle()
        {
            return (float)MathEx.LineToAngle(_vertices[0], _vertices[1]);
        }

        public LineF ShallowClone()
        {
            return new LineF(_vertices);
        }

        public LineF GetPerpendicularLeft(bool normalize = true)
        {
            var p = new LineF(_vertices[0], (_vertices[1] - _vertices[0]).PerpendicularLeft + _vertices[0]);
            if (normalize)
            {
                p.Normalize();
            }
            return p;
        }

        public LineF GetPerpendicularRight(bool normalize = true)
        {
            var p = new LineF(_vertices[0], (_vertices[1] - _vertices[0]).PerpendicularRight + _vertices[0]);
            if (normalize)
            {
                p.Normalize();
            }
            return p;
        }

        /// <summary>Normalize this Line by moving its end point.</summary>
        public void Normalize()
        {
            Vector2 normal = (_vertices[1] - _vertices[0]).Normalized();
            Debug.Assert(Vector2Ex.IsReal(normal), "Unable to normalize 0 length vector.");
            this[1] = normal + _vertices[0];
        }
    }
}
