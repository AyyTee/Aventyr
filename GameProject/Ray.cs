using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Game.Common;
using Game.Rendering;

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
        }

        public class Result : IGetTransformVelocity
        {
            Transform2 _transform;
            Transform2 _velocity;

            public IReadOnlyCollection<(EnterCallbackData EnterData, double MovementT)> PortalsEntered;

            public Result(Transform2 transform, Transform2 velocity)
                : this(transform, velocity, new List<(EnterCallbackData EnterData, double MovementT)>())
            {
            }

            public Result(Transform2 transform, Transform2 velocity, List<(EnterCallbackData EnterData, double MovementT)> portalsEntered)
            {
                Debug.Assert(portalsEntered != null);
                _transform = transform.ShallowClone();
                _velocity = velocity.ShallowClone();
                PortalsEntered = portalsEntered.AsReadOnly();
            }

            public Transform2 GetTransform() => _transform.ShallowClone();
            public Transform2 GetVelocity() => _velocity.ShallowClone();
        }

        /// <summary>
        /// Moves a portable object along a ray, taking into account portal teleportation.  Portal velocity is ignored for this method.
        /// </summary>
        /// <param name="portalable">Instance travelling along ray.</param>
        /// <param name="portals">Collection of portals used for portal teleportation.</param>
        /// <param name="settings"></param>
        /// <param name="portalEnter">Callback that is executed after entering a portal.</param>
        public static Result RayCast(Transform2 transform, Transform2 velocity, IEnumerable<IPortal> portals, Settings settings, Action<EnterCallbackData, double> portalEnter = null)
        {
            Debug.Assert(settings.MaxIterations >= 0);
            Debug.Assert(portals != null);
            Debug.Assert(settings.TimeScale >= 0);
            if (velocity.Position.Length == 0 || settings.TimeScale == 0)
            {
                return new Result(transform, velocity);
            }

            return _rayCast(
                transform.ShallowClone(),
                velocity.ShallowClone(),
                portals,
                velocity.Position.Length * settings.TimeScale,
                null,
                portalEnter,
                settings,
                0);
        }

        static Result _rayCast(Transform2 transform, Transform2 velocityCurrent, IEnumerable<IPortal> portals, double movementLeft, IPortal portalPrevious, Action<EnterCallbackData, double> portalEnter, Settings settings, int count)
        {
            Transform2 begin = transform;
            Transform2 velocity = velocityCurrent.Multiply(settings.TimeScale);
            if (settings.MaxIterations <= count)
            {
                //If we run out of iterations before running out of movement, call _rayEnd with 0 movementLeft just to make sure the AdjustEnpoint setting is handled.
                var end = _rayEnd(portals, 0, portalPrevious, settings, begin, velocity);
                return new Result(end, velocity);
            }

            double distanceMin = movementLeft;
            IPortal portalNearest = null;
            IntersectCoord intersectNearest = null;
            LineF ray = new LineF(begin.Position, begin.Position + velocity.Position);
            foreach (var p in portals)
            {
                if (!p.IsValid() || portalPrevious == p)
                {
                    continue;
                }

                LineF portalLine = new LineF(p.GetWorldVerts());
                IntersectCoord intersect = MathExt.LineLineIntersect(portalLine, ray, true);

                if (intersect != null)
                {
                    double distance = ((Vector2d)begin.Position - intersect.Position).Length;
                    if (distance < distanceMin)
                    {
                        distanceMin = distance;
                        portalNearest = p;
                        intersectNearest = intersect;
                    }
                }
            }
            if (portalNearest != null)
            {
                movementLeft -= distanceMin;

                double t = (velocity.Position.Length - movementLeft) / velocity.Position.Length;

                begin.Position = (Vector2)intersectNearest.Position;

                transform = Portal.Enter(portalNearest, begin);
                velocity = Portal.EnterVelocity(portalNearest, (float)intersectNearest.First, velocity, true);


                //portalEnter?.Invoke(new EnterCallbackData(portalNearest, placeable, intersectNearest.First), t);

                movementLeft *= Math.Abs(transform.Size / begin.Size);
                var result = _rayCast(transform.ShallowClone(), velocity.ShallowClone(), portals, movementLeft, portalNearest.Linked, portalEnter, settings, count + 1);
                var list = new List<(EnterCallbackData EnterData, double MovementT)>(result.PortalsEntered);
                list.Insert(0, ValueTuple.Create(new EnterCallbackData(portalNearest, null, transform, velocity, intersectNearest.First), t));
                return new Result(result.GetTransform(), result.GetVelocity(), list);
            }
            else
            {
                var end = _rayEnd(portals, movementLeft, portalPrevious, settings, begin, velocity);
                return new Result(end, velocity);
            }
        }

        static Transform2 _rayEnd(IEnumerable<IPortal> portals, double movementLeft, IPortal portalPrevious, Settings settings, Transform2 begin, Transform2 velocity)
        {
            begin.Position += velocity.Position.Normalized() * (float)movementLeft;
            if (settings.AdjustEndpoint)
            {
                /*After the end position of the ray has been determined, adjust it's position so that it isn't too close to 
                 * any portal.  Otherwise there is a risk of ambiguity as to which side of a portal the end point is on.*/
                begin = AddMargin(portals, portalPrevious, begin, velocity);
            }
            return begin;
        }

        /// <summary>
        /// Determine a position that is sufficiently far away from all portals so that it isn't ambiguous which side of a 
        /// portal the position is at.
        /// </summary>
        /// <param name="portals"></param>
        /// <param name="portalPrevious">The last portal that was exited.</param>
        /// <param name="transform"></param>
        /// <param name="velocity"></param>
        static Transform2 AddMargin(IEnumerable<IPortalRenderable> portals, IPortalRenderable portalPrevious, Transform2 transform, Transform2 velocity)
        {
            transform = transform.ShallowClone();
            foreach (var p in portals)
            {
                if (!p.IsValid())
                {
                    continue;
                }
                var exitLine = new LineF(p.GetWorldVerts());
                Vector2 position = transform.Position;
                double distanceToPortal = MathExt.PointLineDistance(position, exitLine, true);
                if (distanceToPortal < Portal.EnterMinDistance)
                {
                    Vector2 exitNormal = p.WorldTransform.GetRight();
                    Side sideOf = p == portalPrevious ?
                        exitLine.GetSideOf(position + velocity.Position) :
                        exitLine.GetSideOf(position - velocity.Position);
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
