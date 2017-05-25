using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClipperLib;
using Game.Models;
using OpenTK;
using MathHelper = OpenTK.MathHelper;
using Vector2 = OpenTK.Vector2;
using Vector3 = OpenTK.Vector3;
using Xna = Microsoft.Xna.Framework;

namespace Game.Common
{
    public class IntersectCoord : IAlmostEqual<IntersectCoord, double>
    {
        public readonly Vector2d Position;
        /// <summary>T value for the first line.</summary>
        public readonly double First;
        /// <summary>T value for the second line.</summary>
        public readonly double Last;

        const float EqualityEpsilon = 0.0000001f;

        public IntersectCoord(Vector2d position, double first, double last)
        {
            Position = position;
            First = first;
            Last = last;
        }

        public bool AlmostEqual(IntersectCoord intersect, double delta = EqualityEpsilon)
        {
            if (intersect == null)
            {
                return false;
            }
            return MathEx.AlmostEqual(Position.X, intersect.Position.X, delta) &&
                MathEx.AlmostEqual(Position.Y, intersect.Position.Y, delta) &&
                MathEx.AlmostEqual(First, intersect.First, delta) &&
                MathEx.AlmostEqual(Last, intersect.Last, delta);
        }

        public bool AlmostEqual(IntersectCoord intersect, double delta, double ratioDelta)
        {
            if (intersect == null)
            {
                return false;
            }
            return MathEx.AlmostEqual(Position.X, intersect.Position.X, delta, ratioDelta) &&
                MathEx.AlmostEqual(Position.Y, intersect.Position.Y, delta, ratioDelta) &&
                MathEx.AlmostEqual(First, intersect.First, delta, ratioDelta) &&
                MathEx.AlmostEqual(Last, intersect.Last, delta, ratioDelta);
        }
    }

    public static class MathEx
    {
        public const double Tau = Math.PI * 2;

        #region Lerp
        public static double Lerp(double value0, double value1, double T) => value0 * (1 - T) + value1 * T;

        public static double LerpAngle(double angle0, double angle1, double T, bool isClockwise)
        {
            if (isClockwise)
            {
                if (angle0 <= angle1)
                {
                    angle0 += 2 * Math.PI;
                }
                return Lerp(angle0, angle1, T) % (2 * Math.PI);
            }
            if (angle0 > angle1)
            {
                angle1 += 2 * Math.PI;
            }

            return Lerp(angle0, angle1, T) % (2 * Math.PI);
        }
        #endregion
        #region Nearest
        /// <summary>
        /// Find the nearest PolygonCoord on the polygon relative to provided point.
        /// </summary>
        public static PolygonCoord PointPolygonNearest(IList<Vector2> polygon, Vector2 point)
        {
            var nearest = new PolygonCoord(0, 0);
            double distanceMin = -1;
            for (int i = 0; i < polygon.Count; i++)
            {
                int iNext = (i + 1) % polygon.Count;
                var edge = new LineF(polygon[i], polygon[iNext]);
                double distance = PointLineDistance(point, edge, true);
                if (distanceMin == -1 || distance < distanceMin)
                {
                    nearest = new PolygonCoord(i, edge.NearestT(point, true));
                    distanceMin = distance;
                }
            }
            return nearest;
        }
        #endregion
        #region Distance
        public static double PointLineDistance(Vector2d point, Line line, bool isSegment)
        {
            Vector2d v;
            Vector2d vDelta = line[1] - line[0];
            if (vDelta.X == 0 && vDelta.Y == 0)
            {
                v = line[0];
            }
            else
            {
                double t = ((point.X - line[0].X) * vDelta.X + (point.Y - line[0].Y) * vDelta.Y) / (Math.Pow(vDelta.X, 2) + Math.Pow(vDelta.Y, 2));
                Debug.Assert(double.IsNaN(t) == false);
                if (isSegment) { t = MathHelper.Clamp(t, 0, 1); }
                v = line[0] + Vector2d.Multiply(vDelta, t);
            }
            double distance = (point - v).Length;
            Debug.Assert(distance >= 0);
            return distance;
        }

        public static double PointLineDistance(Xna.Vector2 point, LineF line, bool isSegment)
        {
            return PointLineDistance((Vector2)point, line, isSegment);
        }

