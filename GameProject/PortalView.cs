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
        public List<List<IntPoint>> Paths { get; private set; }
        public List<Vector2> ClipPolygon { get; private set; }
        public List<PortalView> Children { get; private set; }
        public PortalView Parent { get; private set; }
        public Line[] FovLines { get; private set; }
        public Line[] FovLinesPrevious { get; private set; }
        public Line PortalLine { get; private set; }
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

        public PortalView(PortalView parent, Matrix4 viewMatrix, List<IntPoint> path, Line[] fovLines, Line[] fovLinesPrevious)
            : this(parent, viewMatrix, new List<List<IntPoint>>(), fovLines, fovLinesPrevious, null)
        {
            Paths.Add(path);
        }

        public PortalView(PortalView parent, Matrix4 viewMatrix, List<List<IntPoint>> path, Line[] fovLines, Line[] fovLinesPrevious, Line portalLine)
        {
            PortalLine = portalLine;
            FovLines = fovLines;
            FovLinesPrevious = fovLinesPrevious;
            Children = new List<PortalView>();
            Parent = parent;
            if (Parent != null)
            {
                Parent.Children.Add(this);
            }
            ViewMatrix = viewMatrix;
            Paths = path;
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

        public List<PortalView> GetPortalViewList(int maxCount)
        {
            List<PortalView> list = new List<PortalView>();
            Queue<PortalView> queue = new Queue<PortalView>();
            queue.Enqueue(this);
            while (queue.Count != 0)
            {
                if (maxCount <= 0)
                {
                    break;
                }
                PortalView p = queue.Dequeue();
                foreach (PortalView child in p.Children)
                {
                    queue.Enqueue(child);
                }
                list.Add(p);
                maxCount -= 1;
            }
            return list;
        }
    }
}
