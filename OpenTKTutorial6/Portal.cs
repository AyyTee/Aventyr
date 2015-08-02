using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class Portal
    {
        PolygonCoordinate Position;
        private PortalPair PortalPair;
        private bool IsPortal0;
        bool FacingUp;
        double Size = 40;
        public Portal(PolygonCoordinate Position, bool FacingUp)
        {
            this.Position = Position;
            this.FacingUp = FacingUp;
        }
        public Vector2d GetPosition()
        {
            return Position.GetPosition();
        }
        public Vector2d GetNormal()
        {
            return Position.GetNormal();
        }
        public bool GetIsPortal0()
        {
            return IsPortal0;
        }
        public void SetIsPortal0(bool IsPortal0)
        {
            this.IsPortal0 = IsPortal0;
        }
        public double GetSize()
        {
            return Size;
        }
        public void SetSize(double Scale)
        {
            this.Size = Scale;
        }
        public void SetPortalPair(PortalPair PortalPair) 
        {
            this.PortalPair = PortalPair;
        }
        public PortalPair GetPortalPair()
        {
            return PortalPair;
        }
        public Vector2d GetFacing()
        {
            if (FacingUp == true)
            {
                return Position.GetNormal().PerpendicularLeft;
            }
            return Position.GetNormal().PerpendicularRight;
        }
        public bool IsFacingUp()
        {
            return FacingUp;
        }
        public IntersectPoint LineIntersection(Vector2d Start, Vector2d End)
        {
            Vector2d V0 = GetPosition();
            Vector2d F = GetFacing();
            Vector2d V1 = V0 + Vector2d.Multiply(F, GetSize()/2);
            Vector2d V2 = V0 - Vector2d.Multiply(F, GetSize()/2);
            return MathExt.LineIntersection(Start, End, V1, V2, true);
        }
        public void DrawDebug()
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Red);
            Vector2d V0 = GetPosition();
            Vector2d N = GetNormal();
            Vector2d V1 = GetFacing() * GetSize()/2;
            GL.Vertex2(V0 - V1);
            GL.Vertex2(V0 + V1);
            GL.Vertex2(V0 + V1);
            GL.Vertex2(V0 + V1 * .85 + N*3);
            GL.Vertex2(V0);
            GL.Vertex2(V0 + N * 5);
            GL.End();
        }
    }
}
