using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class SceneExt
    {
        public static void RayCast(IPortalable placeable, IList<IPortal> portals)
        {
            if (placeable.GetVelocity().Position.Length == 0)
            {
                return;
            }
            _rayCast(placeable, portals, placeable.GetVelocity().Position.Length, null, 50);
        }

        private static void _rayCast(IPortalable placeable, IList<IPortal> portals, double movementLeft, IPortal portalPrevious, int depthMax)
        {
            if (depthMax <= 0)
            {
                return;
            }
            Transform2 begin = placeable.GetTransform();
            Transform2 velocity = placeable.GetVelocity();
            double distanceMin = movementLeft;
            IPortal portalNearest = null;
            IntersectPoint intersectNearest = new IntersectPoint();
            foreach (IPortal p in portals)
            {
                if (!Portal.IsValid(p))
                {
                    continue;
                }
                if (portalPrevious == p)
                {
                    continue;
                }
                Line portalLine = new Line(Portal.GetWorldVerts(p));
                Line ray = new Line(begin.Position, begin.Position + velocity.Position);
                IntersectPoint intersect = portalLine.Intersects(ray, true);
                //IntersectPoint intersect2 = portalLine.IntersectsParametric(p.GetVelocity(), ray, 5);
                double distance = ((Vector2d)begin.Position - intersect.Position).Length;
                if (intersect.Exists && distance < distanceMin)
                {
                    distanceMin = distance;
                    portalNearest = p;
                    intersectNearest = intersect;
                }
            }
            if (portalNearest != null)
            {
                movementLeft -= distanceMin;
                begin.Position = (Vector2)intersectNearest.Position;
                placeable.SetTransform(begin);
                Portal.Enter(portalNearest, placeable);
                _rayCast(placeable, portals, movementLeft, portalNearest.Linked, depthMax - 1);
            }
            else
            {
                begin.Position += velocity.Position.Normalized() * (float)movementLeft;
                /*After the end position of the ray has been determined, adjust it's position so that it isn't too close to any portal.  
                Otherwise there is a risk of ambiguity as to which side of a portal the end point is on.*/
                foreach (IPortal p in portals)
                {
                    if (!Portal.IsValid(p))
                    {
                        continue;
                    }
                    Line exitLine = new Line(Portal.GetWorldVerts(p));
                    Vector2 position = begin.Position;
                    float distanceToPortal = exitLine.PointDistance(position, true);
                    if (distanceToPortal < Portal.EnterMinDistance)
                    {
                        Vector2 exitNormal = p.GetWorldTransform().GetRight();
                        Side sideOf;
                        if (p == portalPrevious)
                        {
                            sideOf = exitLine.GetSideOf(position + velocity.Position);
                        }
                        else
                        {
                            sideOf = exitLine.GetSideOf(position - velocity.Position);
                        }
                        if (sideOf != exitLine.GetSideOf(exitNormal + p.GetWorldTransform().Position))
                        {
                            exitNormal = -exitNormal;
                        }

                        Vector2 pos = exitNormal * (Portal.EnterMinDistance - distanceToPortal);
                        begin.Position += pos;
                        break;
                    }
                }
                placeable.SetTransform(begin);
            }
        }
    }
}
