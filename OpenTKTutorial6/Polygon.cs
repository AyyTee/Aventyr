using OpenTK;
using OpenTK.Graphics.OpenGL;
using Poly2Tri;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class Polygon
    {
        private Orientation Orient;
        private Exception CIRCLE_DOES_NOT_EXIST = new Exception("CIRCLE_DOES_NOT_EXIST");
        public const int GEOMETRY_NO_PARENT = -1;
        private const double TWO_PI = 2 * Math.PI;
        private double DetailThreshold = .5;
        private List <List<Segment>> Geometry = new List <List<Segment>>();
        public List<Poly2Tri.Polygon> GeometryTriangles = new List<Poly2Tri.Polygon>();
        private List <bool> IsExterior = new List<bool>();
        private List<int> ParentGeometry = new List<int>();
        //VBO Buffer;
        public List <List<Vector2d>> GeometryVertices = new List <List<Vector2d>>();
        public class Segment {
            public Vector2d Position;
            public double Offset;
            public Segment(Vector2d Position, double Offset)
            {
                this.Position = Position;
                this.Offset = Offset;
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
            DetailThreshold = Detail;
            Orient = new Orientation(new Vector2d(0, 0));
        }
        /// <summary>
        /// Adds a vertex. Vertices must be ordered C.Clockwise.
        /// </summary>
        /// <param name="Index"></param>
        /// <param name="Position"></param>
        /// <param name="IsCircle"></param>
        /// <param name="Radius"></param>
        public void AddSegment(int Index, Vector2d Position, double Offset)
        {
            Geometry[Index].Add(new Segment(Position, Offset));
        }
        public void AddSegment(int Index, Vector2d[] Position, double[] Offset)
        {
            for (int i = 0; i < Position.Length; i++)
            {
                AddSegment(Index, Position[i], Offset[i]);
            }
        }
        public void AddSegment(int Index, Vector2d[] Position, double Offset)
        {
            for (int i = 0; i < Position.Length; i++)
            {
                AddSegment(Index, Position[i], Offset);
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
        /// <summary>
        /// Assumes all segments to not contain circles
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
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
        public void BufferVertices(BufferUsageHint Hint)
        {
            GeometryTriangles = new List<Poly2Tri.Polygon>();
            for (int i = 0; i < Geometry.Count(); i++)
            {
                GeometryVertices[i] = GetGeometryVertices(i);
            }
            List<int> Exteriors = new List<int>();
            for (int i = 0; i < Geometry.Count(); i++)
            {
                if (GetIsExterior(i) == true)
                {
                    Exteriors.Add(i);
                }
            }
            //Buffer = new VBO(Exteriors.Count(), PrimitiveType.Triangles);
            Tesselate(Exteriors);
            for (int i = 0; i < GeometryTriangles.Count(); i++)
            {
                //Buffer.AddVertexArray(i, GeometryTriangles[i].Triangles, 1, Hint);
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
                    /*if (Pos.X == PosNext.X)
                    {
                        if (Pos.X > Vector.X) { inside = !inside; }
                    }
                    else
                    {
                        m = (PosNext.Y - Pos.Y) / (PosNext.X - Pos.X);
                        b = Pos.Y - m * Pos.X;
                        ix = (Vector.Y - b) / m;
                        if (ix > Vector.X) { inside = !inside; }
                    }*/
                }
            }
            /*Segment Seg, SegPrev;
            Vector2d C0;
            bool Above;
            double R;
            for (int i = 0; i < Geometry[Index].Count(); i++)
            {
                Seg = GetSegment(Index, i);
                if (Seg.Offset != 0)
                {
                    C0 = GetCircleOrigin(Index, i);
                    R = (Seg.Position - C0).Length;
                    if ((Vector - C0).Length <= R)
                    {
                        SegPrev = GetSegmentPrevious(Index, i);
                        Above = MathExt.PointAboveLine(Seg.Position, SegPrev.Position, Vector);
                        if (Seg.Offset > 0 && inside == GetIsExterior(Index) && Above == false)
                        {
                            inside = !inside;
                        }
                        else if (Seg.Offset < 0 && inside != GetIsExterior(Index) && Above == true)
                        {
                            inside = !inside;
                        }
                    }
                }
            }*/
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
            if (GetSegmentOffset(Index, SegmentIndex) != 0)
            {
                Vector2d Center = GetCircleOrigin(Index, SegmentIndex);
                Vector2d C = Seg.Position - Center;
                double Radius = C.Length;
                int Detail;
                Matrix2d Rot;
                double Diff;
                double[] Dir = GetCircleStartEnd(Index, SegmentIndex);
                Diff = Dir[0] - Dir[1]; 
                if (Radius > DetailThreshold)
                {
                    double a = Math.Acos((Radius - DetailThreshold) / Radius);
                    Detail = (int)Math.Abs(Diff / a) + 1;
                }
                else
                {
                    Detail = 1;
                }
                Rot = Matrix2d.CreateRotation(Diff / (double) Detail);
                Vectors = new Vector2d[Detail];
                Vectors[0] = Seg.Position;
                for (int k = 1; k < Detail; k++)
                {
                    C = MathExt.Matrix2dMult(C, Rot);
                    Vectors[k] = Center + C;
                }
            }
            else
            {
                Vectors = new Vector2d[1] {Seg.Position};
            }
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
            if (Seg.Offset == 0)
            {
                return GetSegmentNormal(Index, SegmentIndex);
            }
            else
            {
                Vector2d Pos = GetGeometryPosition(Index, SegmentT);
                Vector2d Center = GetCircleOrigin(Index, SegmentIndex);
                if (GetIsExterior(Index) == (Seg.Offset < 0))
                {
                    return (Pos - Center).Normalized();
                }
                else
                {
                    return (Center - Pos).Normalized();
                }
            }
        }
        public double GetSegmentOffset(int Index, int SegmentIndex)
        {
            double Offset = GetSegment(Index, SegmentIndex).Offset;
            if (GetIsExterior(Index))
            {
                return Offset;
            }
            return -Offset;
        }
        public void SetSegmentOffset(int Index, int SegmentIndex, double Offset)
        {
            GetSegment(Index, SegmentIndex).Offset = Offset;
        }
        public void AddSegmentOffset(int Index, int SegmentIndex, double Offset)
        {
            GetSegment(Index, SegmentIndex).Offset += Offset;
        }
        public Vector2d GetCircleOrigin(int Index, int SegmentIndex)
        {
            Segment Seg = GetSegment(Index, SegmentIndex);
            //Debug.Assert(Seg.Offset == 0, "Circle does not exist.");
            Segment SegNext = GetSegmentNext(Index, SegmentIndex);
            
            Vector2d V0 = (Seg.Position + SegNext.Position)/2;
            Vector2d N = GetSegmentNormal(Index, SegmentIndex) * GetSegmentLength(Index, SegmentIndex, false);
            Vector2d V1 = V0 + N;
            Vector2d V2 = V0 - N * GetSegmentOffset(Index, SegmentIndex);
            Vector2d V3 = (V2 + SegNext.Position) / 2;
            Vector2d V4 = V3 + (V2 - SegNext.Position).PerpendicularLeft;
            return MathExt.LineIntersection(V0, V1, V3, V4, false).Vector;
        }
        private double[] GetCircleStartEnd(int Index, int SegmentIndex)
        {
            Segment Seg = GetSegment(Index, SegmentIndex);
            Segment SegNext = GetSegmentNext(Index, SegmentIndex);
            Vector2d Origin = GetCircleOrigin(Index, SegmentIndex);
            double Dir0 = MathExt.AngleLine(Seg.Position, Origin);
            double Dir1 = MathExt.AngleLine(SegNext.Position, Origin);
            
            if ((GetSegmentOffset(Index, SegmentIndex) > 0) == GetIsExterior(Index))
            {
                if (Dir1 < Dir0)
                {
                    Dir1 += TWO_PI;
                }
            }
            else
            {
                if (Dir1 > Dir0)
                {
                    Dir1 -= TWO_PI;
                }
            }
            return new double[] { Dir0, Dir1 };
        }
        public double GetGeometryPerimeter(int Index)
        {
            return GetGeometryPerimeter(Index, GetSegmentCount(Index));
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
            /*int SegmentIndex;
            double T;
            if (Absolute == true)
            {
                double Pos = 0;
                double Length = 0;
                int i = 0;
                do
                {
                    Length = GetSegmentLength(Index, i);
                    Pos += Length;
                    i++;
                } while (Pos <= SegmentT);
                SegmentIndex = (int) (i - 1);
                T = 1 - (Pos - SegmentT) / Length;
            }
            else
            {
                SegmentIndex = (int)SegmentT;
                T = SegmentT % 1;
            }
            */
            int SegmentIndex = (int)SegmentT;
            double T = SegmentT % 1;
            Segment SegNext = GetSegmentNext(Index, (int) SegmentIndex);
            Segment Seg = GetSegment(Index, (int) SegmentIndex);
            if (GetSegmentOffset(Index, SegmentIndex) == 0)
            {
                return MathExt.Lerp(Seg.Position, SegNext.Position, T);
            }
            Vector2d Origin = GetCircleOrigin(Index, SegmentIndex);
            double Radius = (Seg.Position - Origin).Length;
            double[] Dir = GetCircleStartEnd(Index, SegmentIndex);
            double DirT = MathExt.Lerp(Dir[0], Dir[1], T);
            return Origin + new Vector2d(Radius * Math.Sin(DirT), Radius * Math.Cos(DirT));
        }
        public double GetSegmentLength(int Index, int SegmentIndex)
        {
            return GetSegmentLength(Index, SegmentIndex, true);
        }
        /// <summary>
        /// If precise is true then then arc circumference is returned rather than segment distance.
        /// </summary>
        /// <param name="Index"></param>
        /// <param name="SegmentIndex"></param>
        /// <param name="Precise"></param>
        /// <returns></returns>
        public double GetSegmentLength(int Index, int SegmentIndex, bool Precise)
        {
            Segment Seg = GetSegment(Index, SegmentIndex);
            Segment SegNext = GetSegmentNext(Index, SegmentIndex);
            if (GetSegmentOffset(Index, SegmentIndex) == 0 || Precise == false)
            {
                return (Seg.Position - SegNext.Position).Length;
            }
            else
            {
                Vector2d Origin = GetCircleOrigin(Index, SegmentIndex);
                double[] Dir = GetCircleStartEnd(Index, SegmentIndex);
                return Math.Abs(Dir[1] - Dir[0]) * (Seg.Position - Origin).Length;
            }
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
                //if (GetSegmentOffset(Index, i) == 0)
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
                /*else
                {

                }*/
            }
            return Intersects;
        }
        public void Draw()
        {
            Matrix4d M = Orient.GetTransform();
            GL.MultMatrix(ref M);
            /*if (Buffer != null)
            {
                Buffer.Draw();
            }*/
            GL.PopMatrix();
        }
        public void DrawDebug()
        {
            GL.PushMatrix();
            Matrix4d M = Orient.GetTransform();
            GL.MultMatrix(ref M);
            /*if (Buffer != null)
            {
                Buffer.DrawWireFrame();
            }*/
            for (int i = 0; i < Geometry.Count(); i++)
            {
                for (int j = 0; j < Geometry[i].Count(); j++)
                {
                    if (GetSegmentOffset(i, j) == 0)
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
            GL.PopMatrix();
        }
    }
}
