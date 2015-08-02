using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Game
{
    class PolygonCoordinate
    {
        private Polygon Polygon;
        public ushort Index;
        public double SegmentT;
        public PolygonCoordinate(Polygon Polygon, ushort Index, double SegmentT, bool Absolute)
        {
            this.Polygon = Polygon;
            this.Index = Index;
            SetSegmentT(SegmentT, Absolute);
        }
        public Vector2d GetNormal() {
            return Polygon.GetSegmentNormal(Index, SegmentT);
        }
        public Vector2d GetPosition() 
        {
            return Polygon.GetGeometryPosition(Index, SegmentT);   
        }
        public void SetSegmentT(double SegmentT, bool Absolute)
        {
            int Count = Polygon.GetSegmentCount(Index);
            if (Absolute == true)
            {
                SegmentT = MathExt.ValueWrap(SegmentT, Polygon.GetGeometryPerimeter(Index));
                double Pos = 0;
                double Length = 0;
                int i = 0;
                do
                {
                    Length = Polygon.GetSegmentLength(Index, i);
                    Pos += Length;
                    i++;
                } while (Pos <= SegmentT);
                this.SegmentT = i - (Pos - SegmentT) / Length;
            }
            else
            {
                SegmentT = MathExt.ValueWrap(SegmentT, Count);
                this.SegmentT = SegmentT;
            }
        }
        public double GetSegmentT(bool Absolute)
        {
            if (Absolute == true)
            {
                return Polygon.GetGeometryPerimeter(Index, SegmentT);
            }
            else
            {
                return SegmentT;
            }
        }
        public void DrawDebug()
        {
            GL.Begin(PrimitiveType.Lines);
            Vector2d V0 = GetPosition();
            GL.Color3(Color.Lime);
            Vector2d V1 = new Vector2d(4, 4);
            Vector2d V2 = new Vector2d(4, -4);
            GL.Vertex2(V0 - V1);
            GL.Vertex2(V0 + V1);
            GL.Vertex2(V0 - V2);
            GL.Vertex2(V0 + V2);
            GL.Color3(Color.White);
            Vector2d V3 = GetNormal() * 10;
            GL.Vertex2(V0);
            GL.Vertex2(V0 + V3);
            GL.End();
        }
    }
}
