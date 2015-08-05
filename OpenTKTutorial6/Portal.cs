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
    class Portal : Entity
    {
        private PortalPair PortalPair;
        private bool IsPortal0;
        bool FacingUp;
        double Size = 40;
        public Portal(bool FacingUp) : base (new Vector3())
        {
            this.FacingUp = FacingUp;
        }
        public bool GetIsPortal0()
        {
            return IsPortal0;
        }
        public void SetIsPortal0(bool IsPortal0)
        {
            this.IsPortal0 = IsPortal0;
        }
        //public Vector2d GetPosition2D
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
        public bool IsFacingUp()
        {
            return FacingUp;
        }
    }
}
