using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Game
{
    public static class GeometryUtil
    {
        /// <summary>
        /// Enumerates the times and places, if any, where a moving line crosses a point during a time step.
        /// The line's endpoints move at a constant rate along their linear paths.
        /// </summary>
        /// <remarks>Original code found here https://github.com/Strilanc/Methods/blob/master/Methods/Methods/LineSweepPoint/GeometryUtil.cs </remarks>
        public static IEnumerable<Sweep> WhenLineSweepsPoint(Vector2d point, Line pathOfLineStartPoint, Line pathOfLineEndPoint)
        {
            var a = point - pathOfLineStartPoint[0];
            var b = -pathOfLineStartPoint.Delta;
            var c = pathOfLineEndPoint[0] - pathOfLineStartPoint[0];
            var d = pathOfLineEndPoint.Delta - pathOfLineStartPoint.Delta;

            //return from t in QuadraticRoots(b.Cross(d), a.Cross(d) + b.Cross(c), a.Cross(c))
            return from across in QuadraticRoots(Vector2Ext.Cross(b, d), Vector2Ext.Cross(a, d) + Vector2Ext.Cross(b, c), Vector2Ext.Cross(a, c))
                    where across >= 0 && across <= 1
                    let start = pathOfLineStartPoint.Lerp(across)
                    let end = pathOfLineEndPoint.Lerp(across)
                    let time = point.LerpProjectOnto(new Line(start, end))
                    where time >= 0 && time <= 1
                    orderby time
                    select new Sweep(timeProportion: time, acrossProportion: across);
        }

        public struct Sweep
        {
            public readonly double TimeProportion;
            public readonly double AcrossProportion;
            public Sweep(double timeProportion, double acrossProportion)
            {
                TimeProportion = timeProportion;
                AcrossProportion = acrossProportion;
            }
        }

        ///<summary>
        ///The proportion that, when lerped across the given line, results in the given point.
        ///If the point is not on the line segment, the result is the closest point on the extended line.
        ///</summary>
        public static double LerpProjectOnto(this Vector2d point, Line line)
        {
            return line.NearestT(point, false);
            /*var b = point - line[0];
            var d = line.Delta;
            return (b * d) / (d * d);*/
        }

        ///<summary>
        ///Enumerates the real solutions to the formula a*x^2 + b*x + c = 0.
        ///Handles degenerate cases.
        ///If a=b=c=0 then only zero is enumerated, even though technically all real numbers are solutions.
        ///</summary>
        public static IEnumerable<double> QuadraticRoots(double a, double b, double c)
        {
            // degenerate? (0x^2 + bx + c == 0)
            if (a == 0)
            {
                // double-degenerate? (0x^2 + 0x + c == 0)
                if (b == 0)
                {
                    // triple-degenerate? (0x^2 + 0x + 0 == 0)
                    if (c == 0)
                    {
                        // every other real number is also a solution, but hopefully one example will be fine
                        yield return 0;
                    }
                    yield break;
                }

                yield return -c / b;
                yield break;
            }

            // ax^2 + bx + c == 0
            // x = (-b +- sqrt(b^2 - 4ac)) / 2a

            var d = b * b - 4 * a * c;
            if (d < 0) yield break; // no real roots

            var s0 = -b / (2 * a);
            var sd = Math.Sqrt(d) / (2 * a);
            yield return s0 - sd;
            if (sd == 0) yield break; // unique root

            yield return s0 + sd;
        }
    }
}