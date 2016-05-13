using ClipperLib;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class PortalView
    {
        public Matrix4 ViewMatrix { get; private set; }
        public List<List<IntPoint>> Paths { get; private set; }
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

        public static PortalView CalculatePortalViews(IPortal[] portals, ICamera2 camera, int depth)
        {
            Debug.Assert(camera != null);
            Debug.Assert(depth >= 0);
            Debug.Assert(portals != null);
            List<IntPoint> view = ClipperConvert.ToIntPoint(CameraExt.GetWorldVerts(camera));
            List<List<IntPoint>> paths = new List<List<IntPoint>>();
            paths.Add(view);
            PortalView portalView = new PortalView(null, camera.GetViewMatrix(), view, new Line[0], new Line[0]);
            Vector2 camPos = camera.GetWorldTransform().Position;
            CalculatePortalViews(null, portals, camera.GetViewMatrix(), camPos, camPos - camera.GetWorldVelocity().Position, depth, portalView, Matrix4.Identity);
            return portalView;
        }

        private static void CalculatePortalViews(IPortal portalEnter, IPortal[] portals, Matrix4 viewMatrix, Vector2 viewPos, Vector2 viewPosPrevious, int depth, PortalView portalView, Matrix4 portalMatrix)
        {
            if (depth <= 0)
            {
                return;
            }
            const float AREA_EPSILON = 0.0001f;
            Clipper c = new Clipper();
            //The clipper must be set to strictly simple. Otherwise polygons might have duplicate vertices which causes poly2tri to generate incorrect results.
            c.StrictlySimple = true;
            foreach (IPortal p in portals)
            {
                if (!_isPortalValid(portalEnter, p, viewPos))
                {
                    continue;
                }

                Vector2[] fov = Vector2Ext.Transform(Portal.GetFov(p, viewPos, 500, 3), portalMatrix);
                if (MathExt.GetArea(fov) < AREA_EPSILON)
                {
                    continue;
                }
                List<IntPoint> pathFov = ClipperConvert.ToIntPoint(fov);

                var viewNew = new List<List<IntPoint>>();

                c.AddPath(pathFov, PolyType.ptSubject, true);
                c.AddPaths(portalView.Paths, PolyType.ptClip, true);
                c.Execute(ClipType.ctIntersection, viewNew);
                c.Clear();

                if (viewNew.Count <= 0)
                {
                    continue;
                }
                c.AddPaths(viewNew, PolyType.ptSubject, true);
                foreach (IPortal other in portals)
                {
                    if (other == p)
                    {
                        continue;
                    }
                    if (!_isPortalValid(portalEnter, other, viewPos))
                    {
                        continue;
                    }
                    //Skip this portal if it's inside the current portal's FOV.
                    Line portalLine = new Line(Portal.GetWorldVerts(p));
                    Line portalOtherLine = new Line(Portal.GetWorldVerts(other));
                    if (portalLine.IsInsideFOV(viewPos, portalOtherLine))
                    {
                        continue;
                    }
                    Vector2[] otherFov = Vector2Ext.Transform(Portal.GetFov(other, viewPos, 500, 3), portalMatrix);
                    if (MathExt.GetArea(otherFov) < AREA_EPSILON)
                    {
                        continue;
                    }
                    MathExt.SetHandedness(otherFov, true);
                    List<IntPoint> otherPathFov = ClipperConvert.ToIntPoint(otherFov);
                    c.AddPath(otherPathFov, PolyType.ptClip, true);
                }
                var viewNewer = new List<List<IntPoint>>();
                c.Execute(ClipType.ctDifference, viewNewer, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
                c.Clear();
                if (viewNewer.Count <= 0)
                {
                    continue;
                }

                Vector2 viewPosNew = Vector2Ext.Transform(viewPos, Portal.GetPortalMatrix(p, p.Linked));
                Vector2 viewPosPreviousNew = Vector2Ext.Transform(viewPosPrevious, Portal.GetPortalMatrix(p, p.Linked));

                Matrix4 portalMatrixNew = Portal.GetPortalMatrix(p.Linked, p) * portalMatrix;
                Matrix4 viewMatrixNew = portalMatrixNew * viewMatrix;

                Line[] lines = Portal.GetFovLines(p, viewPos, 500);
                lines[0] = lines[0].Transform(portalMatrix);
                lines[1] = lines[1].Transform(portalMatrix);
                Line[] linesPrevious = Portal.GetFovLines(p, viewPosPrevious, 500);
                linesPrevious[0] = linesPrevious[0].Transform(portalMatrix);
                linesPrevious[1] = linesPrevious[1].Transform(portalMatrix);

                Line portalWorldLine = new Line(Portal.GetWorldVerts(p));
                portalWorldLine = portalWorldLine.Transform(portalMatrix);
                PortalView portalViewNew = new PortalView(portalView, viewMatrixNew, viewNewer, lines, linesPrevious, portalWorldLine);

                CalculatePortalViews(p, portals, viewMatrix, viewPosNew, viewPosPreviousNew, depth - 1, portalViewNew, portalMatrixNew);
            }
        }

        private static bool _isPortalValid(IPortal previous, IPortal next, Vector2 viewPos)
        {
            //skip this portal if it isn't linked 
            if (!Portal.IsValid(next))
            {
                return false;
            }
            //or it's the exit portal
            if (previous != null && next == previous.Linked)
            {
                return false;
            }
            //or if the portal is one sided and the view point is on the wrong side
            Vector2[] pv2 = Portal.GetWorldVerts(next);
            Line portalLine = new Line(pv2);
            if (next.OneSided)
            {
                if (portalLine.GetSideOf(pv2[0] + next.GetWorldTransform().GetRight()) != portalLine.GetSideOf(viewPos))
                {
                    return false;
                }
            }
            //or if this portal isn't inside the fov of the exit portal
            if (previous != null)
            {
                Line portalEnterLine = new Line(Portal.GetWorldVerts(previous.Linked));
                if (!portalEnterLine.IsInsideFOV(viewPos, portalLine))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
