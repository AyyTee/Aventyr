﻿using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Portals;
using Game.Rendering;
using OpenTK;

namespace Game.Models
{
    public static class Clip
    {
        const double PortalClipMargin = 0.0001;

        public class ClipModel
        {
            public readonly LineF[] ClipLines;
            public readonly Matrix4 Transform;
            public readonly Model Model;
            public readonly IRenderable Entity;

            public ClipModel(IRenderable entity, Model model, LineF[] clipLines, Matrix4 transform)
            {
                Entity = entity;
                Model = model;
                ClipLines = clipLines;
                Transform = transform;
            }
        }

        public class ClipPolygon
        {
            public IList<LineF> ClipLines { get; set; }
            public Matrix4 Transform { get; set; }
            public IList<Vector2> Polygon { get; set; }

            public ClipPolygon(IList<Vector2> polygon, IList<LineF> clipLines, Matrix4 transform)
            {
                Polygon = polygon;
                ClipLines = clipLines;
                Transform = transform;
            }
        }

        public static List<ClipModel> GetClipModels(IRenderable entity, IList<IPortal> portalList, int depth)
        {
            var clipModels = new List<ClipModel>();
            if (entity.GetWorldTransform() == null)
            {
                return clipModels;
            }
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
                    clipModels.Add(new ClipModel(entity, m, new LineF[0], Matrix4.Identity));
                }
            }
            return clipModels;
        }

        /// <param name="depth">Number of iterations.</param>
        /// <param name="clipModels">Adds the ClipModel instances to this list.</param>
        static List<ClipModel> _getClipModels(IRenderable entity, Model model, IList<IPortal> portalList, Vector2 centerPoint, IPortal portalEnter, Matrix4 modelMatrix, int depth, int count)
        {
            List<ClipModel> clipModels = new List<ClipModel>();
            if (depth <= 0)
            {
                return clipModels;
            }
            
            List<IPortal> collisions = Portal.GetCollisions(
                centerPoint, 
                Vector2Ext.Transform(model.GetWorldConvexHull(), 
                entity.GetWorldTransform().GetMatrix() * modelMatrix), 
                portalList, 
                PortalClipMargin);

            List<LineF> clipLines = new List<LineF>();
            foreach (IPortal portal in collisions)
            {
                Vector2[] pv = Portal.GetWorldVerts(portal);
                LineF clipLine = new LineF(pv);

                LineF portalLine = new LineF(pv);
                Vector2 normal = portal.WorldTransform.GetRight();
                if (portal.WorldTransform.MirrorX)
                {
                    normal = -normal;
                }

                Vector2 portalNormal = portal.WorldTransform.Position + normal;
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
                    Vector2 centerPointNext = Vector2Ext.Transform(portal.WorldTransform.Position + normal, Portal.GetLinkedMatrix(portal));
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

        static List<ClipPolygon> _getClipModels(IList<Vector2> polygon, IList<IPortal> portalList, Vector2 centerPoint, IPortal portalEnter, Matrix4 modelMatrix, int depth, int count)
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

                LineF portalLine = new LineF(Portal.GetWorldVerts(portal));

                List<Vector2> convexHull = new List<Vector2>(polygon);
                convexHull.Add(centerPoint);
                convexHull = Vector2Ext.Transform(MathExt.GetConvexHull(convexHull, true), modelMatrix);

                if (MathExt.LinePolygonDistance(portalLine, convexHull) < PortalClipMargin)
                {
                    collisions.Add(portal);
                }
            }

            collisions = collisions.OrderBy(item => (item.WorldTransform.Position - centerPoint).Length).ToList();
            for (int i = 0; i < collisions.Count; i++)
            {
                IPortal portal = collisions[i];
                for (int j = collisions.Count - 1; j > i; j--)
                {
                    LineF currentLine = new LineF(Portal.GetWorldVerts(collisions[i]));
                    LineF checkLine = new LineF(Portal.GetWorldVerts(collisions[j]));
                    Side checkSide = currentLine.GetSideOf(checkLine);
                    if (checkSide != currentLine.GetSideOf(centerPoint))
                    {
                        collisions.RemoveAt(j);
                    }
                }
            }

            List<LineF> clipLines = new List<LineF>();
            foreach (IPortal portal in collisions)
            {
                Vector2[] pv = Portal.GetWorldVerts(portal);
                LineF clipLine = new LineF(pv);

                LineF portalLine = new LineF(pv);
                Vector2 normal = portal.WorldTransform.GetRight();
                if (portal.WorldTransform.MirrorX)
                {
                    normal = -normal;
                }

                Vector2 portalNormal = portal.WorldTransform.Position + normal;
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
                    Vector2 centerPointNext = Vector2Ext.Transform(portal.WorldTransform.Position + normal, Portal.GetLinkedMatrix(portal));
                    clipModels.AddRange(_getClipModels(polygon, portalList, centerPointNext, portal, modelMatrix * Portal.GetLinkedMatrix(portal), depth - 1, count + 1));
                }
            }
            clipModels.Add(new ClipPolygon(polygon, clipLines.ToArray(), modelMatrix));
            return clipModels;
        }
    }
}
