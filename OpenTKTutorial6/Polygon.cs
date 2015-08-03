using OpenTK;
using OpenTK.Graphics.OpenGL;
using Poly2Tri;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Game
{
    class Polygon
    {
        private Orientation Orient;
        public const int GEOMETRY_NO_PARENT = -1;
        private List <List<Segment>> Geometry = new List <List<Segment>>();
        public List<Poly2Tri.Polygon> GeometryTriangles = new List<Poly2Tri.Polygon>();
        private List <bool> IsExterior = new List<bool>();
        private List<int> ParentGeometry = new List<int>();
        public List <List<Vector2d>> GeometryVertices = new List <List<Vector2d>>();
        public class Segment {
            public Vector2d Position;
            public Segment(Vector2d Position)
            {
                this.Position = Position;
            }
        }
        public Polygon()
        {
            AddGeometry(GEOMETRY_NO_PARENT);
            Orient = new Orientation(new Vector2d(0, 0));
        }
        public Polygon(double Detail)
        {
            AddGeometry(GEOMETRY_NO_PARENT);
            Orient = new Orientation(new Vector2d(0, 0));
        }
        /// <summary>
        /// Adds a vertex. Vertices must be ordered C.Clockwise.
        /// </summary>
        /// <param name="Index"></param>
        /// <param name="Position"></param>
        /// <param name="IsCircle"></param>
        /// <param name="Radius"></param>
        public void AddSegment(int Index, Vector2d Position)
        {
            Geometry[Index].Add(new Segment(Position));
        }
        public void AddSegment(int Index, Vector2d[] Position)
        {
            for (int i = 0; i < Position.Length; i++)
            {
                AddSegment(Index, Position[i]);
            }
        }
        public void AddGeometry(int ParentGeometry)
        {
            Geometry.Add(new List<Segment>());
            GeometryVertices.Add(new List<Vector2d>());
            this.ParentGeometry.Add(ParentGeometry);
        }
        public List<int> GetGeometryChildren(int Index)
        {
            List<int> ChildList = new List<int>();
            for (int i = 0; i < Geometry.Count(); i++)
            {
                if (ParentGeometry[i] == Index)
                {
                    ChildList.Add(i);
                }
            }
            return ChildList;
        }
        public int GetGeometryCount()
        {
            return Geometry.Count();
        }
        public int GetSegmentCount(int Index)
        {
            return Geometry[Index].Count();
        }
        public bool IsConvex(int Index)
        {
            Vector2d V0, V1;
            V0 = GetSegment(Index, 0).Position;
            V1 = GetSegmentNext(Index, 0).Position;
            bool Above = MathExt.PointAboveLine(new Vector2d(0, 0), V0, V1);
            for (int i = 1; i < GetSegmentCount(Index); i++)
            {
                V0 = GetSegment(Index, i).Position;
                V1 = GetSegmentNext(Index, i).Position;
                if (MathExt.PointAboveLine(new Vector2d(0, 0), V0, V1) != Above)
                {
                    return false;
                }
            }
            return true;
        }
        public void Tesselate(List<int> Exteriors)
        {
            List<Vector2d> Verts0;
            PolygonPoint[] TempVerts;
            for (int i = 0; i < Exteriors.Count(); i++)
            {
                Verts0 = GeometryVertices[Exteriors[i]];
                TempVerts = new PolygonPoint[Verts0.Count()];
                for (int j = 0; j < Verts0.Count(); j++)
                {
                    TempVerts[j] = new PolygonPoint(Verts0[j].X, Verts0[j].Y);
                }
                Poly2Tri.Polygon Poly = new Poly2Tri.Polygon(TempVerts);
                GeometryTriangles.Add(Poly);

                List<int> ChildList = GetGeometryChildren(Exteriors[i]);
                for (int k = 0; k < ChildList.Count(); k++)
                {
                    List<Vector2d> Verts1 = GeometryVertices[ChildList[k]];
                    PolygonPoint[] HoleVerts = new PolygonPoint[Verts1.Count()];
                    for (int j = 0; j < Verts1.Count(); j++)
                    {
                        HoleVerts[j] = new PolygonPoint(Verts1[j].X, Verts1[j].Y);
                    }
                    Poly2Tri.Polygon Hole = new Poly2Tri.Polygon(HoleVerts);
                    Poly.AddHole(Hole);
                }
                P2T.Triangulate(Poly);
            }
        }
        public Segment GetSegment(int Index, int SegmentIndex)
        {
            int a = (SegmentIndex) % Geometry[Index].Count();
            return Geometry[Index][a];
        }
        public Segment GetSegmentPrevious(int Index, int SegmentIndex)
        {
            int a = (SegmentIndex + Geometry[Index].Count() - 1) % Geometry[Index].Count();
            return Geometry[Index][a];
        }
        public Segment GetSegmentNext(int Index, int SegmentIndex)
        {
            int a = (SegmentIndex + 1) % Geometry[Index].Count();
            return Geometry[Index][a];
        } 
        public List<Vector2d> GetGeometryVertices(int Index)
        {
            int Count = Geometry[Index].Count();
            List<Vector2d> Vertices = new List<Vector2d>();
            for (int i = 0; i < Count; i++)
            {
                Vertices.AddRange(GetSegmentVertices(Index, i));
            }
            return Vertices;
        }
        /// <summary>
        /// Returns whether a vector lies inside a specific line loop.  It is assumed that the line loop is not complex.
        /// </summary>
        /// <param name="Vector"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public bool InsidePolygon(Vector2d Vector, int Index)
        {
            List<Vector2d> Vectors = GeometryVertices[Index];
            bool inside = false;
            Vector2d Pos, PosNext;
            GetGeometryVertices(Index);
            int Count = GeometryVertices[Index].Count();
            for (int i = 0; i < Count; i++)
            {
                Pos = GeometryVertices[Index][i];
                PosNext = GeometryVertices[Index][(i + 1) % Count];
                if (((Pos.Y <= Vector.Y) && (PosNext.Y > Vector.Y)) || ((Pos.Y > Vector.Y) && (PosNext.Y <= Vector.Y)))
                {
                    if (MathExt.PointAboveLine(Pos, PosNext, Vector))
                    {
                        inside = !inside;
                    }
                }
            }
            if (GetIsExterior(Index) == false)
            {
                inside = !inside;
            }
            return inside;
        }
        public bool InsideGeometry(Vector2d Vector, int Index)
        {
            if (Geometry.Count() == 0)
            {
                return false;
            }
            int Depth = 0;
            for (int i = 0; i < Geometry.Count(); i++)
            {
                if (ParentGeometry[i] == Index)
                {
                    if (InsidePolygon(Vector, i))
                    {
                        Depth = _InsideGeometry(Vector, i, Depth);
                        break;
                    }
                }
            }
            if (Depth % 2 == 0)
            {
                return false;
            }
            return true;
        }
        private int _InsideGeometry(Vector2d Vector, int Index, int Depth)
        {
            for (int i = 0; i < Geometry.Count(); i++)
            {
                if (ParentGeometry[i] == Index)
                {
                    if (InsidePolygon(Vector, i) == (Depth % 2 == 1))
                    {
                        return _InsideGeometry(Vector, i, Depth + 1);
                    }
                }
            }
            return Depth + 1;
        }
        public bool GetIsExterior(int Index)
        {
            int i = 0;
            do {
                Index = ParentGeometry[Index];
                i++;
            } while (Index > -1);
            if (i % 2 == 0)
            {
                return false;
            }
            return true;
        }
        public Vector2d[] GetSegmentVertices(int Index, int SegmentIndex)
        {
            Segment SegNext = GetSegmentNext(Index, SegmentIndex);
            Segment Seg = GetSegment(Index, SegmentIndex);
            Vector2d[] Vectors;
            Vectors = new Vector2d[1] {Seg.Position};
            return Vectors;
        }
        public Vector2d GetSegmentNormal(int Index, int SegmentIndex)
        {
            Segment SegNext = GetSegmentNext(Index, SegmentIndex);
            Segment Seg = GetSegment(Index, SegmentIndex);
            Vector2d Vector = Seg.Position - SegNext.Position;
            if (GetIsExterior(Index) == true)
            {
                return Vector.PerpendicularLeft.Normalized();
            }
            else
            {
                return Vector.PerpendicularRight.Normalized();
            }
        }
        public Vector2d GetSegmentNormal(int Index, double SegmentT)
        {
            int SegmentIndex = (int) SegmentT;
            Segment Seg = GetSegment(Index, SegmentIndex);
            return GetSegmentNormal(Index, SegmentIndex);
        }
        public double GetGeometryPerimeter(int Index)
        {
            return GetGeometryPerimeter(Index, GetGeometryCount());
        }
        public double GetGeometryPerimeter(int Index, double EndT)
        {
            double Perimeter = 0;
            int SegmentIndex = (int)EndT;
            for (int i = 0; i < SegmentIndex; i++)
            {
                Perimeter += GetSegmentLength(Index, i);
            }
            Perimeter += GetSegmentLength(Index, SegmentIndex) * (EndT % 1);
            return Perimeter;
        }
        public double GetGeometryPerimeter(int Index, double StartT, double EndT)
        {
            //not tested
            double Low = Math.Min(StartT, EndT);
            double High = Math.Max(StartT, EndT);
            int Count = GetSegmentCount(Index);
            double Diff = StartT - EndT;
            Low = MathExt.ValueWrap(Low, Count);
            High = Low + Diff;
            return Math.Abs(GetGeometryPerimeter(Index, StartT) - GetGeometryPerimeter(Index, EndT));
        }
        public double GetPolygonArea(int Index)
        {
            //not tested
            int i,Count;
            double Area = 0;
            Segment Seg, SegNext;
            Count = GetSegmentCount(Index);
            for (i = 0; i < Count; i++) 
            {
                Seg = GetSegment(Index, i);
                SegNext = GetSegmentNext(Index, i);
                Area += Seg.Position.X * SegNext.Position.Y;
                Area -= Seg.Position.Y * SegNext.Position.X;
            }

            Area /= 2;
            return Math.Abs(Area);//(area < 0 ? -area : area);
        }
        public Vector2d GetGeometryPosition(int Index, double SegmentT)
        {
            int SegmentIndex = (int)SegmentT;
            double T = SegmentT % 1;
            Segment SegNext = GetSegmentNext(Index, (int) SegmentIndex);
            Segment Seg = GetSegment(Index, (int) SegmentIndex);
            return MathExt.Lerp(Seg.Position, SegNext.Position, T);
        }
        /// <summary>
        /// If precise is true then then arc circumference is returned rather than segment distance.
        /// </summary>
        /// <param name="Index"></param>
        /// <param name="SegmentIndex"></param>
        /// <param name="Precise"></param>
        /// <returns></returns>
        public double GetSegmentLength(int Index, int SegmentIndex)
        {
            Segment Seg = GetSegment(Index, SegmentIndex);
            Segment SegNext = GetSegmentNext(Index, SegmentIndex);
            return (Seg.Position - SegNext.Position).Length;
        }
        public List<Vector2d> LineIntersection(Vector2d Start, Vector2d End)
        {
            List<Vector2d> Intersects = new List<Vector2d>();
            for (int i = 0; i < Geometry.Count(); i++)
            {
                Intersects.AddRange(LineIntersection(Start, End, i));
            }
            return Intersects;
        }
        public List<Vector2d> LineIntersection(Vector2d Start, Vector2d End, int Index)
        {
            int Count = GetSegmentCount(Index);
            Segment Seg, SegNext;
            List<Vector2d> Intersects = new List<Vector2d>();
            Vector2d Pos;
            for (int i = 0; i < Count; i++)
            {
                Seg = GetSegment(Index, i);
                SegNext = GetSegmentNext(Index, i);
                IntersectPoint V = MathExt.LineIntersection(Start, End, Seg.Position, SegNext.Position, true);
                    
                if (V.Exists == true)
                {
                    Pos = V.Vector;
                    Intersects.Add(Pos);
                }
            }
            return Intersects;
        }
        public void Draw()
        {
        }
        public void DrawDebug()
        {
            for (int i = 0; i < Geometry.Count(); i++)
            {
                for (int j = 0; j < Geometry[i].Count(); j++)
                {
                    GL.Begin(PrimitiveType.Lines);
                    var Pos0 = GetSegment(i, j).Position;
                    var Pos1 = GetSegmentNext(i, j).Position;
                    var Pos2 = (Pos0 + Pos1) / 2;
                    GL.Color3(Color.Lime);
                    GL.Vertex2(Pos2);
                    Vector2d Norm = GetSegmentNormal(i, j) * 5;
                    GL.Vertex2(Pos2 + Norm);
                    GL.Vertex2(Pos1);
                    GL.Vertex2(Pos1 * .8 + Pos0 * .2 + Norm);
                    GL.Vertex2(Pos1 * .8 + Pos0 * .2 + Norm);
                    GL.Vertex2(Pos1 * .8 + Pos0 * .2);
                    GL.End();
                }
            }
        }
    }
}
