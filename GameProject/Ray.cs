using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;

namespace Game
{
    public static class Ray
    {
        public class Settings
        {
            /// <summary>Scaling factor for velocity.</summary>
            public float TimeScale = 1;
            /// <summary>The raycast will stop after this number of portal teleportations.</summary>
            public int MaxIterations = 50;
            /// <summary>The end position of the portalable instance will be adjusted to not be too near to any portal.</summary>
            public bool AdjustEndpoint = true;

            public Settings()
            {
            }
        }

        /// <summary>
        /// Moves a portable object along a ray, taking into account portal teleportation.  Portal velocity is ignored for this method.
        /// </summary>
        /// <param name="portalable">Instance travelling along ray.</param>
        /// <param name="portals">Collection of portals used for portal teleportation.</param>
        /// <param name="portalEnter">Callback that is executed after entering a portal.</param>
        public static void RayCast(IPortalable portalable, IEnumerable<IPortal> portals, Settings settings, Action<EnterCallbackData, double> portalEnter = null)
        {
            Debug.Assert(settings.MaxIterations >= 0);
            Debug.Assert(portalable != null);
            Debug.Assert(portals != null);
            Debug.Assert(settings.TimeScale >= 0);
            Debug.Assert(!(portalable is IPortal));
            if (portalable.GetVelocity().Position.Length == 0 || settings.TimeScale == 0)
            {
                return;
            }
            _rayCast(
                portalable, 
                portals,
                portalable.GetVelocity().Position.Length * settings.TimeScale, 
                null, 
                portalEnter, 
                settings, 
                0);
        }

        private static void _rayCast(IPortalable placeable, IEnumerable<IPortal> portals, double movementLeft, IPortal portalPrevious, Action<EnterCallbackData, double> portalEnter, Settings settings, int count)
        {
            Transform2 begin = placeable.GetTransform();
            Transform2 velocity = placeable.GetVelocity().Multiply(settings.TimeScale);
            if (settings.MaxIterations <= count)
            {
                //If we run out of iterations before running out of movement, call _rayEnd with 0 movementLeft just to make sure the AdjustEnpoint setting is handled.
                _rayEnd(placeable, portals, 0, portalPrevious, settings, begin, velocity);
                return;
            }
            if (!placeable.IsPortalable)
            {
                _rayEnd(placeable, portals, movementLeft, portalPrevious, settings, begin, velocity);
                return;
            }
            double distanceMin = movementLeft;
            IPortal portalNearest = null;
            IntersectCoord intersectNearest = new IntersectCoord();
            LineF ray = new LineF(begin.Position, begin.Position + velocity.Position);
            foreach (IPortal p in portals)
            {
                if (!Portal.IsValid(p) || portalPrevious == p)
                {
                    continue;
                }

                LineF portalLine = new LineF(Portal.GetWorldVerts(p));
                IntersectCoord intersect = MathExt.LineLineIntersect(portalLine, ray, true);
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

                double t = (velocity.Position.Length - movementLeft) / velocity.Position.Length;

                begin.Position = (Vector2)intersectNearest.Position;
                placeable.SetTransform(begin);
                Portal.Enter(portalNearest, placeable, (float)intersectNearest.First, true);
                
                portalEnter?.Invoke(new EnterCallbackData(portalNearest, placeable, intersectNearest.First), t);

                movementLeft *= Math.Abs(placeable.GetTransform().Size / begin.Size);
                _rayCast(placeable, portals, movementLeft, portalNearest.Linked, portalEnter, settings, count + 1);
            }
            else
            {
                _rayEnd(placeable, portals, movementLeft, portalPrevious, settings, begin, velocity);
            }
        }

        private static void _rayEnd(IPortalable placeable, IEnumerable<IPortal> portals, double movementLeft, IPortal portalPrevious, Settings settings, Transform2 begin, Transform2 velocity)
        {
            begin.Position += velocity.Position.Normalized() * (float)movementLeft;
            if (settings.AdjustEndpoint)
            {
                /*After the end position of the ray has been determined, adjust it's position so that it isn't too close to 
                 * any portal.  Otherwise there is a risk of ambiguity as to which side of a portal the end point is on.*/
                begin = AddMargin(portals, portalPrevious, begin, velocity);
            }
            placeable.SetTransform(begin);
        }

        /// <summary>
        /// Determine a position that is sufficiently far away from all portals so that it isn't ambiguous which side of a 
        /// portal the position is at.
        /// </summary>
        /// <param name="portals"></param>
        /// <param name="portalPrevious">The last portal that was exited.</param>
        /// <param name="transform"></param>
        /// <param name="velocity"></param>
        private static Transform2 AddMargin(IEnumerable<IPortal> portals, IPortal portalPrevious, Transform2 transform, Transform2 velocity)
        {
            transform = transform.ShallowClone();
            foreach (IPortal p in portals)
            {
                if (!Portal.IsValid(p))
                {
                    continue;
                }
                LineF exitLine = new LineF(Portal.GetWorldVerts(p));
                Vector2 position = transform.Position;
                double distanceToPortal = MathExt.PointLineDistance(position, exitLine, true);
                if (distanceToPortal < Portal.EnterMinDistance)
                {
                    Vector2 exitNormal = p.WorldTransform.GetRight();
                    Side sideOf;
                    if (p == portalPrevious)
                    {
                        sideOf = exitLine.GetSideOf(position + velocity.Position);
                    }
                    else
                    {
                        sideOf = exitLine.GetSideOf(position - velocity.Position);
                    }
                    if (sideOf != exitLine.GetSideOf(exitNormal + p.WorldTransform.Position))
                    {
                        exitNormal = -exitNormal;
                    }

                    Vector2 pos = exitNormal * (Portal.EnterMinDistance - (float)distanceToPortal);
                    transform.Position += pos;
                    break;
                }
            }
            return transform;
        }
    }
}
