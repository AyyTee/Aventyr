using ClipperLib;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public struct IntersectPoint
    {
        public bool Exists;
        public Vector2d Position;
        public double T;
    }
    public static class MathExt
    {
        static public Vector2d Matrix2dMult(Vector2d V, Matrix2d M)
        {
            return new Vector2d(V.X * M.M11 + V.Y * M.M21, V.X * M.M12 + V.Y * M.M22);
        }
        static public Vector2 Matrix2Mult(Vector2 V, Matrix2 M)
        {
            return new Vector2(V.X * M.M11 + V.Y * M.M21, V.X * M.M12 + V.Y * M.M22);
        }
        static public double AngleLine(Vector2d V0, Vector2d V1)
        {
            return AngleVector(V0 - V1);
        }
        static public double AngleLine(Vector2 V0, Vector2 V1)
        {
            return AngleLine(new Vector2d(V0.X, V0.Y), new Vector2d(V1.X, V1.Y));
        }

        static public double AngleVector(Vector2d V0)
        {
            //Debug.Assert(V0 == Vector2d.Zero, "Vector must have non-zero length.");
            double val = Math.Atan2(V0.X, V0.Y);

            if (Double.IsNaN(val))
            {
                return 0;
            }
            return (val + 2 * Math.PI) % (2 * Math.PI) - Math.PI / 2;
        }

        static public double AngleVector(Vector2 V0)
        {
            return AngleVector(new Vector2d(V0.X, V0.Y));
        }

        static public double AngleDiff(double angle0, double angle1)
        {

            //return Math.PI - Math.Abs(Math.Abs(angle0 - angle1) - Math.PI);
            return ((angle1 - angle0) % (Math.PI * 2) + Math.PI * 3) % (Math.PI * 2) - Math.PI;
        }

        static public double Lerp(double Value0, double Value1, double T)
        {
            return Value0 * (1 - T) + Value1 * T;
        }

        static public Vector2d Lerp(Vector2d Vector0, Vector2d Vector1, double T)
        {
            return Vector0 * (1 - T) + Vector1 * T;
        }

        static public Vector2 Lerp(Vector2 Vector0, Vector2 Vector1, float T)
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

        static public double Round(double value, double size)
        {
            return Math.Round(value/size) * size;
        }

        static public double AngleWrap(double value)
        {
            double tau = Math.PI * 2;
            return ((value % tau) + tau) % tau;
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

        static public double PointLineDistance(Vector2 ps0, Vector2 pe0, Vector2 Point, bool IsSegment)
        {
            return PointLineDistance(new Vector2d(ps0.X, ps0.Y), new Vector2d(pe0.X, pe0.Y), new Vector2d(Point.X, Point.Y), IsSegment);
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

        /// <summary>
        /// Tests if two lines intersect.
        /// </summary>
        /// <returns>Location where the two lines intersect</returns>
        static public IntersectPoint LineIntersection(Vector2d ps0, Vector2d pe0, Vector2d ps1, Vector2d pe1, bool SegmentOnly)
        {
            IntersectPoint v = new IntersectPoint();
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
                        v.Exists = false;
                        return v;
                    }
                }
            }
            else
            {
                v.Exists = false;
                return v;
            }
            v.Exists = true;
            v.Position = Lerp(ps0, pe0, ua);
            v.T = ua;
            return v;
        }

        static public IntersectPoint LineIntersection(Vector2 ps0, Vector2 pe0, Vector2 ps1, Vector2 pe1, bool segmentOnly)
        {
            return LineIntersection(new Vector2d(ps0.X, ps0.Y), new Vector2d(pe0.X, pe0.Y), new Vector2d(ps1.X, ps1.Y), new Vector2d(pe1.X, pe1.Y), segmentOnly);
        }

        static public bool PointInRectangle(Vector2d V0, Vector2d V1, Vector2d Point)
        {
            if (((Point.X >= V0.X && Point.X <= V1.X) || (Point.X <= V0.X && Point.X >= V1.X)) && ((Point.Y >= V0.Y && Point.Y <= V1.Y) || (Point.Y <= V0.Y && Point.Y >= V1.Y)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if a line segment is inside a rectangle
        /// </summary>
        /// <param name="topLeft">Top left corner of rectangle</param>
        /// <param name="bottomRight">Bottom right corner of rectangle</param>
        /// <param name="lineBegin">Beginning of line segment</param>
        /// <param name="lineEnd">Ending of line segment</param>
        /// <returns>True if the line is contained within or intersects the rectangle</returns>
        static public bool LineInRectangle(Vector2d topLeft, Vector2d bottomRight, Vector2d lineBegin, Vector2d lineEnd)
        {
            if (PointInRectangle(topLeft, bottomRight, lineBegin) || PointInRectangle(topLeft, bottomRight, lineEnd))
            {
                return true;
            }
            Vector2d[] v = new Vector2d[4] {
                topLeft,
                new Vector2d(bottomRight.X, topLeft.Y),
                bottomRight,
                new Vector2d(topLeft.X, bottomRight.Y),
            };
            for (int i = 0; i < v.Length; i++)
            {
                if (LineIntersection(v[i], v[(i+1) % v.Length], lineBegin, lineEnd, true).Exists)
                {
                    return true;
                }
            }
            return false;
        }

        static public bool LineInRectangle(Vector2 topLeft, Vector2 bottomRight, Vector2 lineBegin, Vector2 lineEnd)
        {
            return LineInRectangle(new Vector2d(topLeft.X, topLeft.Y), new Vector2d(bottomRight.X, bottomRight.Y), new Vector2d(lineBegin.X, lineBegin.Y), new Vector2d(lineEnd.X, lineEnd.Y));
        }

        /// <summary>Computes the convex hull of a polygon, in clockwise order in a Y-up 
        /// coordinate system (counterclockwise in a Y-down coordinate system).</summary>
        /// <remarks>Uses the Monotone Chain algorithm, a.k.a. Andrew's Algorithm.
        /// Script found at: http://loyc-etc.blogspot.com/2014/05/2d-convex-hull-in-c-45-lines-of-code.html
        /// </remarks>
        public static List<Vector2> ComputeConvexHull(IEnumerable<Vector2> points)
        {
            var list = new List<Vector2>(points);
            return ComputeConvexHull(list, true);
        }
        public static List<Vector2> ComputeConvexHull(List<Vector2> points, bool sortInPlace = false)
        {
            if (points.Count <= 3)
            {
                return points;
            }
            if (!sortInPlace)
                points = new List<Vector2>(points);
            points.Sort((a, b) =>
              a.X == b.X ? a.Y.CompareTo(b.Y) : (a.X > b.X ? 1 : -1));

            List<Vector2> hull = new List<Vector2>();
            int L = 0, U = 0; // size of lower and upper hulls

            // Builds a hull such that the output polygon starts at the leftmost point.
            for (int i = points.Count - 1; i >= 0; i--)
            {
                Vector2 p = points[i];

                // build lower hull (at end of output list)
                while (L >= 2 && Vector2Ext.Cross(hull[hull.Count - 1] - hull[hull.Count - 2], p - hull[hull.Count - 1]) >= 0)
                {
                    hull.RemoveAt(hull.Count - 1);
                    L--;
                }
                hull.Add(p);
                L++;

                // build upper hull (at beginning of output list)

                while (U >= 2 && Vector2Ext.Cross(hull[0] - hull[1], p - hull[0]) <= 0)
                {
                    hull.RemoveAt(0);
                    U--;
                }
                if (U != 0) // when U=0, share the point added above
                    hull.Insert(0, p);
                U++;
                Debug.Assert(U + L == hull.Count + 1);
            }
            hull.RemoveAt(hull.Count - 1);
            return hull;
        }

        /// <summary>
        /// Tests if a point is contained in a closed polygon
        /// </summary>
        /// <param name="polygon">A closed polygon</param>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <remarks>Code was found here http://dominoc925.blogspot.com/2012/02/c-code-snippet-to-determine-if-point-is.html
        /// </remarks>
        public static bool IsPointInPolygon(Vector2[] polygon, Vector2 point)
        {
            bool isInside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }

        /// <summary>
        /// Returns true if vertices are ordered clockwise, false they are counter-clockwise.  It is assumed that the polygon they form is simple.
        /// </summary>
        /// <param name="v">Array of vertices that form a polygon</param>
        /// <returns></returns
        public static bool IsClockwise(Vector2[] polygon)
        {
            Debug.Assert(polygon.Length >= 3, "Polygon must have 3 or more vertices.");
            double signedArea = 0;
            for (int i0 = 0; i0 < polygon.Length; i0++)
            {
                int i1 = (i0 + 1) % polygon.Length;
                signedArea += (polygon[i0].X * polygon[i1].Y - polygon[i1].X * polygon[i0].Y);
            }
            Debug.Assert(signedArea != 0, "Polygon has 0 area.");
            return Math.Sign(signedArea) == -1;
        }

        public static bool IsClockwise(List<Vector2> polygon)
        {
            return IsClockwise(polygon.ToArray());
        }

        /// <summary>
        /// Sets the handedness of a polygon.
        /// </summary>
        /// <param name="polygon">A polygon represented as a list of vectors.</param>
        /// <param name="clockwise">Clockwise if true, C.Clockwise if false.</param>
        /// <returns></returns>
        public static List<Vector2> SetHandedness(List<Vector2> polygon, bool clockwise)
        {
            if (IsClockwise(polygon) != clockwise)
            {
                polygon.Reverse();
            }
            return polygon;
        }

        public static Vector2[] SetHandedness(Vector2[] polygon, bool clockwise)
        {
            if (IsClockwise(polygon) != clockwise)
            {
                Array.Reverse(polygon);
            }
            return polygon;
        }

        /// <summary>
        /// Returns all self intersections in a line strip.  The line strip is defined by an array of Vectors.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="includeTwice">If true, each intersection will be added twice relative to each intersecting line.</param>
        /// <returns></returns>
        public static PolyCoord[] GetLineStripIntersections(Vector2[] vertices, bool includeTwice)
        {
            List<PolyCoord> intersections = new List<PolyCoord>();
            //for now we'll just use the slow O(n^2) implementation
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                for (int j = i + 2; j < vertices.Length - 1; j++)
                {
                    IntersectPoint first = LineIntersection(vertices[i], vertices[i + 1], vertices[j], vertices[j + 1], true);
                    if (first.Exists && first.T < 1)
                    {
                        intersections.Add(new PolyCoord(i, (float)first.T));
                        if (includeTwice)
                        {
                            IntersectPoint second = LineIntersection(vertices[i], vertices[i + 1], vertices[j], vertices[j + 1], true);
                            Debug.Assert(second.Exists);
                            intersections.Add(new PolyCoord(j, (float)second.T));
                        }
                    }
                }
            }
            return intersections.ToArray();
        }
    }
}
