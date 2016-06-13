using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class SceneExt
    {
        /// <summary>
        /// Moves a portable object along a ray, taking into account portal teleportation.
        /// </summary>
        /// <param name="portalable">Instance travelling along ray.</param>
        /// <param name="portals">Collection of portals used for portal teleportation.</param>
        /// <param name="ignorePortalVelocity">Don't add a portals world velocity when entering it.</param>
        /// <param name="timeScale">Scaling factor for velocity.</param>
        /// <param name="maxIterations">The raycast will stop after this number of portal teleportations.</param>
        public static void RayCast(IPortalable portalable, IEnumerable<IPortal> portals, float timeScale = 1, int maxIterations = 50)
        {
            RayCast(portalable, portals, null);
        }


        public static void RayCast(IPortalable portalable, IEnumerable<IPortal> portals, Action<IPortal> portalEnter, float timeScale = 1, int maxIterations = 50)
        {
            Debug.Assert(maxIterations >= 0);
            Debug.Assert(portalable != null);
            Debug.Assert(portals != null);
            Debug.Assert(timeScale >= 0);
            if (portalable.GetVelocity().Position.Length == 0 || timeScale == 0)
            {
                return;
            }
            _rayCast(portalable, portals, portalable.GetVelocity().Position.Length * timeScale, null, 50, timeScale, portalEnter);
        }

        private static void _rayCast(IPortalable placeable, IEnumerable<IPortal> portals, double movementLeft, IPortal portalPrevious, int depthMax, float timeScale, Action<IPortal> portalEnter)
        {
            if (depthMax <= 0)
            {
                return;
            }
            Transform2 begin = placeable.GetTransform();
            Transform2 velocity = placeable.GetVelocity().Multiply(timeScale);
            double distanceMin = movementLeft;
            IPortal portalNearest = null;
            IntersectCoord intersectNearest = new IntersectCoord();
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
                IntersectCoord intersect = MathExt.LineLineIntersect(portalLine, ray, true);
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
                portalEnter?.Invoke(portalNearest);
                _rayCast(placeable, portals, movementLeft, portalNearest.Linked, depthMax - 1, timeScale, portalEnter);
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
                    double distanceToPortal = MathExt.PointLineDistance(position, exitLine, true);
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

                        Vector2 pos = exitNormal * (Portal.EnterMinDistance - (float)distanceToPortal);
                        begin.Position += pos;
                        break;
                    }
                }
                placeable.SetTransform(begin);
            }
        }
    }
}
