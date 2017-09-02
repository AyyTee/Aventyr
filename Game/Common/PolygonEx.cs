using System;
using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics.Collision.Shapes;
using Game.Physics;
using Vector2 = OpenTK.Vector2;
using Xna = Microsoft.Xna.Framework;

namespace Game.Common
{
    public static class PolygonEx
    {
        public static Transform2 GetTransform(IList<Vector2> vertices, IPolygonCoord coord)
        {
            return new Transform2(
                GetEdge(vertices, coord).Lerp(coord.EdgeT),
                -(float)MathEx.VectorToAngleReversed(GetEdge(vertices, coord).GetNormal()),
                1);
        }

        public static Transform2 GetTransform(FixtureCoord fixtureCoord)
        {
            Vector2[] vertices = Vector2Ex.ToOtk(((PolygonShape)fixtureCoord.Fixture.Shape).Vertices);
            return GetTransform(vertices, fixtureCoord);
        }

        public static LineF GetEdge(IList<Vector2> vertices, IPolygonCoord coord)
        {
            DebugEx.Assert(vertices.Count >= 1, "Polygon must have at least 1 vertex.");
            return new LineF(vertices[coord.EdgeIndex], vertices[(coord.EdgeIndex + 1) % vertices.Count]);
        }

        public static float EdgeIndexT(IPolygonCoord coord)
        {
            return coord.EdgeIndex + coord.EdgeT;
        }

        /// <summary>
        /// Returns polygon with the order of the verticies changed so that the normals face outward or inward.
        /// </summary>
        public static List<Vector2> SetNormals(List<Vector2> polygon, bool faceOutward = false)
        {
            return MathEx.SetWinding(polygon, faceOutward);
        }

        /// <summary>
        /// Returns polygon with the order of the verticies changed so that the normals face outward or inward.
        /// </summary>
        public static Vector2[] SetNormals(Vector2[] polygon, bool faceOutward = false)
        {
            return MathEx.SetWinding(polygon, faceOutward);
        }

        /// <summary>
        /// Changes handedness so polygon edge normals face outward.
        /// </summary>
        public static IList<Xna.Vector2> SetInterior(IList<Xna.Vector2> polygon)
        {
            return MathEx.SetWinding(polygon, false);
        }

        public static bool IsInterior(IList<Vector2> polygon)
        {
            return !MathEx.IsClockwise(polygon);
        }

