using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class PortalPair
    {
        //private VBO FOVBuffer = new VBO(2, PrimitiveType.TriangleFan);
        private Portal[] Portals = new Portal[2];
        public PortalPair()
        {
        }
        public PortalPair(Portal Portal0)
        {
            SetPortal0(Portal0);
        }
        public PortalPair(Portal Portal0, Portal Portal1)
        {
            SetPortal0(Portal0);
            SetPortal1(Portal1);
        }
        public void SetPortals(Portal Portal0, Portal Portal1)
        {
            SetPortal0(Portal0);
            SetPortal1(Portal1);
        }
        public void SetPortals(Portal[] Portals)
        {
            SetPortal0(Portals[0]);
            SetPortal1(Portals[1]);
        }
        public void SetPortal0(Portal Portal0)
        {
            Portals[0] = Portal0;
            Portal0.SetPortalPair(this);
            Portal0.SetIsPortal0(true);
        }
        public void SetPortal1(Portal Portal1)
        {
            Portals[1] = Portal1;
            Portal1.SetPortalPair(this);
            Portal1.SetIsPortal0(false);
        }
        public Portal[] GetPortals()
        {
            return Portals;
        }
        public Portal GetPortal0()
        {
            return Portals[0];
        }
        public Portal GetPortal1()
        {
            return Portals[1];
        }
        public double GetScale(bool Portal0)
        {
            if (Portal0)
            {
                return GetPortal1().GetSize() / GetPortal0().GetSize();
            }
            return GetPortal0().GetSize() / GetPortal1().GetSize();
        }
        public Vector2d GetTranslation(bool Portal0)
        {
            if (Portal0)
            {
                return GetPortal1().GetPosition() - GetPortal0().GetPosition();
            }
            return GetPortal0().GetPosition() - GetPortal1().GetPosition();
        }
        public double GetRotation(bool Portal0)
        {
            Vector2d V0, V1;
            double Dir;
            if (GetMirrored() == false)
            {
                V0 = GetPortal0().GetFacing();
                V1 = GetPortal1().GetFacing();
                Dir = Math.Atan2(V1.Y, V1.X) - Math.Atan2(V0.Y, V0.X);
            }
            else
            {
                V0 = GetPortal0().GetFacing();
                V1 = GetPortal1().GetFacing();
                Dir = -Math.Atan2(V1.Y, V1.X) - Math.Atan2(V0.Y, V0.X) + Math.PI;
            }
            if (Portal0)
            {
                return Dir;
            }
            return -Dir;
            
        }
        public bool GetMirrored()
        {
            return GetPortal0().IsFacingUp() == GetPortal1().IsFacingUp();
        }
        public Matrix4d GetTransform(bool Portal0)
        {
            return GetTransform(Portal0, false);
        }
        public Matrix4d GetTransform(bool Portal0, bool NoTranslate)
        {
            Matrix4d Matrix = Matrix4d.Identity;
            Vector3d V0 = new Vector3d();
            if (NoTranslate == false)
            {
                V0 = new Vector3d(GetPortal0().GetPosition());
                Matrix *= Matrix4d.CreateTranslation(-V0);
            }
            Matrix *= Matrix4d.CreateRotationZ(GetRotation(true));
            double S = GetScale(true);
            if (GetMirrored())
            {
                Matrix *= Matrix4d.Scale(-S, S, S);
            }
            else
            {
                Matrix *= Matrix4d.Scale(S);
            }
            if (NoTranslate == false)
            {
                Matrix *= Matrix4d.CreateTranslation(V0 + new Vector3d(GetTranslation(true)));
            }
            if (Portal0)
            {
                return Matrix;
            }
            return Matrix.Inverted();
        }
        public void UpdateFOV(Vector2 position, double Distance, bool Portal0)
        {
            /*Portal Portal;
            if (Portal0)
            {
                Portal = GetPortal0();
            }
            else
            {
                Portal = GetPortal1();
            }
            Vector2d Pos = Portal.GetPosition();
            Vector2d Facing = Portal.GetFacing();
            int Detail = 6;
            Vector2d[] V = new Vector2d[2 + Detail];
            double Width = Portal.GetSize()/2;
            V[0] = Pos - Vector2d.Multiply(Facing, Width);
            V[1] = Pos + Vector2d.Multiply(Facing, Width);
            double L0 = Distance - (V[0] - position).Length;
            double L1 = Distance - (V[1] - position).Length;
            if (L0 > 0 && L1 > 0) 
            {
                double L;
                Vector2d N;
                L = Distance - (V[1] - P.POV).Length;
                N = (V[1] - P.POV).Normalized();
                Vector2d C = V[1] + N * L - position;
                double Dir0 = MathExt.AngleLine(V[0], position);
                double Dir1 = MathExt.AngleLine(V[1], position);
                double Diff = Dir1 - Dir0;
                if (Math.Abs(Diff) > Math.PI)
                {
                    if (Dir0 < Dir1)
                    {
                        Dir0 += Math.PI * 2;
                    }
                    else
                    {
                        Dir0 -= Math.PI * 2;
                    }
                    Diff = Dir1 - Dir0;
                }
                
                Matrix2d Rot = Matrix2d.CreateRotation(Diff / (double)(Detail-1));
                for (int i = 0; i < Detail; i++)
                {
                    V[2 + i] = position + C;
                    C = MathExt.Matrix2dMult(C, Rot);
                }
            }*/
        }
    }
}
