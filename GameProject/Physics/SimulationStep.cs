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
            public Transform2 Previous;
            public Line StartEnd;
            public IPortalable Instance;
            public PortalableMovement(IPortalable instance, Line startEnd, Transform2 previous)
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

        public static void Step(IEnumerable<IPortalable> moving, IEnumerable<IPortal> portals, double stepSize, Action<EnterCallbackData> portalEnter)
        {
            /*foreach (IPortalable p in moving)
            {
                Transform2 shift = p.GetVelocity().Multiply((float)stepSize);
                p.SetTransform(p.GetTransform().Add(shift));
            }*/
            Step(moving, portals, stepSize, portalEnter, new List<PortalableSweep>());
        }

        private static void Step(IEnumerable<IPortalable> moving, IEnumerable<IPortal> portals, double stepSize, Action<EnterCallbackData> portalEnter, List<PortalableSweep> previous)
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
                    Line lineStart = new Line(Portal.GetWorldVerts(p));

                    Transform2 t = p.WorldTransform.Add(p.WorldVelocity.Multiply((float)stepSize));
                    Line lineEnd = new Line(Portal.GetWorldVerts(p, t));

                    lineMovement.Add(new PortalMovement(p, lineStart, lineEnd));
                }

                foreach (IPortalable p in moving)
                {
                    if (p.WorldTransform == null || p.WorldVelocity == null)
                    {
                        continue;
                    }
                    Transform2 shift = p.WorldVelocity.Multiply((float)stepSize);
                    Transform2 t = p.WorldTransform.Add(p.WorldVelocity.Multiply((float)stepSize));

                    Line movement = new Line(p.WorldTransform.Position, t.Position);

                    pointMovement.Add(new PortalableMovement(p, movement, p.WorldTransform));
                }
            }

            List<PortalableSweep> earliest = GetEarliestCollision(pointMovement, lineMovement, previous);

            if (earliest.Count == 0)
            {
                foreach (PortalableMovement p in pointMovement)
                {
                    IPortalable portalable = p.Instance;
                    Transform2 shift = portalable.GetVelocity().Multiply((float)stepSize);
                    portalable.SetTransform(portalable.GetTransform().Add(shift));

                    Transform2 worldVelocity = p.Instance.WorldVelocity.Multiply((float)stepSize);
                    p.Instance.WorldTransform = p.Instance.WorldTransform.Add(worldVelocity);
                }
                return;
            }

            double tDelta = earliest[0].Sweep.TimeProportion;
            foreach (PortalableMovement move in pointMovement)
            {
                Transform2 velocity = move.Instance.GetVelocity().Multiply((float)(stepSize * tDelta));
                move.Instance.SetTransform(move.Instance.GetTransform().Add(velocity));

                Transform2 worldVelocity = move.Instance.WorldVelocity.Multiply((float)(stepSize * tDelta));
                move.Instance.WorldTransform = move.Instance.WorldTransform.Add(worldVelocity);
            }

            foreach (PortalableSweep sweep in earliest)
            {
                float intersectT = (float)sweep.Sweep.AcrossProportion;
                IPortalable instance = sweep.Portalable.Instance;
                bool worldOnly = instance.Parent != null && !PortalCommon.IsRoot(instance.Parent);
                Portal.Enter(sweep.Portal.Portal, instance, intersectT, false, worldOnly);
                portalEnter?.Invoke(new EnterCallbackData(sweep.Portal.Portal, sweep.Portalable.Instance, intersectT));
            }

            Step(moving, portals, stepSize * (1 - tDelta), portalEnter, earliest);
        }

        private static List<PortalableSweep> GetEarliestCollision(List<PortalableMovement> pointMovement, List<PortalMovement> lineMovement, List<PortalableSweep> previous)
        {
            double tMin = 1;
            const double repeatIntersectionEpsilon = 0.0005;
            List<PortalableSweep> earliest = new List<PortalableSweep>();
            foreach (PortalableMovement move in pointMovement)
            {
                if (move.Instance.IsPortalable)
                {
                    Debug.Assert(!(move is IPortal), "Portals cannot do portal teleporation with other portals.");
                    foreach (PortalMovement portal in lineMovement)
                    {
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
                                //Prevent rounding portalable instance for immediately entering the portal it exited.
                                /*if (previous?.Portal.Portal.Linked == portal.Portal && 
                                    previous?.Portalable.Instance == move.Instance && 
                                    time < repeatIntersectionEpsilon)
                                {
                                    continue;
                                }*/
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
    }
}