        /// <summary>
        /// Returns whether a polygon is self intersecting.
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static bool IsSimple(IList<Vector2> polygon)
        {
            //Using the really slow but easy to implement O(n^2) algorithm for now.
            for (int i = 0; i < polygon.Count; i++)
            {
                LineF line = new LineF(polygon[i], polygon[(i + 1) % polygon.Count]);
                for (int j = 0; j < polygon.Count; j++)
                {
                    int jNext = (j + 1) % polygon.Count;
                    int jPrev = (j - 1 + polygon.Count) % polygon.Count;
                    if (j == i || jNext == i || jPrev == i)
                    {
                        continue;
                    }
                    var intersect = MathEx.LineLineIntersect(line, new LineF(polygon[j], polygon[jNext]), true);
                    if (intersect != null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the center of mass of a polygon.
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetCentroid(IList<Vector2> polygon)
        {
            Vector2 centroid = new Vector2();
            double atmp = 0;
            double xtmp = 0;
            double ytmp = 0;
            int j = polygon.Count;
            for (int i = 0; i < j; i += 1)
            {
                int iNext = (i + 1) % j;
                double x1 = polygon[i].X;
                double y1 = polygon[i].Y;
                double x2 = polygon[iNext].X;
                double y2 = polygon[iNext].Y;
                double ai = x1 * y2 - x2 * y1;
                atmp += ai;
                xtmp += (x2 + x1) * ai;
                ytmp += (y2 + y1) * ai;
            }
            if (atmp != 0)
            {
                atmp *= 3;
                centroid.X = (float)(xtmp / atmp);
                centroid.Y = (float)(ytmp / atmp);
            }
            return centroid;
        }

        class ConvexVert
        {
            public Vector2 V;
            public HashSet<ConvexVert> Diagonals = new HashSet<ConvexVert>();

            public ConvexVert(Vector2 v)
            {
                V = v;
            }
        }

        static List<List<Vector2>> DecomposeConcave(List<ConvexVert> polygon, bool isClockwise)
        {
            List<List<Vector2>> concaveList = new List<List<Vector2>>();
            List<Vector2> vertices = new List<Vector2>();
            for (int i = 0; i < polygon.Count; i++)
            {
                vertices.Add(polygon[i].V);
            }

            if (MathEx.IsConvex(vertices))
            {
                concaveList.Add(vertices);
                return concaveList;
            }

            for (int i = 0; i < polygon.Count; i++)
            {
                ConvexVert point = polygon[i];

                if (point.Diagonals.Count == 0)
                {
                    continue;
                }

                ConvexVert pointNext = polygon[(i + 1) % polygon.Count];
                ConvexVert pointPrev = polygon[(i + polygon.Count - 1) % polygon.Count];

                ConvexVert diagonal = null;
                int diagonalIndex = -1;
                foreach (ConvexVert vert in point.Diagonals)
                {
                    diagonalIndex = polygon.FindIndex(item => item == vert);
                    //Check that this diagonal exists in the current polygon and that it isn't the next or previous vertice.
                    if (diagonalIndex != -1 && Math.Abs(MathEx.ValueDiff(diagonalIndex, i, polygon.Count)) > 1)
                    {
                        diagonal = vert;
                        break;
                    }
                }

                if (diagonal == null)
                {
                    continue;
                }

                DebugEx.Assert(diagonalIndex != i);

                if (IsReflex(pointPrev.V, point.V, pointNext.V, isClockwise))
                {
                    List<ConvexVert> subpolygon0 = new List<ConvexVert>();
                    List<ConvexVert> subpolygon1 = new List<ConvexVert>();

                    int indexHigh = diagonalIndex < i ? i : diagonalIndex;
                    int indexLow = diagonalIndex > i ? i : diagonalIndex;

                    subpolygon0.AddRange(polygon.GetRange(indexLow, indexHigh - indexLow + 1));
                    subpolygon1.AddRange(polygon.GetRange(0, indexLow + 1));
                    subpolygon1.AddRange(polygon.GetRange(indexHigh, polygon.Count - indexHigh));

                    DebugEx.Assert(subpolygon0.Count >= 3);
                    DebugEx.Assert(subpolygon1.Count >= 3);
                    concaveList.AddRange(DecomposeConcave(subpolygon0, isClockwise));
                    concaveList.AddRange(DecomposeConcave(subpolygon1, isClockwise));
                    return concaveList;
                }
            }

            DebugEx.Assert(false, "Execution should not have reached this point.");
            return concaveList;
        }

        static bool IsReflex(Vector2 prev, Vector2 current, Vector2 next, bool isClockwise)
        {
            double angleNext = MathEx.VectorToAngleReversed(next - current);
            double anglePrev = MathEx.VectorToAngleReversed(prev - current);
            double angleDiff = MathEx.AngleDiff(anglePrev, angleNext);
            return Math.Sign(angleDiff) < 0 != isClockwise;
        }

        /// <summary>
        /// Decomposes a simple polygon into a list of concave polygons.  Winding order is preserved.
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static List<List<Vector2>> DecomposeConcave(IList<Vector2> polygon)
        {
            DebugEx.Assert(polygon.Count >= 3);
            if (MathEx.IsConvex(polygon))
            {
                List<List<Vector2>> concave = new List<List<Vector2>>();
                concave.Add(new List<Vector2>(polygon));
                return concave;
            }

            Poly2Tri.Polygon tris = PolygonFactory.CreatePolygon(polygon);

            List<ConvexVert> verts = new List<ConvexVert>();
            for (int i = 0; i < tris.Points.Count; i++)
            {
                verts.Add(new ConvexVert(Vector2Ex.ToOtk(tris.Points[i])));
            }

            for (int i = 0; i < tris.Triangles.Count; i++)
            {
                var tri = tris.Triangles[i];

                for (int j = 0; j < 3; j++)
                {
                    if (!tri.EdgeIsConstrained[(j + 2) % 3])
                    {
                        int jNext = (j + 1) % 3;
                        var v0 = Vector2Ex.ToOtk(tri.Points[j]);
                        var v1 = Vector2Ex.ToOtk(tri.Points[jNext]);
                        ConvexVert vert0 = verts.Find(item => item.V == v0);
                        ConvexVert vert1 = verts.Find(item => item.V == v1);

                        int vertIndex0 = verts.FindIndex(item => item == vert0);
                        int vertIndex1 = verts.FindIndex(item => item == vert1);
                        DebugEx.Assert(Math.Abs(MathEx.ValueDiff(vertIndex0, vertIndex1, verts.Count)) > 1);

                        vert0.Diagonals.Add(vert1);
                        vert1.Diagonals.Add(vert0);
                    }
                }
            }

            return DecomposeConcave(verts, MathEx.IsClockwise(polygon));
        }
    }
}
