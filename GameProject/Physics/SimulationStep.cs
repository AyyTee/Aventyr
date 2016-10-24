using Game.Common;
using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class SimulationStep
    {
        private class PortalableMovement
        {
            public Transform2d Previous;
            public Line StartEnd;
            public IPortalCommon Instance;
            public PortalableMovement(IPortalCommon instance, Line startEnd, Transform2d previous)
            {
                Instance = instance;
                StartEnd = startEnd;
                Previous = previous;
            }
        }

        private class PortalMovement
        {
            public Line Start;
            public Line End;
            public IPortal Portal;
            public PortalMovement(IPortal portal, Line start, Line end)
            {
                Portal = portal;
                Start = start;
                End = end;
            }
        }

        private class PortalableSweep
        {
            public GeometryUtil.Sweep Sweep;
            public PortalableMovement Portalable;
            public PortalMovement Portal;
            public PortalableSweep(GeometryUtil.Sweep sweep, PortalableMovement portalable, PortalMovement portal)
            {
                Sweep = sweep;
                Portalable = portalable;
                Portal = portal;
            }
        }

        public static void Step(IEnumerable<IPortalCommon> moving, IEnumerable<IPortal> portals, double stepSize, Action<EnterCallbackData> portalEnter)
        {
            Step(moving, portals, stepSize, portalEnter, new List<PortalableSweep>());
            foreach (IPortalable instance in moving.OfType<IPortalable>())
            {
                AddMargin(portals, instance);
            }
        }

        private static void Step(IEnumerable<IPortalCommon> moving, IEnumerable<IPortal> portals, double stepSize, Action<EnterCallbackData> portalEnter, List<PortalableSweep> previous)
        {
            List<PortalableMovement> pointMovement = new List<PortalableMovement>();
            List<PortalMovement> lineMovement = new List<PortalMovement>();

            //Get the start positions and initial end positions for all portals and portalables.
            {
                foreach (IPortal p in portals)
                {
                    if (!Portal.IsValid(p))
                    {
                        continue;
                    }
                    Line lineStart = new Line(Vector2Ext.ToDouble(Portal.GetWorldVerts(p)));

                    Transform2d t = (Transform2d)p.WorldTransform.Add(p.WorldVelocity.Multiply((float)stepSize));
                    Line lineEnd = new Line(Vector2Ext.ToDouble(Portal.GetWorldVerts(p, (Transform2)t)));

                    lineMovement.Add(new PortalMovement(p, lineStart, lineEnd));
                }

                foreach (IPortalCommon p in moving)
                {
                    if (p.WorldTransform == null || p.WorldVelocity == null)
                    {
                        continue;
                    }
                    Transform2d shift = (Transform2d)p.WorldVelocity.Multiply((float)stepSize);
                    Transform2d t = (Transform2d)p.WorldTransform.Add(p.WorldVelocity.Multiply((float)stepSize));

                    Line movement = new Line((Vector2d)p.WorldTransform.Position, t.Position);

                    pointMovement.Add(new PortalableMovement(p, movement, (Transform2d)p.WorldTransform));
                }
            }

            List<PortalableSweep> earliest = GetEarliestCollision(pointMovement, lineMovement, previous, stepSize);

            if (earliest.Count == 0)
            {
                foreach (PortalableMovement p in pointMovement)
                {
                    IPortalable portalable = p.Instance as IPortalable;
                    if (portalable != null)
                    {
                        Transform2d shift = (Transform2d)portalable.GetVelocity().Multiply((float)stepSize);
                        portalable.SetTransform(portalable.Transform.Add((Transform2)shift));
                    }
                    
                    Transform2d worldVelocity = (Transform2d)p.Instance.WorldVelocity.Multiply((float)stepSize);
                    p.Instance.WorldTransform = p.Instance.WorldTransform.Add((Transform2)worldVelocity);
                }
                return;
            }

            double tDelta = earliest[0].Sweep.TimeProportion;
            foreach (PortalableMovement move in pointMovement)
            {
                IPortalable portalable = move.Instance as IPortalable;
                if (portalable != null)
                {
                    Transform2d velocity = (Transform2d)portalable.GetVelocity().Multiply((float)(stepSize * tDelta));
                    portalable.SetTransform(portalable.GetTransform().Add((Transform2)velocity));
                }

                Transform2d worldVelocity = (Transform2d)move.Instance.WorldVelocity.Multiply((float)(stepSize * tDelta));
                move.Instance.WorldTransform = move.Instance.WorldTransform.Add((Transform2)worldVelocity);
            }

            foreach (PortalableSweep sweep in earliest)
            {
                float intersectT = (float)sweep.Sweep.AcrossProportion;
                IPortalCommon instance = sweep.Portalable.Instance;

                /*Before this instance enters the portal we place it exactly on the portal to reduce 
                 * precision errors.*/
                PlaceOnPortal(instance, sweep.Portal.Portal, intersectT);

                bool worldOnly = !PortalCommon.IsRoot(instance);
                Portal.Enter(sweep.Portal.Portal, instance, intersectT, false, worldOnly);
                portalEnter?.Invoke(new EnterCallbackData(sweep.Portal.Portal, sweep.Portalable.Instance, intersectT));

                /*After this instance has entered the portal we again go ahead and place it exactly on the 
                 * portal (this time on the exit) to reduce precision errors.*/
                PlaceOnPortal(instance, sweep.Portal.Portal.Linked, intersectT);
            }

            Step(moving, portals, stepSize * (1 - tDelta), portalEnter, earliest);
        }

        private static void PlaceOnPortal(IPortalCommon instance, IPortal portal, float t)
        {
            Line portalLine = new Line(Vector2Ext.ToDouble(Portal.GetWorldVerts(portal)));
            Transform2d transform = (Transform2d)instance.WorldTransform;
            transform.Position = (Vector2d)portalLine.Lerp(t);
            instance.WorldTransform = (Transform2)transform;
        }

        /// <param name="previous">A list of the previous earliest portal collisions.  This is used to 
        /// detect repeat portal entry.</param>
        /// <param name="timeSpan">This is purely used for determining what t value exceeds the minimum 
        /// amount of time allowed for repeat portal entry.</param>
        private static List<PortalableSweep> GetEarliestCollision(List<PortalableMovement> pointMovement, List<PortalMovement> lineMovement, List<PortalableSweep> previous, double timeSpan)
        {
            double tMin = 1;
            double repeatIntersectionEpsilon = 0.00005 / timeSpan;
            List<PortalableSweep> earliest = new List<PortalableSweep>();
            foreach (PortalableMovement move in pointMovement)
            {
                if (move.Instance.IsPortalable)
                {
                    Debug.Assert(!(move is IPortal), "Portals cannot do portal teleporation with other portals.");
                    foreach (PortalMovement portal in lineMovement)
                    {
                        if (move.Instance == portal.Portal)
                        {
                            continue;
                        }
                        var collisionList = MathExt.MovingPointLineIntersect(move.StartEnd, portal.Start, portal.End);
                        if (collisionList.Count == 0)
                        {
                            continue;
                        }

                        double time = collisionList[0].TimeProportion;
                        if (time >= 0)
                        {
                            if (time <= tMin)
                            {
                                if (time < tMin)
                                {
                                    earliest.Clear();
                                }
                                //Prevent precision errors from causing a portalable instance from immediately reentering a portal.
                                if (time < repeatIntersectionEpsilon)
                                {
                                    if (previous.Exists(item =>
                                        item.Portal.Portal.Linked == portal.Portal &&
                                        item.Portalable.Instance == move.Instance))
                                    {
                                        continue;
                                    }
                                }
                                earliest.Add(new PortalableSweep(collisionList[0], move, portal));
                                tMin = collisionList[0].TimeProportion;
                            }
                        }
                    }
                }
            }
            return earliest;
        }

        private static void AddMargin(IEnumerable<IPortal> portals, IPortalCommon instance)
        {
            Transform2d transform = (Transform2d)instance.WorldTransform;
            foreach (IPortal p in portals.Where(item => item.OneSided && Portal.IsValid(item)))
            {
                Line exitLine = new Line(Vector2Ext.ToDouble(Portal.GetWorldVerts(p)));
                Vector2d position = transform.Position;
                double distanceToPortal = MathExt.PointLineDistance(position, exitLine, true);
                if (distanceToPortal < Portal.EnterMinDistance)
                {
                    Vector2d exitNormal = (Vector2d)p.WorldTransform.GetRight();

                    Vector2d pos = exitNormal * (Portal.EnterMinDistance - (float)distanceToPortal);
                    transform.Position += pos;
                    instance.WorldTransform = (Transform2)transform;
                    /*We return now rather than look for more portals that are too close because it is assumed that 
                     * portals will never be closer than 2 * Portal.EnterMinDistance*/
                    return;
                }
            }
        }
    }
}