        public static double PointLineDistance(Vector2 point, LineF line, bool isSegment)
        {
            return PointLineDistance((Vector2d)point, line, isSegment);
        }

        public static double PointPolygonDistance(Vector2 point, IList<Vector2> polygon)
        {
            var edge = new LineF(polygon[0], polygon[1]);
            double distMin = PointLineDistance(point, edge, true);
            for (int i = 1; i < polygon.Count; i++)
            {
                edge = new LineF(polygon[i], polygon[(i + 1) % polygon.Count]);
                distMin = Math.Min(distMin, PointLineDistance(point, edge, true));
            }
            if (PointInPolygon(point, polygon))
            {
                distMin = -distMin;
            }
            return distMin;
        }

        /// <summary>
        /// Returns the distance between a line and polygon. 
        /// If the line is contained in the polygon then the distance will be negative value.
        /// </summary>
        public static double LinePolygonDistance(LineF line, IList<Vector2> polygon)
        {
            LineF edge = new LineF(polygon[0], polygon[1]);
            double distMin = LineLineDistance(line, edge);
            for (int i = 1; i < polygon.Count; i++)
            {
                edge = new LineF(polygon[i], polygon[(i + 1) % polygon.Count]);
                distMin = Math.Min(distMin, LineLineDistance(line, edge));
            }
            if (PointInPolygon(line[0], polygon))
            {
                distMin = -distMin;
            }
            return distMin;
        }

