using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class PortalSegments
    {
        private List<Portal> Portals = new List<Portal>();
        private LineSegments Lines = new LineSegments();
        public PortalSegments()
        {
        }
        public void Add(Vector2d Start, Vector2d End, Portal P)
        {
            Lines.Add(Start, End);
            Portals.Add(P);
        }
        public void AddRange(PortalSegments PS)
        {
            Lines.AddRange(PS.Lines);
            Portals.AddRange(PS.Portals);
        }
        public Portal GetPortal(int Index)
        {
            return Portals[Index];
        }
        public Portal GetPortalLast()
        {
            if (Portals.Count() >= 2)
            {
                return Portals[Portals.Count() - 2];
            }
            return null;
        }
        public LineSegments GetLines()
        {
            return Lines;
        }
    }
}
