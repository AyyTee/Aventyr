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
        public List<IntPoint> Path { get; private set; }
        public List<PortalView> Children { get; private set; }
        public PortalView Parent { get; private set; }
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

        public PortalView(PortalView parent, Matrix4 viewMatrix, List<IntPoint> path)
        {
            Children = new List<PortalView>();
            Parent = parent;
            if (Parent != null)
            {
                Parent.Children.Add(this);
            }
            ViewMatrix = viewMatrix;
            Path = path;
        }

        public List<PortalView> GetPortalViewList()
        {
            List<PortalView> list = new List<PortalView>();
            foreach (PortalView p in Children)
            {
                list.Add(p);
                p.GetPortalViewList();
            }
            return list;
        }
    }
}
