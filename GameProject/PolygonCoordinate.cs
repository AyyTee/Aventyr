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
        /*private PolygonOld Polygon;
        public ushort Index;
        public double SegmentT;
        public PolygonCoordinate(PolygonOld Polygon, ushort Index, double SegmentT, bool Absolute)
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
        }*/
    }
}
