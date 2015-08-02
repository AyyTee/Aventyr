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
    class Actor
    {
        private Orientation Orient;
        private Vector2d Speed;
        private int DrawRadius;
        public Actor(Vector2d Position)
        {
            Orient = new Orientation(Position);
            Speed = new Vector2d(0, 0);
        }
        public Actor(Vector2d Position, Vector2d Speed)
        {
            Orient = new Orientation(Position);
            this.Speed = Speed;
        }
        public double GetScale()
        {
            return Orient.Scale;
        }
        public void SetScale(double Scale)
        {
            Orient.Scale = Scale;
        }
        public Vector2d GetSpeed()
        {
            return Speed;
        }
        public void SetSpeed(Vector2d Speed)
        {
            this.Speed = Speed;
        }
        public void SetPosition(Vector2d Position)
        {
            Orient.Position = Position;
        }
        public Vector2d GetPosition()
        {
            return Orient.Position;
        }
        public double GetRotation()
        {
            return Orient.Rotation;
        }
        public void SetRotation(double Angle)
        {
            Orient.Rotation = Angle;
        }
        public bool GetMirrored()
        {
            return Orient.Mirrored;
        }
        public void SetMirrored(bool Mirrored)
        {
            Orient.Mirrored = Mirrored;
        }
        public void PortalEnter(Portal P)
        {
            Matrix4d M = P.GetPortalPair().GetTransform(P.GetIsPortal0(), true);
            Vector3d V2 = Vector3d.Transform(new Vector3d(GetSpeed()), M);
            SetSpeed(new Vector2d(V2.X, V2.Y));
            SetScale(GetScale() * P.GetPortalPair().GetScale(P.GetIsPortal0()));
            bool Mirrored = P.GetPortalPair().GetMirrored();
            if (Mirrored != Orient.Mirrored)
            {
                if (Orient.Mirrored)
                {
                    SetMirrored(false);
                }
                else
                {
                    SetMirrored(true);
                }
            }
            double Dir = P.GetPortalPair().GetRotation(P.GetIsPortal0());
            SetRotation(GetRotation() + Dir);
        }
        public void Draw()
        {
            GL.PushMatrix();
            Matrix4d M = Orient.GetTransform();
            GL.MultMatrix(ref M);
            Vector2d V0 = new Vector2d(0, 0);
            Vector2d V1 = new Vector2d(10, 0);
            Vector2d V2 = new Vector2d(0, -10);
            GL.Begin(PrimitiveType.Triangles);
            GL.Color3(Color.MidnightBlue);
            GL.Vertex2(V0);
            GL.Color3(Color.SpringGreen);
            GL.Vertex2(V1);
            GL.Color3(Color.Ivory);
            GL.Vertex2(V2);
            GL.End();
            GL.PopMatrix();
        }
    }
}