using ClipperLib;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class PortalView
    {
        public Matrix4 ViewMatrix { get; private set; }
        public Vector2[] Path { get; private set; }
        public List<Vector2> ClipPolygon { get; private set; }
        public List<PortalView> Children { get; private set; }
        public PortalView Parent { get; private set; }
        public Portal Portal { get; private set; }
        public Line[] FovLines { get; private set; }
        public int Count
        {
            get
            {
                int total = 1;
                foreach (PortalView p in Children)
                {
                    total += p.Count;
                }
                return total;
            }
        }

        public PortalView(PortalView parent, Matrix4 viewMatrix, Vector2[] path, Line[] fovLines, Portal portal)
        {
            Portal = portal;
            FovLines = fovLines;
            Children = new List<PortalView>();
            Parent = parent;
            if (Parent != null)
            {
                Parent.Children.Add(this);
            }
            ViewMatrix = viewMatrix;
            Path = path;
        }

        public List<Line> GetClipLines()
        {
            List<Line> clipLines = new List<Line>();
            Vector2[] vList = Path;//ClipperExt.ConvertToVector2(Path);
            for (int i = 0; i < Path.Length; i++)
            {
                int iNext = (i + 1) % Path.Length;
                Vector2 v0 = Path[i];
                Vector2 v1 = Path[iNext];
                clipLines.Add(new Line(v0, v1));
            }
            if (!MathExt.IsClockwise(vList))
            {
                foreach(Line l in clipLines)
                {
                    l.Reverse();
                }
            }
            return clipLines;
        }

        public List<PortalView> GetPortalViewList()
        {
            List<PortalView> list = new List<PortalView>();
            list.Add(this);
            foreach (PortalView p in Children)
            {
                list.AddRange(p.GetPortalViewList());
            }
            return list;
        }
    }
}
