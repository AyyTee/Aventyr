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
        /// <summary>T value for the first line.</summary>
        public double TFirst;
        /// <summary>T value for the second line.</summary>
        public double TLast;

        const float EQUALITY_EPSILON = 0.0000001f;

        public bool Equals(IntersectPoint intersect)
        {
            if (Exists == intersect.Exists && Exists == false)
            {
                return true;
            }
            return Exists == intersect.Exists &&
                (Position - intersect.Position).Length < EQUALITY_EPSILON &&
                Math.Abs(TFirst - intersect.TFirst) < EQUALITY_EPSILON &&
                Math.Abs(TLast - intersect.TLast) < EQUALITY_EPSILON;
        }
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

        static public Vector3d Lerp(Vector3d Vector0, Vector3d Vector1, double T)
        {
            return Vector0 * (1 - T) + Vector1 * T;
        }

        static public Vector3 Lerp(Vector3 Vector0, Vector3 Vector1, float T)
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
                    if (IsSegment) {t = MathHelper.Clamp(t, 0, 1);}
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

        /// <summary>Tests if two lines intersect.</summary>
        /// <returns>Location where the two lines intersect. TFirst is relative to the first line.</returns>
        static public IntersectPoint LineIntersection(Vector2d ps0, Vector2d pe0, Vector2d ps1, Vector2d pe1, bool SegmentOnly)
        {
            IntersectPoint v = new IntersectPoint();
            double ua, ub;
            double ud = (pe1.Y - ps1.Y) * (pe0.X - ps0.X) - (pe1.X - ps1.X) * (pe0.Y - ps0.Y);
            if (ud != 0)
            {
                ua = ((pe1.X - ps1.X) * (ps0.Y - ps1.Y) - (pe1.Y - ps1.Y) * (ps0.X - ps1.X)) / ud;
                ub = ((pe0.X - ps0.X) * (ps0.Y - ps1.Y) - (pe0.Y - ps0.Y) * (ps0.X - ps1.X)) / ud;
                if (SegmentOnly)
                {
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
            v.TFirst = ua;
            v.TLast = ub;
            return v;
        }

        /// <summary>Tests if two lines intersect.</summary>
        /// <returns>Location where the two lines intersect. T is relative to the first line.</returns>
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

        public static bool IsClockwise(List<IntPoint> polygon)
        {
            Debug.Assert(polygon.Count >= 3, "Polygon must have 3 or more vertices.");
            double signedArea = 0;
            for (int i0 = 0; i0 < polygon.Count; i0++)
            {
                int i1 = (i0 + 1) % polygon.Count;
                signedArea += (polygon[i0].X * polygon[i1].Y - polygon[i1].X * polygon[i0].Y);
            }
            Debug.Assert(signedArea != 0, "Polygon has 0 area.");
            return Math.Sign(signedArea) == -1;
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
                    if (first.Exists && first.TFirst < 1)
                    {
                        intersections.Add(new PolyCoord(i, (float)first.TFirst));
                        if (includeTwice)
                        {
                            IntersectPoint second = LineIntersection(vertices[i], vertices[i + 1], vertices[j], vertices[j + 1], true);
                            Debug.Assert(second.Exists);
                            intersections.Add(new PolyCoord(j, (float)second.TFirst));
                        }
                    }
                }
            }
            return intersections.ToArray();
        }

        /// <summary>
        /// Find the bounding box for an array of lines.  A mimimum of one line is required.
        /// </summary>
        public static void GetBBox(Line[] lines, out Vector2 vMin, out Vector2 vMax)
        {
            Debug.Assert(lines.Length > 0, "A minimum of one line is needed for there to be a bounding box.");
            vMin = lines[0][0];
            vMax = lines[0][0];
            for (int i = 0; i < lines.Length; i++)
            {
                vMin = Vector2.ComponentMin(vMin, lines[i][0]);
                vMin = Vector2.ComponentMin(vMin, lines[i][1]);
                vMax = Vector2.ComponentMax(vMax, lines[i][0]);
                vMax = Vector2.ComponentMax(vMax, lines[i][1]);
            }
        }

        /// <summary>Finds the intersections between a line and a circle.  IntersectPoint contains the T value for the intersecting line.</summary>
        /// <param name="circle">Origin of circle.</param>
        /// <param name="radius">Radius of circle.</param>
        /// <param name="line">Line used to check intersections with circle.</param>
        /// <returns>Array of intersections. If no intersections exist then an array of length 0 is returned.</returns>
        /// <remarks>Original code was found here http://csharphelper.com/blog/2014/09/determine-where-a-line-intersects-a-circle-in-c/
        /// </remarks>
        public static IntersectPoint[] GetLineCircleIntersections(Vector2 circle, float radius, Line line, bool isSegment)
        {
            IntersectPoint intersect0 = new IntersectPoint();
            IntersectPoint intersect1 = new IntersectPoint();
            double dx, dy, A, B, C, det, t;

            dx = line[1].X - line[0].X;
            dy = line[1].Y - line[0].Y;

            A = dx * dx + dy * dy;
            B = 2 * (dx * (line[0].X - circle.X) + dy * (line[0].Y - circle.Y));
            C = (line[0].X - circle.X) * (line[0].X - circle.X) +
                (line[0].Y - circle.Y) * (line[0].Y - circle.Y) -
                radius * radius;

            det = B * B - 4 * A * C;
            if ((A <= 0.0000001) || (det < 0))
            {
                // No real solutions.
            }
            else if (det == 0)
            {
                // One solution.
                t = -B / (2 * A);
                if (t >= 0 && t < 1 || !isSegment)
                {
                    intersect0.Position = new Vector2d(line[0].X + t * dx, line[0].Y + t * dy);
                    intersect0.Exists = true;
                    intersect0.TFirst = t;
                    return new IntersectPoint[] { intersect0 };
                }
            }
            else
            {
                // Two solutions.
                List<IntersectPoint> list = new List<IntersectPoint>();
                t = (float)((-B + Math.Sqrt(det)) / (2 * A));
                if (t >= 0 && t < 1 || !isSegment)
                {
                    intersect0.Position = new Vector2d(line[0].X + t * dx, line[0].Y + t * dy);
                    intersect0.Exists = true;
                    intersect0.TFirst = t;
                    list.Add(intersect0);
                }
                
                t = (float)((-B - Math.Sqrt(det)) / (2 * A));
                if (t >= 0 && t < 1 || !isSegment)
                {
                    intersect1.Position = new Vector2d(line[0].X + t * dx, line[0].Y + t * dy);
                    intersect1.Exists = true;
                    intersect1.TFirst = t;
                    list.Add(intersect1);
                }
                return list.ToArray();
            }
            return new IntersectPoint[0];
        }

        /// <summary>Get the area of a polygon.</summary>
        /// <remarks>Original code was found here http://csharphelper.com/blog/2014/07/calculate-the-area-of-a-polygon-in-c/
        /// </remarks>
        public static double GetArea(Vector2[] polygon)
        {
            // Add the first point to the end.
            int num_points = polygon.Length;
            Vector2[] pts = new Vector2[num_points + 1];
            polygon.CopyTo(pts, 0);
            pts[num_points] = polygon[0];

            // Get the areas.
            float area = 0;
            for (int i = 0; i < num_points; i++)
            {
                area +=
                    (pts[i + 1].X - pts[i].X) *
                    (pts[i + 1].Y + pts[i].Y) / 2;
            }

            // Return the result.
            return Math.Abs(area);
        }

        /// <summary>
        /// Check if a simple polygon is convex.
        /// </summary>
        /// <param name="polygon"></param>
        /// <remarks>Original code was found here http://stackoverflow.com/questions/471962/how-do-determine-if-a-polygon-is-complex-convex-nonconvex </remarks>
        public static bool IsConvex(Vector2[] polygon)
        {
            if (polygon.Length < 4)
            {
                return true;
            }
            bool sign = false;
            int n = polygon.Length;
            for (int i = 0; i < n; i++)
            {
                double dx1 = polygon[(i + 2) % n].X - polygon[(i + 1) % n].X;
                double dy1 = polygon[(i + 2) % n].Y - polygon[(i + 1) % n].Y;
                double dx2 = polygon[i].X - polygon[(i + 1) % n].X;
                double dy2 = polygon[i].Y - polygon[(i + 1) % n].Y;
                double zcrossproduct = dx1 * dy2 - dy1 * dx2;
                if (i == 0)
                {
                    sign = zcrossproduct > 0;
                }
                else
                {
                    if (sign != (zcrossproduct > 0))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static Matrix4d GetHomography(Vector2[] src, Vector2[] dest)
        {
            Debug.Assert(src.Length == 4 && dest.Length == 4, "Source and destination quads must have 4 vertices each.");
            if (IsConvex(src) != IsConvex(dest))
            {
                throw new ExceptionInvalidPolygon();
            }
            Matrix4d mat = QuadToSquare(src[0], src[1], src[2], src[3]);
            mat *= SquareToQuad(dest[0], dest[1], dest[2], dest[3]);
            
            mat.Column2 = mat.Column3;
            mat.Column3 = Matrix4d.Identity.Column3;
            //mat.M34 = 1 - mat.M34;
            /*mat.M43 = 0;
            mat.M44 = 1;*/

            return mat;
        }

        private static Matrix4d SquareToQuad(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            double dx1 = v1.X - v2.X, dy1 = v1.Y - v2.Y;
            double dx2 = v3.X - v2.X, dy2 = v3.Y - v2.Y;
            double sx = v0.X - v1.X + v2.X - v3.X;
            double sy = v0.Y - v1.Y + v2.Y - v3.Y;
            double g = (sx * dy2 - dx2 * sy) / (dx1 * dy2 - dx2 * dy1);
            double h = (dx1 * sy - sx * dy1) / (dx1 * dy2 - dx2 * dy1);
            double a = v1.X - v0.X + g * v1.X;
            double b = v3.X - v0.X + h * v3.X;
            double c = v0.X;
            double d = v1.Y - v0.Y + g * v1.Y;
            double e = v3.Y - v0.Y + h * v3.Y;
            double f = v0.Y;

            Matrix4d mat = new Matrix4d();
            mat.M11 = a; mat.M12 = d; mat.M13 = 0; mat.M14 = g;
            mat.M21 = b; mat.M22 = e; mat.M23 = 0; mat.M24 = h;
            mat.M31 = 0; mat.M32 = 0; mat.M33 = 1; mat.M34 = 0;
            mat.M41 = c; mat.M42 = f; mat.M43 = 0; mat.M44 = 1;
            return mat;
        }

        private static Matrix4d QuadToSquare(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            Matrix4d mat = SquareToQuad(v0, v1, v2, v3);

            //invert through adjoint
            double a = mat.M11, d = mat.M12,	/*ignore*/ 	g = mat.M14;
            double b = mat.M21, e = mat.M22, /*3rd col*/	h = mat.M24;
            /*ignore 3rd row*/
            double c = mat.M41, f = mat.M42;

            double a1 = e - f * h;
            double b1 = c * h - b;
            double c1 = b * f - c * e;
            double d1 = f * g - d;
            double e1 = a - c * g;
            double f1 = c * d - a * f;
            double g1 = d * h - e * g;
            double h1 = b * g - a * h;
            double i1 = a * e - b * d;

            double idet = 1.0f / (a * a1 + b * d1 + c * g1);

            mat.M11 = a1 * idet;    mat.M12 = d1 * idet;    mat.M13 = 0; mat.M14 = g1 * idet;
            mat.M21 = b1 * idet;    mat.M22 = e1 * idet;    mat.M23 = 0; mat.M24 = h1 * idet;
            mat.M31 = 0;            mat.M32 = 0;            mat.M33 = 1; mat.M34 = 0;
            mat.M41 = c1 * idet;    mat.M42 = f1 * idet;    mat.M43 = 0; mat.M44 = i1 * idet;
            return mat;
        }

        /// <summary>
        /// Bisects a 3d triangle with a plane perpendicular to the xy-plane.
        /// </summary>
        /// <param name="triangle">Triangle to bisect.</param>
        /// <param name="bisector">Bisection plane defined by a line on the xy-plane.</param>
        /// <param name="keepSide">Which side of the bisector to not remove from the triangle.</param>
        /// <returns>Triangles not removed by the bisection.  Will either be 0,1,2 triangles.</returns>
        public static Triangle[] BisectTriangle(Triangle triangle, Line bisector, Line.Side keepSide = Line.Side.IsLeftOf)
        {
            Debug.Assert(triangle != null);
            Debug.Assert(bisector != null);
            Debug.Assert(keepSide != Line.Side.IsNeither);
            Vector2[] vertices = new Vector2[]
            {
                new Vector2(triangle[0].Position.X, triangle[0].Position.Y),
                new Vector2(triangle[1].Position.X, triangle[1].Position.Y),
                new Vector2(triangle[2].Position.X, triangle[2].Position.Y)
            };

            List<Vertex> keep = new List<Vertex>();
            int intersectCount = 0;
            for (int i = 0; i < Triangle.VERTEX_COUNT; i++)
            {
                Line.Side side = bisector.GetSideOf(vertices[i], false);
                if (side == keepSide || side == Line.Side.IsNeither)
                {
                    keep.Add(triangle[i]);
                }
                Line edge = new Line(vertices[i], vertices[(i + 1) % Triangle.VERTEX_COUNT]);
                IntersectPoint intersect = edge.Intersects(bisector, true);
                if (intersect.Exists && intersect.TFirst > 0 && intersect.TFirst < 1)
                {
                    intersectCount++;
                    Debug.Assert(intersectCount <= 2);
                    int index = (i + 1) % Triangle.VERTEX_COUNT;
                    keep.Add(Vertex.Lerp(triangle[i], triangle[index], (float)intersect.TFirst));
                }
            }

            Debug.Assert(keep.Count <= 4);
            if (keep.Count == 3)
            {
                return new Triangle[]
                {
                    new Triangle(keep[0], keep[1], keep[2])
                };
            }
            else if (keep.Count == 4)
            {
                return new Triangle[]
                {
                    new Triangle(keep[0], keep[1], keep[2]),
                    new Triangle(keep[0], keep[2], keep[3])
                };
            }
            else
            {
                return new Triangle[0];
            }
        }
    }
}
