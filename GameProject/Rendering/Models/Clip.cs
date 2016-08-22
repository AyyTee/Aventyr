using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class Clip
    {
        const double PORTAL_CLIP_MARGIN = 0.0001;

        public class ClipModel
        {
            readonly Line[] _clipLines;
            public Line[] ClipLines { get { return _clipLines; } }
            readonly Matrix4 _transform;
            public Matrix4 Transform { get { return _transform; } }
            public readonly Model Model;
            public readonly IRenderable Entity;

            public ClipModel(IRenderable entity, Model model, Line[] clipLines, Matrix4 transform)
            {
                Entity = entity;
                Model = model;
                _clipLines = clipLines;
                _transform = transform;
            }
        }

        public class ClipPolygon
        {
            public IList<Line> ClipLines { get; set; }
            public Matrix4 Transform { get; set; }
            public IList<Vector2> Polygon { get; set; }

            public ClipPolygon(IList<Vector2> polygon, IList<Line> clipLines, Matrix4 transform)
            {
                Polygon = polygon;
                ClipLines = clipLines;
                Transform = transform;
            }
        }


        public static List<ClipModel> GetClipModels(IRenderable entity, IList<IPortal> portalList, int depth)
        {
            List<ClipModel> clipModels = new List<ClipModel>();
            if (entity.IsPortalable && !entity.DrawOverPortals)
            {
                foreach (Model m in entity.GetModels())
                {
                    clipModels.AddRange(_getClipModels(entity, m, portalList, entity.GetWorldTransform().Position, null, Matrix4.Identity, depth, 0));
                }
            }
            else
            {
                foreach (Model m in entity.GetModels())
                {
                    clipModels.Add(new ClipModel(entity, m, new Line[0], Matrix4.Identity));
                }
            }
            return clipModels;
        }

        /// <param name="depth">Number of iterations.</param>
        /// <param name="clipModels">Adds the ClipModel instances to this list.</param>
        private static List<ClipModel> _getClipModels(IRenderable entity, Model model, IList<IPortal> portalList, Vector2 centerPoint, IPortal portalEnter, Matrix4 modelMatrix, int depth, int count)
        {
            List<ClipModel> clipModels = new List<ClipModel>();
            if (depth <= 0)
            {
                return clipModels;
            }
            List<IPortal> collisions = new List<IPortal>();
            foreach (IPortal portal in portalList)
            {
                if (!Portal.IsValid(portal))
                {
                    continue;
                }

                Line portalLine = new Line(Portal.GetWorldVerts(portal));
                Vector2[] convexHull = Vector2Ext.Transform(model.GetWorldConvexHull(), entity.GetWorldTransform().GetMatrix() * modelMatrix);

                /* Don't clip with portals unless part of the portal is slightly inside the model's convex hull.  
                 * This is to prevent rounding errors from causing a portal to clip a model it isn't touching.*/
                if (MathExt.LineInPolygon(portalLine, convexHull) && 
                    (MathExt.PointPolygonDistance(portalLine[0], convexHull) > PORTAL_CLIP_MARGIN || 
                    MathExt.PointPolygonDistance(portalLine[1], convexHull) > PORTAL_CLIP_MARGIN))
                {
                    collisions.Add(portal);
                }
            }

            collisions = collisions.OrderBy(item => (item.GetWorldTransform().Position - centerPoint).Length).ToList();
            for (int i = 0; i < collisions.Count; i++)
            {
                IPortal portal = collisions[i];
                for (int j = collisions.Count - 1; j > i; j--)
                {
                    Line currentLine = new Line(Portal.GetWorldVerts(collisions[i]));
                    Line checkLine = new Line(Portal.GetWorldVerts(collisions[j]));
                    Side checkSide = currentLine.GetSideOf(checkLine);
                    if (checkSide != currentLine.GetSideOf(centerPoint))
                    {
                        collisions.RemoveAt(j);
                    }
                }
            }

            List<Line> clipLines = new List<Line>();
            foreach (IPortal portal in collisions)
            {
                Vector2[] pv = Portal.GetWorldVerts(portal);
                Line clipLine = new Line(pv);

                Line portalLine = new Line(pv);
                Vector2 normal = portal.GetWorldTransform().GetRight();
                if (portal.GetWorldTransform().MirrorX)
                {
                    normal = -normal;
                }

                Vector2 portalNormal = portal.GetWorldTransform().Position + normal;
                if (portalLine.GetSideOf(centerPoint) != portalLine.GetSideOf(portalNormal))
                {
                    normal *= Portal.EnterMinDistance;
                }
                else
                {
                    clipLine = clipLine.Reverse();
                    normal *= -Portal.EnterMinDistance;
                }

                clipLines.Add(clipLine);
                if (portalEnter == null || portal != portalEnter.Linked)
                {
                    Vector2 centerPointNext = Vector2Ext.Transform(portal.GetWorldTransform().Position + normal, Portal.GetLinkedMatrix(portal));
                    clipModels.AddRange(_getClipModels(entity, model, portalList, centerPointNext, portal, modelMatrix * Portal.GetLinkedMatrix(portal), depth - 1, count + 1));
                }
            }
            clipModels.Add(new ClipModel(entity, model, clipLines.ToArray(), modelMatrix));
            return clipModels;
        }

        public static List<ClipPolygon> GetClipModels(IList<Vector2> polygon, Vector2 center, IList<IPortal> portalList, int depth)
        {
            return _getClipModels(polygon, portalList, center, null, Matrix4.Identity, depth, 0);
        }

        private static List<ClipPolygon> _getClipModels(IList<Vector2> polygon, IList<IPortal> portalList, Vector2 centerPoint, IPortal portalEnter, Matrix4 modelMatrix, int depth, int count)
        {
            List<ClipPolygon> clipModels = new List<ClipPolygon>();
            if (depth <= 0)
            {
                return clipModels;
            }
            List<IPortal> collisions = new List<IPortal>();
            foreach (IPortal portal in portalList)
            {
                if (!Portal.IsValid(portal))
                {
                    continue;
                }

                Line portalLine = new Line(Portal.GetWorldVerts(portal));

                List<Vector2> convexHull = new List<Vector2>(polygon);
                convexHull.Add(centerPoint);
                convexHull = (List<Vector2>)Vector2Ext.Transform(MathExt.GetConvexHull(convexHull, true), modelMatrix);

                if (MathExt.LinePolygonDistance(portalLine, convexHull) < PORTAL_CLIP_MARGIN)
                {
                    collisions.Add(portal);
                }
            }

            collisions = collisions.OrderBy(item => (item.GetWorldTransform().Position - centerPoint).Length).ToList();
            for (int i = 0; i < collisions.Count; i++)
            {
                IPortal portal = collisions[i];
                for (int j = collisions.Count - 1; j > i; j--)
                {
                    Line currentLine = new Line(Portal.GetWorldVerts(collisions[i]));
                    Line checkLine = new Line(Portal.GetWorldVerts(collisions[j]));
                    Side checkSide = currentLine.GetSideOf(checkLine);
                    if (checkSide != currentLine.GetSideOf(centerPoint))
                    {
                        collisions.RemoveAt(j);
                    }
                }
            }

            List<Line> clipLines = new List<Line>();
            foreach (IPortal portal in collisions)
            {
                Vector2[] pv = Portal.GetWorldVerts(portal);
                Line clipLine = new Line(pv);

                Line portalLine = new Line(pv);
                Vector2 normal = portal.GetWorldTransform().GetRight();
                if (portal.GetWorldTransform().MirrorX)
                {
                    normal = -normal;
                }

                Vector2 portalNormal = portal.GetWorldTransform().Position + normal;
                if (portalLine.GetSideOf(centerPoint) != portalLine.GetSideOf(portalNormal))
                {
                    normal *= Portal.EnterMinDistance;
                }
                else
                {
                    clipLine = clipLine.Reverse();
                    normal *= -Portal.EnterMinDistance;
                }

                clipLines.Add(clipLine);
                if (portalEnter == null || portal != portalEnter.Linked)
                {
                    Vector2 centerPointNext = Vector2Ext.Transform(portal.GetWorldTransform().Position + normal, Portal.GetLinkedMatrix(portal));
                    clipModels.AddRange(_getClipModels(polygon, portalList, centerPointNext, portal, modelMatrix * Portal.GetLinkedMatrix(portal), depth - 1, count + 1));
                }
            }
            clipModels.Add(new ClipPolygon(polygon, clipLines.ToArray(), modelMatrix));
            return clipModels;
        }
    }
}
