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

        public class Result : IGetWorldTransformVelocity
        {
            Transform2 _worldTransform;
            Transform2 _worldVelocity;

            public Transform2 WorldTransform => _worldTransform.ShallowClone();
            public Transform2 WorldVelocity => _worldVelocity.ShallowClone();

            public IReadOnlyCollection<(EnterCallbackData EnterData, double MovementT)> PortalsEntered;

            public Result(Transform2 worldTransform, Transform2 worldVelocity)
                : this(worldTransform, worldVelocity, new List<(EnterCallbackData EnterData, double MovementT)>())
            {
            }

            public Result(Transform2 worldTransform, Transform2 worldVelocity, List<(EnterCallbackData EnterData, double MovementT)> portalsEntered)
            {
                Debug.Assert(portalsEntered != null);
                _worldTransform = worldTransform.ShallowClone();
                _worldVelocity = worldVelocity.ShallowClone();
                PortalsEntered = portalsEntered.AsReadOnly();
            }
        }

        /// <summary>
        /// Moves a portable object along a ray, taking into account portal teleportation.  Portal velocity is ignored for this method.
        /// </summary>
        /// <param name="portalable">Instance travelling along ray.</param>
        /// <param name="portals">Collection of portals used for portal teleportation.</param>
        /// <param name="settings"></param>
        /// <param name="portalEnter">Callback that is executed after entering a portal.</param>
        public static Result RayCast(Transform2 worldTransform, Transform2 worldVelocity, IEnumerable<IPortalRenderable> portals, Settings settings, Action<EnterCallbackData, double> portalEnter = null)
        {
            Debug.Assert(settings.MaxIterations >= 0);
            Debug.Assert(portals != null);
            Debug.Assert(settings.TimeScale >= 0);
            if (worldVelocity.Position.Length == 0 || settings.TimeScale == 0)
            {
                return new Result(worldTransform, worldVelocity);
            }

            return _rayCast(
                worldTransform.ShallowClone(),
                worldVelocity.ShallowClone(),
                portals,
                worldVelocity.Position.Length * settings.TimeScale,
                null,
                portalEnter,
                settings,
                0);
        }

        static Result _rayCast(Transform2 worldTransform, Transform2 velocityCurrent, IEnumerable<IPortalRenderable> portals, double movementLeft, IPortalRenderable portalPrevious, Action<EnterCallbackData, double> portalEnter, Settings settings, int count)
        {
            Transform2 begin = worldTransform;
            Transform2 velocity = velocityCurrent.Multiply(settings.TimeScale);
            if (settings.MaxIterations <= count)
            {
                //If we run out of iterations before running out of movement, call _rayEnd with 0 movementLeft just to make sure the AdjustEnpoint setting is handled.
                var end = _rayEnd(portals, 0, portalPrevious, settings, begin, velocity);
                return new Result(end, velocity);
            }

            double distanceMin = movementLeft;
            IPortalRenderable portalNearest = null;
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

                worldTransform = Portal.Enter(portalNearest, begin);
                velocity = Portal.EnterVelocity(portalNearest, (float)intersectNearest.First, velocity, true);


                //portalEnter?.Invoke(new EnterCallbackData(portalNearest, placeable, intersectNearest.First), t);

                movementLeft *= Math.Abs(worldTransform.Size / begin.Size);
                var result = _rayCast(worldTransform.ShallowClone(), velocity.ShallowClone(), portals, movementLeft, portalNearest.Linked, portalEnter, settings, count + 1);
                var list = new List<(EnterCallbackData EnterData, double MovementT)>(result.PortalsEntered);
                list.Insert(0, ValueTuple.Create(new EnterCallbackData(portalNearest, null, worldTransform, velocity, intersectNearest.First), t));
                return new Result(result.WorldTransform, result.WorldVelocity, list);
            }
            else
            {
                var end = _rayEnd(portals, movementLeft, portalPrevious, settings, begin, velocity);
                return new Result(end, velocity);
            }
        }

        static Transform2 _rayEnd(IEnumerable<IPortalRenderable> portals, double movementLeft, IPortalRenderable portalPrevious, Settings settings, Transform2 begin, Transform2 velocity)
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
