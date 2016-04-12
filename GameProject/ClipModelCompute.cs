using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class ClipModelCompute
    {
        public static List<ClipModel> GetClipModels(IEntity entity, IList<Portal> portalList, int depth)
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
        private static List<ClipModel> _getClipModels(IEntity entity, Model model, IList<Portal> portalList, Vector2 centerPoint, Portal portalEnter, Matrix4 modelMatrix, int depth, int count)
        {
            List<ClipModel> clipModels = new List<ClipModel>();
            if (depth <= 0)
            {
                return clipModels;
            }
            List<float> cutLines = new List<float>();
            List<Portal> collisions = new List<Portal>();
            foreach (Portal portal in portalList)
            {
                //ignore any portal attached to this entity on the first recursive iteration
                if (portal.Parent == entity && count == 0)
                {
                    continue;
                }
                Line portalLine = new Line(portal.GetWorldVerts());
                Vector2[] convexHull = Vector2Ext.Transform(model.GetWorldConvexHull(), entity.GetWorldTransform().GetMatrix() * modelMatrix);

                if (portalLine.IsInsideOfPolygon(convexHull) && portal.IsValid())
                {
                    collisions.Add(portal);
                }
            }

            collisions = collisions.OrderBy(item => (item.GetWorldTransform().Position - centerPoint).Length).ToList();
            for (int i = 0; i < collisions.Count; i++)
            {
                Portal portal = collisions[i];
                for (int j = collisions.Count - 1; j > i; j--)
                {
                    Line currentLine = new Line(collisions[i].GetWorldVerts());
                    Line checkLine = new Line(collisions[j].GetWorldVerts());
                    Side checkSide = currentLine.GetSideOf(checkLine);
                    if (checkSide != currentLine.GetSideOf(centerPoint))
                    {
                        collisions.RemoveAt(j);
                    }
                }
            }

            List<Line> clipLines = new List<Line>();
            foreach (Portal portal in collisions)
            {
                Vector2[] pv = portal.GetWorldVerts();
                Line clipLine = new Line(pv);

                Line portalLine = new Line(pv);
                Vector2 normal = portal.GetWorldTransform().GetRight();
                if (portal.GetWorldTransform().IsMirrored)
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
                    clipLine.Reverse();
                    normal *= -Portal.EnterMinDistance;
                }

                clipLines.Add(clipLine);
                if (portalEnter == null || portal != portalEnter.Linked)
                {
                    Vector2 centerPointNext = Vector2Ext.Transform(portal.GetWorldTransform().Position + normal, portal.GetPortalMatrix());
                    clipModels.AddRange(_getClipModels(entity, model, portalList, centerPointNext, portal, modelMatrix * portal.GetPortalMatrix(), depth - 1, count + 1));
                }
            }
            clipModels.Add(new ClipModel(entity, model, clipLines.ToArray(), modelMatrix));
            return clipModels;
        }
    }
}