        public static double LineLineDistance(LineF line0, LineF line1)
        {
            if (LineLineIntersect(line0, line1, true) != null)
            {
                return 0;
            }
            return new[] {
                PointLineDistance(line1[0], line0, true),
                PointLineDistance(line1[1], line0, true),
                PointLineDistance(line0[0], line1, true),
                PointLineDistance(line0[1], line1, true)
            }.Min();
        }
        #endregion
        #region Inside
        public static bool PointInRectangle(Vector2d v0, Vector2d v1, Vector2d point)
        {
            if (((point.X >= v0.X && point.X <= v1.X) || (point.X <= v0.X && point.X >= v1.X)) && ((point.Y >= v0.Y && point.Y <= v1.Y) || (point.Y <= v0.Y && point.Y >= v1.Y)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the line segment is at least partially contained in a polygon
        /// </summary>
        /// <param name="line"></param>
        /// <param name="polygon">A closed polygon</param>
        /// <returns></returns>
        public static bool LineInPolygon(LineF line, IList<Vector2> polygon)
        {
            if (PointInPolygon(line[0], polygon) || PointInPolygon(line[1], polygon))
            {
                return true;
            }
            return LinePolygonIntersect(line, polygon).Count > 0;
        }

        /// <summary>
        /// Tests if a point is contained in a polygon.
        /// </summary>
        /// <remarks>
        /// Code was found here http://dominoc925.blogspot.com/2012/02/c-code-snippet-to-determine-if-point-is.html
        /// </remarks>
        public static bool PointInPolygon(Vector2 point, IList<Vector2> polygon)
        {
            Debug.Assert(polygon != null);
            Debug.Assert(polygon.Count >= 2);
            bool isInside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
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
        /// Check if a line segment is inside a rectangle
        /// </summary>
        /// <param name="topLeft">Top left corner of rectangle</param>
        /// <param name="bottomRight">Bottom right corner of rectangle</param>
        /// <param name="lineBegin">Beginning of line segment</param>
        /// <param name="lineEnd">Ending of line segment</param>
        /// <returns>True if the line is contained within or intersects the rectangle</returns>
        public static bool LineInRectangle(Vector2d topLeft, Vector2d bottomRight, Vector2d lineBegin, Vector2d lineEnd)
        {
            if (PointInRectangle(topLeft, bottomRight, lineBegin) || PointInRectangle(topLeft, bottomRight, lineEnd))
            {
                return true;
            }
            Vector2d[] v = {
                topLeft,
                new Vector2d(bottomRight.X, topLeft.Y),
                bottomRight,
                new Vector2d(topLeft.X, bottomRight.Y),
            };
            for (int i = 0; i < v.Length; i++)
            {
                if (LineLineIntersect(new LineF(v[i], v[(i + 1) % v.Length]), new LineF(lineBegin, lineEnd), true) != null)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool LineInRectangle(Vector2 topLeft, Vector2 bottomRight, Vector2 lineBegin, Vector2 lineEnd)
        {
            return LineInRectangle(
                new Vector2d(topLeft.X, topLeft.Y), 
                new Vector2d(bottomRight.X, bottomRight.Y), 
                new Vector2d(lineBegin.X, lineBegin.Y), 
                new Vector2d(lineEnd.X, lineEnd.Y));
        }
        #endregion
        #region Convex Hull
        /// <summary>Computes the convex hull of a polygon, in clockwise order in a Y-up 
        /// coordinate system (counterclockwise in a Y-down coordinate system).</summary>
        /// <remarks>Uses the Monotone Chain algorithm, a.k.a. Andrew's Algorithm.
        /// Script found at http://loyc-etc.blogspot.com/2014/05/2d-convex-hull-in-c-45-lines-of-code.html
        /// </remarks>
        public static List<Vector2> GetConvexHull(List<Vector2> points, bool sortInPlace = false)
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
            int l = 0, u = 0; // size of lower and upper hulls

            // Builds a hull such that the output polygon starts at the leftmost point.
            for (int i = points.Count - 1; i >= 0; i--)
            {
                Vector2 p = points[i];

                // build lower hull (at end of output list)
                while (l >= 2 && Vector2Ex.Cross(hull[hull.Count - 1] - hull[hull.Count - 2], p - hull[hull.Count - 1]) >= 0)
                {
                    hull.RemoveAt(hull.Count - 1);
                    l--;
                }
                hull.Add(p);
                l++;

                // build upper hull (at beginning of output list)

                while (u >= 2 && Vector2Ex.Cross(hull[0] - hull[1], p - hull[0]) <= 0)
                {
                    hull.RemoveAt(0);
                    u--;
                }
                if (u != 0) // when U=0, share the point added above
                    hull.Insert(0, p);
                u++;
                Debug.Assert(u + l == hull.Count + 1);
            }
            hull.RemoveAt(hull.Count - 1);
            return hull;
        }
        #endregion
        #region Polygon Winding Order

        /// <summary>
        /// Returns true if vertices are ordered clockwise, false they are counter-clockwise.  It is assumed that the polygon they form is simple.
        /// </summary>
        /// <param name="polygon"></param>
        public static bool IsClockwise(IList<Vector2> polygon)
        {
            Debug.Assert(polygon.Count >= 3, "Polygon must have 3 or more vertices.");
            Debug.Assert(PolygonEx.IsSimple(polygon));
            double signedArea = 0;
            for (int i = 0; i < polygon.Count; i++)
            {
                int iNext = (i + 1) % polygon.Count;
                signedArea += (polygon[i].X * polygon[iNext].Y - polygon[iNext].X * polygon[i].Y);
            }
            Debug.Assert(signedArea != 0, "Polygon has 0 area.");
            return Math.Sign(signedArea) == -1;
        }

        public static bool IsClockwise(IList<Xna.Vector2> polygon)
        {
            Debug.Assert(polygon.Count >= 3, "Polygon must have 3 or more vertices.");
            double signedArea = 0;
            for (int i = 0; i < polygon.Count; i++)
            {
                int iNext = (i + 1) % polygon.Count;
                signedArea += (polygon[i].X * polygon[iNext].Y - polygon[iNext].X * polygon[i].Y);
            }
            Debug.Assert(signedArea != 0, "Polygon has 0 area.");
            return Math.Sign(signedArea) == -1;
        }

        public static bool IsClockwise(List<IntPoint> polygon)
        {
            Debug.Assert(polygon.Count >= 3, "Polygon must have 3 or more vertices.");
            double signedArea = 0;
            for (int i = 0; i < polygon.Count; i++)
            {
                int iNext = (i + 1) % polygon.Count;
                signedArea += (polygon[i].X * polygon[iNext].Y - polygon[iNext].X * polygon[i].Y);
            }
            Debug.Assert(signedArea != 0, "Polygon has 0 area.");
            return Math.Sign(signedArea) == -1;
        }

        /// <summary>
        /// Returns a copy of the polygon with the new winding order set.
        /// </summary>
        /// <param name="polygon">A polygon represented as a list of vectors.</param>
        /// <param name="clockwise">Clockwise if true, C.Clockwise if false.</param>
        /// <returns></returns>
        public static Vector2[] SetWinding(Vector2[] polygon, bool clockwise)
        {
            if (IsClockwise(polygon) != clockwise)
            {
                return polygon.Reverse().ToArray();
            }
            return polygon;
        }

        /// <summary>
        /// Returns a copy of the polygon with the new winding order set.
        /// </summary>
        /// <param name="polygon">A polygon represented as a list of vectors.</param>
        /// <param name="clockwise">Clockwise if true, C.Clockwise if false.</param>
        /// <returns></returns>
        public static List<Vector2> SetWinding(List<Vector2> polygon, bool clockwise)
        {
            var copy = new List<Vector2>(polygon);
            if (IsClockwise(polygon) != clockwise)
            {
                copy.Reverse();
                return copy;
            }
            return polygon;
        }

        public static IList<Xna.Vector2> SetWinding(IList<Xna.Vector2> polygon, bool clockwise)
        {
            if (IsClockwise(polygon) != clockwise)
            {
                return polygon.Reverse().ToList();
            }
            return polygon;
        }
        #endregion
        #region Intersections
        /// <summary>Tests if two lines intersect.</summary>
        /// <returns>Location where the two lines intersect. TFirst is relative to the first line.</returns>
        public static IntersectCoord LineLineIntersect(Line line0, Line line1, bool segmentOnly)
        {

            double ua, ub;
            double ud = (line1[1].Y - line1[0].Y) * (line0[1].X - line0[0].X) - (line1[1].X - line1[0].X) * (line0[1].Y - line0[0].Y);
            if (ud != 0)
            {
                ua = ((line1[1].X - line1[0].X) * (line0[0].Y - line1[0].Y) - (line1[1].Y - line1[0].Y) * (line0[0].X - line1[0].X)) / ud;
                ub = ((line0[1].X - line0[0].X) * (line0[0].Y - line1[0].Y) - (line0[1].Y - line0[0].Y) * (line0[0].X - line1[0].X)) / ud;
                if (segmentOnly)
                {
                    if (ua < 0 || ua > 1 || ub < 0 || ub > 1)
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
            return new IntersectCoord(line0[0].Lerp(line0[1], ua), ua, ub);
        }

        /// <summary>
        /// Returns all self intersections in a line strip.  The line strip is defined by an array of Vectors.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="includeTwice">If true, each intersection will be added twice relative to each intersecting line.</param>
        /// <returns></returns>
        public static PolygonCoord[] LineStripIntersect(Vector2[] vertices, bool includeTwice)
        {
            var intersections = new List<PolygonCoord>();
            //for now we'll just use the slow O(n^2) implementation
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                for (int j = i + 2; j < vertices.Length - 1; j++)
                {
                    IntersectCoord first = LineLineIntersect(new LineF(vertices[i], vertices[i + 1]), new LineF(vertices[j], vertices[j + 1]), true);
                    if (first != null && first.First < 1)
                    {
                        intersections.Add(new PolygonCoord(i, (float)first.First));
                        if (includeTwice)
                        {
                            IntersectCoord second = LineLineIntersect(new LineF(vertices[i], vertices[i + 1]), new LineF(vertices[j], vertices[j + 1]), true);
                            Debug.Assert(second != null);
                            intersections.Add(new PolygonCoord(j, (float)second.First));
                        }
                    }
                }
            }
            return intersections.ToArray();
        }

        /// <summary>
        /// Checks if the line intersects the edges of a polygon.  This does not test if the line is contained in the polygon.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="polygon">A closed polygon</param>
        /// <returns>An intersection point</returns>
        public static List<PolygonCoord> LinePolygonIntersect(LineF line, IList<Vector2> polygon)
        {
            List<PolygonCoord> points = new List<PolygonCoord>();
            for (int i0 = 0; i0 < polygon.Count; i0++)
            {
                int i1 = (i0 + 1) % polygon.Count;
                IntersectCoord intersect = LineLineIntersect(line, new LineF(polygon[i0], polygon[i1]), true);

                if (intersect != null)
                {
                    points.Add(new PolygonCoord(i0, (float)intersect.Last));
                }
            }
            return points;
        }

        /// <summary>Finds the intersections between a line and a circle.  IntersectPoint contains the T value for the intersecting line.</summary>
        /// <param name="circle">Origin of circle.</param>
        /// <param name="radius">Radius of circle.</param>
        /// <param name="line">Line used to check intersections with circle.</param>
        /// <param name="isSegment"></param>
        /// <returns>Array of intersections. If no intersections exist then an array of length 0 is returned.</returns>
        /// <remarks>Original code was found here 
        /// http://csharphelper.com/blog/2014/09/determine-where-a-line-intersects-a-circle-in-c/
        /// </remarks>
        public static IntersectCoord[] LineCircleIntersect(Vector2 circle, float radius, LineF line, bool isSegment)
        {
            double dx = line[1].X - line[0].X;
            double dy = line[1].Y - line[0].Y;

            double a = dx * dx + dy * dy;
            double b = 2 * (dx * (line[0].X - circle.X) + dy * (line[0].Y - circle.Y));
            double c = (line[0].X - circle.X) * (line[0].X - circle.X) +
                       (line[0].Y - circle.Y) * (line[0].Y - circle.Y) -
                       radius * radius;

            double det = b * b - 4 * a * c;
            if ((a <= 0.0000001) || (det < 0))
            {
                // No real solutions.
            }
            else if (det == 0)
            {
                // One solution.
                double t = -b / (2 * a);
                if (t >= 0 && t < 1 || !isSegment)
                {
                    return new[] { new IntersectCoord(new Vector2d(line[0].X + t * dx, line[0].Y + t * dy), t, 0) };
                }
            }
            else
            {
                // Two solutions.
                var list = new List<IntersectCoord>();
                double t = (float)((-b + Math.Sqrt(det)) / (2 * a));
                if (t >= 0 && t < 1 || !isSegment)
                {
                    list.Add(new IntersectCoord(new Vector2d(line[0].X + t * dx, line[0].Y + t * dy), t, 0));
                }

                t = (float)((-b - Math.Sqrt(det)) / (2 * a));
                if (t >= 0 && t < 1 || !isSegment)
                {
                    list.Add(new IntersectCoord(new Vector2d(line[0].X + t * dx, line[0].Y + t * dy), t, 0));
                }
                return list.ToArray();
            }
            return new IntersectCoord[0];
        }

        public static List<GeometryUtil.Sweep> MovingPointLineIntersect(Line point, Line lineStart, Line lineEnd)
        {
            return GeometryUtil.WhenLineSweepsPoint(
                point[0],
                lineStart,
                lineEnd.Translate(-point.Delta)
                ).ToList();
        }

        /// <summary>
        /// Returns a list of moving line on line collisions sorted by time of collision in ascending order.
        /// </summary>
        public static List<GeometryUtil.Sweep> MovingLineLineIntersect(Line line0Start, Line line0End, Line line1Start, Line line1End)
        {
            List<GeometryUtil.Sweep> list = new List<GeometryUtil.Sweep>();
            list.AddRange(GeometryUtil.WhenLineSweepsPoint(line0Start[0], line1Start, line1End.Translate(-line0End[0])));
            list.AddRange(GeometryUtil.WhenLineSweepsPoint(line0Start[1], line1Start, line1End.Translate(-line0End[1])));
            list.AddRange(GeometryUtil.WhenLineSweepsPoint(line1Start[0], line0Start, line0End.Translate(-line1End[0])));
            list.AddRange(GeometryUtil.WhenLineSweepsPoint(line1Start[1], line0Start, line0End.Translate(-line1End[1])));
            return list.OrderBy(item => item.TimeProportion).ToList();
        }
        #endregion
        #region Homography
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

        static Matrix4d SquareToQuad(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
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

            var mat = new Matrix4d
            {
                M11 = a,
                M12 = d,
                M13 = 0,
                M14 = g,
                M21 = b,
                M22 = e,
                M23 = 0,
                M24 = h,
                M31 = 0,
                M32 = 0,
                M33 = 1,
                M34 = 0,
                M41 = c,
                M42 = f,
                M43 = 0,
                M44 = 1
            };
            return mat;
        }

        static Matrix4d QuadToSquare(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            Matrix4d mat = SquareToQuad(v0, v1, v2, v3);

            //invert through adjoint
            double a = mat.M11, d = mat.M12,    /*ignore*/    g = mat.M14;
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

            mat.M11 = a1 * idet; mat.M12 = d1 * idet; mat.M13 = 0; mat.M14 = g1 * idet;
            mat.M21 = b1 * idet; mat.M22 = e1 * idet; mat.M23 = 0; mat.M24 = h1 * idet;
            mat.M31 = 0; mat.M32 = 0; mat.M33 = 1; mat.M34 = 0;
            mat.M41 = c1 * idet; mat.M42 = f1 * idet; mat.M43 = 0; mat.M44 = i1 * idet;
            return mat;
        }
        #endregion

        public static double LineToAngle(Vector2d v0, Vector2d v1) => VectorToAngle(v1 - v0);
        
        public static double LineToAngle(Vector2 v0, Vector2 v1) => LineToAngle(new Vector2d(v0.X, v0.Y), new Vector2d(v1.X, v1.Y));

        public static double VectorToAngle(Vector2d v0)
        {
            double val = Math.Atan2(v0.X, v0.Y);
            return double.IsNaN(val) ?
                0 :
                (val + 2 * Math.PI) % (2 * Math.PI) - Math.PI / 2;
        }

        public static double VectorToAngle(Vector2 v0) => VectorToAngle(new Vector2d(v0.X, v0.Y));

        public static double AngleDiff(Vector2 v0, Vector2 v1) => AngleDiff(VectorToAngle(v0), VectorToAngle(v1));

        public static double AngleDiff(double angle0, double angle1)
        {
            return ((angle1 - angle0) % (Math.PI * 2) + Math.PI * 3) % (Math.PI * 2) - Math.PI;
        }

        public static Vector2 AngleToVector(float angle)
        {
            return new Vector2((float)Math.Cos(angle), (float)-Math.Sin(angle));
        }

        public static Vector2d AngleToVector(double angle)
        {
            return new Vector2d(Math.Cos(angle), -Math.Sin(angle));
        }

        /// <summary>
        /// Returns the difference between two numbers on a looping numberline.
        /// </summary>
        /// <param name="val0"></param>
        /// <param name="val1"></param>
        /// <param name="wrapSize"></param>
        /// <returns></returns>
        public static int ValueDiff(int val0, int val1, int wrapSize)
        {
            return ((val1 - val0) % (wrapSize) + (wrapSize * 3) / 2) % (wrapSize) - wrapSize / 2;
        }

        public static double ValueWrap(double value, double mod)
        {
            Debug.Assert(mod > 0);
            var newValue = value % mod;
            return newValue < 0 ?
                mod + newValue :
                newValue;
        }

        public static int ValueWrap(int value, int mod)
        {
            Debug.Assert(mod > 0);
            var newValue = value % mod;
            return newValue < 0 ?
                mod + newValue :
                newValue;
        }

        public static double Round(double value, double size) => Math.Round(value / size) * size;

        public static double AngleWrap(double value) => ((value % Tau) + Tau) % Tau;

        public static Vector2 AngularVelocity(Vector2 point, Vector2 pivotPoint, float rotationSpeed) => AngularVelocity(point - pivotPoint, rotationSpeed);

        public static Vector2 AngularVelocity(Vector2 offset, float rotationSpeed) => offset.PerpendicularLeft * rotationSpeed;

        /// <summary>
        /// Find the bounding box for an array of lines.  A mimimum of one line is required.
        /// </summary>
        public static void GetBBox(LineF[] lines, out Vector2 vMin, out Vector2 vMax)
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

        /// <summary>Get the area of a polygon.</summary>
        /// <remarks>Original code was found here 
        /// http://csharphelper.com/blog/2014/07/calculate-the-area-of-a-polygon-in-c/
        /// </remarks>
        public static double GetArea(IList<Vector2> polygon)
        {
            // Add the first point to the end.
            int numPoints = polygon.Count;
            Vector2[] pts = new Vector2[numPoints + 1];
            polygon.CopyTo(pts, 0);
            pts[numPoints] = polygon[0];

            // Get the areas.
            float area = 0;
            for (int i = 0; i < numPoints; i++)
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
        /// <remarks>Original code was found here 
        /// http://stackoverflow.com/questions/471962/how-do-determine-if-a-polygon-is-complex-convex-nonconvex 
        /// </remarks>
        public static bool IsConvex(IList<Vector2> polygon)
        {
            if (polygon.Count < 4)
            {
                return true;
            }
            bool sign = false;
            int n = polygon.Count;
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

        /// <summary>
        /// Check if two lists are isomorphic. In otherwords, they are equal if they contain the same 
        /// elements in the same order but potentially with an index offset.
        /// </summary>
        /// <param name="second"></param>
        /// <param name="equality">Method that tests the equality of elements in first and second.</param>
        /// <param name="noReversing">First and second are not equal if they are in reverse order.</param>
        /// <param name="first"></param>
        public static bool IsIsomorphic<T>(IList<T> first, IList<T> second, Func<T, T, bool> equality, bool noReversing = false)
        {
            Debug.Assert(first != null);
            Debug.Assert(second != null);
            if (first.Count != second.Count)
            {
                return false;
            }
            int offset = 0;
            for (int i = 0; i < first.Count; i++)
            {
                int j = (i + offset) % first.Count;

                if (!equality.Invoke(first[i], second[j]))
                {
                    i = 0;
                    offset++;
                    if (offset > first.Count)
                    {
                        if (noReversing)
                        {
                            return false;
                        }
                        return IsIsomorphic(first.Reverse().ToList(), second, equality, true);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Check if two lists are isomorphic. In otherwords, they are equal if they contain the same 
        /// elements in the same order but potentially with an index offset.
        /// </summary>
        /// <param name="second"></param>
        /// <param name="noReversing">First and second are not equal if they are in reverse order.</param>
        /// <param name="first"></param>
        public static bool IsIsomorphic<T>(IList<T> first, IList<T> second, bool noReversing = false)
        {
            return IsIsomorphic(first, second, (itemFirst, itemSecond) => itemFirst.Equals(itemSecond), noReversing);
        }

        /// <summary>
        /// Compares two doubles and returns if they are almost equal. 
        /// Primarily intended as a helper function for other classes implementing IAlmostEquals.
        /// </summary>
        /// <param name="delta">The maximum difference between the two values.</param>
        /// <returns></returns>
        public static bool AlmostEqual(double value0, double value1, double delta)
        {
            return Math.Abs(value1 - value0) <= delta;
        }

        /// <summary>
        /// Compares two doubles and returns if they are almost equal. 
        /// Primarily intended as a helper function for other classes implementing IAlmostEquals.
        /// </summary>
        /// <param name="delta">The maximum difference between the two values.</param>
        /// <param name="ratioDelta">The maximum ratio between the two values.</param>
        public static bool AlmostEqual(double value0, double value1, double delta, double ratioDelta)
        {
            if (value0 == 0)
            {
                return false;
            }
            return Math.Abs(1 - Math.Min(value0, value1) / Math.Max(value0, value1)) <= ratioDelta || Math.Abs(value1 - value0) <= delta;
        }

        /// <summary>
        /// Get incenter of a given triangle. The incenter defined as the intersection point of 3 edges that are angle bisectors of the triangle vertices.
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns></returns>
        public static Vector2d GetTriangleIncenter(Vector2d[] triangle)
        {
            Debug.Assert(triangle.Length == 3);
            var edgeLength = new[]
            {
                (triangle[2] - triangle[1]).Length,
                (triangle[0] - triangle[2]).Length,
                (triangle[1] - triangle[0]).Length
            };
            return (edgeLength[0] * triangle[0] + edgeLength[1] * triangle[1] + edgeLength[2] * triangle[2]) / edgeLength.Sum();
        }

        /// <summary>
        /// Get incenter of a given triangle. The incenter defined as the intersection point of 3 edges that are angle bisectors of the triangle vertices.
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns></returns>
        public static Vector2 GetTriangleIncenter(Vector2[] triangle)
        {
            return (Vector2)GetTriangleIncenter(Vector2Ex.ToDouble(triangle));
        }
    }
}
