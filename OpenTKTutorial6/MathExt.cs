using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    struct IntersectPoint
    {
        public bool Exists;
        public Vector2d Vector;
    }
    static class MathExt
    {
        static public Vector2d Matrix2dMult(Vector2d V, Matrix2d M)
        {
            return new Vector2d(V.X * M.M11 + V.Y * M.M21, V.X * M.M12 + V.Y * M.M22);//new Vector2d(M.Column0 * V, M.Column1 * V);
        }
        static public Vector2 Matrix2Mult(Vector2 V, Matrix2 M)
        {
            return new Vector2(V.X * M.M11 + V.Y * M.M21, V.X * M.M12 + V.Y * M.M22);
        }
        static public double AngleLine(Vector2d V0, Vector2d V1)
        {
            return (Math.Atan2(V0.X - V1.X, V0.Y - V1.Y) + 2 * Math.PI) % (2 * Math.PI);
        }
        static public double AngleLine(Vector2 V0, Vector2 V1)
        {
            return (Math.Atan2(V0.X - V1.X, V0.Y - V1.Y) + 2 * Math.PI) % (2 * Math.PI);
        }
        static public double AngleVector(Vector2d V0)
        {
            return (Math.Atan2(V0.X, V0.Y) + 2 * Math.PI) % (2 * Math.PI);
        }
        static public double AngleDiff(double Angle0, double Angle1)
        {
            return ((((Angle0 - Angle1) % Math.PI*2) + Math.PI*3) % Math.PI*2) - Math.PI;
        }
        static public double Lerp(double Value0, double Value1, double T)
        {
            return Value0 * (1 - T) + Value1 * T;
        }
        static public Vector2d Lerp(Vector2d Vector0, Vector2d Vector1, double T)
        {
            return Vector0 * (1 - T) + Vector1 * T;
        }
        static public double ValueWrap(double Value, double mod)
        {
            Value = Value % mod;
            if (Value < 0)
            {
                return mod + Value;
            }
            return Value;
        }
        static public double LerpAngle(double Angle0, double Angle1, double T, bool IsClockwise)
        {
            if (IsClockwise == true)
            {
                if (Angle0 <= Angle1)
                {
                    Angle0 += 2 * Math.PI;
                }
                return Lerp(Angle0, Angle1, T) % (2 * Math.PI);
            }
            else
            {
                if (Angle0 > Angle1)
                {
                    Angle1 += 2 * Math.PI;
                }
                
                return Lerp(Angle0, Angle1, T) % (2 * Math.PI);
            }
        }
        /*static public Vector2d[] LineCircleIntersection(Vector2d ps0, Vector2d pe0, Vector2d Origin, double Radius)
        {
            //private int FindLineCircleIntersections(float cx, float cy, float radius, PointF point1, PointF point2, out PointF intersection1, out PointF intersection2)
            float dx, dy, A, B, C, det, t;

            dx = point2.X - point1.X;
            dy = point2.Y - point1.Y;

            A = dx * dx + dy * dy;
            B = 2 * (dx * (point1.X - cx) + dy * (point1.Y - cy));
            C = (point1.X - cx) * (point1.X - cx) + (point1.Y - cy) * (point1.Y - cy) - Math.Pow(radius, 2);

            det = B * B - 4 * A * C;
            if ((A <= 0.0000001) || (det < 0))
            {
                // No real solutions.
                intersection1 = new PointF(float.NaN, float.NaN);
                intersection2 = new PointF(float.NaN, float.NaN);
                return 0;
            }
            else if (det == 0)
            {
                // One solution.
                t = -B / (2 * A);
                intersection1 = new PointF(point1.X + t * dx, point1.Y + t * dy);
                intersection2 = new PointF(float.NaN, float.NaN);
                return 1;
            }
            else
            {
                // Two solutions.
                t = (float)((-B + Math.Sqrt(det)) / (2 * A));
                intersection1 = new PointF(point1.X + t * dx, point1.Y + t * dy);
                t = (float)((-B - Math.Sqrt(det)) / (2 * A));
                intersection2 = new PointF(point1.X + t * dx, point1.Y + t * dy);
                return 2;
            }
        }*/
        static public double PointLineDistance(Vector2d ps0, Vector2d pe0, Vector2d Point, bool IsSegment)
        {
            {
                Vector2d V;
                Vector2d VDelta = pe0 - ps0;
                if ((VDelta.X == 0) && (VDelta.Y == 0))
                {
                    V = ps0;
                }
                else
                {
                    double t = ((Point.X - ps0.X) * VDelta.X + (Point.Y - ps0.Y) * VDelta.Y) / (Math.Pow(VDelta.X, 2) + Math.Pow(VDelta.Y, 2));
                    if (IsSegment) {t = Math.Min(Math.Max(0, t), 1);}
                    V = ps0 + Vector2d.Multiply(VDelta, t);
                }
                return (Point - V).Length;
            }
        }
        /// <summary>
        /// Returns a projection of V0 onto V1
        /// </summary>
        /// <param name="V0"></param>
        /// <param name="V1"></param>
        static public Vector2d VectorProject(Vector2d V0, Vector2d V1)
        {
            return V1.Normalized() * V0;
        }
        /// <summary>
        /// Mirrors V0 across an axis defined by V1
        /// </summary>
        /// <param name="V0"></param>
        /// <param name="V1"></param>
        /// <returns></returns>
        static public Vector2d VectorMirror(Vector2d V0, Vector2d V1)
        {
            return -V0 + 2 * (V0 - VectorProject(V0, V1));
        }
        static public IntersectPoint LineIntersection(Vector2d ps0, Vector2d pe0, Vector2d ps1, Vector2d pe1, bool SegmentOnly)
        {
            IntersectPoint V = new IntersectPoint();
            double ua;
            double ud = (pe1.Y - ps1.Y) * (pe0.X - ps0.X) - (pe1.X - ps1.X) * (pe0.Y - ps0.Y);
            if (ud != 0)
            {
                ua = ((pe1.X - ps1.X) * (ps0.Y - ps1.Y) - (pe1.Y - ps1.Y) * (ps0.X - ps1.X)) / ud;
                if (SegmentOnly)
                {
                    double ub = ((pe0.X - ps0.X) * (ps0.Y - ps1.Y) - (pe0.Y - ps0.Y) * (ps0.X - ps1.X)) / ud;
                    if (ua < 0 || ua > 1 || ub < 0 || ub > 1)
                    {
                        V.Exists = false;
                        return V;
                    }
                }
            }
            else
            {
                V.Exists = false;
                return V;
            }
            V.Exists = true;
            V.Vector = Lerp(ps0, pe0, ua);
            return V;
        }
        static public bool PointInBox(Vector2d V0, Vector2d V1, Vector2d Point)
        {
            if (((Point.X >= V0.X && Point.X <= V1.X) || (Point.X <= V0.X && Point.X >= V1.X)) && ((Point.Y >= V0.Y && Point.Y <= V1.Y) || (Point.Y <= V0.Y && Point.Y >= V1.Y)))
            {
                return true;
            }
            return false;
        }
        static public bool PointAboveLine(Vector2d LineStart, Vector2d LineEnd, Vector2d Point)
        {
            double m, b, ix;
            if (LineStart.X == LineEnd.X)
            {
                return LineStart.X < Point.X;
            }
            m = (LineStart.Y - LineEnd.Y) / (LineStart.X - LineEnd.X);
            b = LineStart.Y - m * LineStart.X;
            ix = (Point.Y - b) / m;
            return ix < Point.X;
        }
    }
}
