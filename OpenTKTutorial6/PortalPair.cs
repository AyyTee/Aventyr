using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class PortalPair
    {
        private Portal[] _portals = new Portal[2];
        public Portal[] Portals 
        { 
            get { return _portals; } 
            set 
            {
                Debug.Assert(Portals[0] != Portals[1], "The same Portal instance cannot be assigned to both array indices.");
                _portals = value; 
            } 
        }

        public PortalPair()
        {
        }

        public PortalPair(Portal portal0, Portal portal1)
        {
            Portals[0] = portal0;
            Portals[1] = portal1;
        }
    }
}
