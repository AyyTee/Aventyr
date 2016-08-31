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
            Step(moving, portals, stepSize, portalEnter, null);
        }

        private static void Step(IEnumerable<IPortalable> moving, IEnumerable<IPortal> portals, double stepSize, Action<EnterCallbackData> portalEnter, PortalableSweep previous)
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
                    lineMovement.Add(new PortalMovement(p, lineStart, new Line()));
                }

                foreach (IPortalable p in moving)
                {
                    Transform2 transform = p.GetTransform();
                    Vector2 pointStart = transform.Position;

                    Transform2 shift = p.GetVelocity().Multiply((float)stepSize);
                    //p.SetTransform(transform.Add(shift));
                    p.Transform = p.Transform.Add(shift);
                    Vector2 pointEnd = p.GetTransform().Position;

                    pointMovement.Add(new PortalableMovement(p, new Line(pointStart, pointEnd), transform));
                }

                foreach (PortalMovement line in lineMovement)
                {
                    line.End = new Line(Portal.GetWorldVerts(line.Portal));
                }

                //Move portalable instances back to their starting spots.
                foreach (PortalableMovement p in pointMovement)
                {
                    p.Instance.Transform = p.Previous;
                }
            }

            PortalableSweep earliest = GetEarliestCollision(pointMovement, lineMovement, previous);

            if (earliest == null)
            {
                foreach (PortalableMovement p in pointMovement)
                {
                    IPortalable portalable = p.Instance;
                    Transform2 shift = portalable.GetVelocity().Multiply((float)stepSize);
                    portalable.SetTransform(portalable.GetTransform().Add(shift));
                }
                return;
            }

            //Move portalable instance back to their start positions so that portal teleportation can be done correctly.
            /*foreach (PortalableMovement p in pointMovement)
            {
                p.Instance.SetTransform(p.Previous);
                
            }*/

            double tDelta = earliest.Sweep.TimeProportion;
            foreach (PortalableMovement move in pointMovement)
            {
                Transform2 velocity = move.Instance.GetVelocity().Multiply((float)(stepSize * tDelta));
                move.Instance.SetTransform(move.Instance.GetTransform().Add(velocity));
            }
            float intersectT = (float)earliest.Sweep.AcrossProportion;
            Portal.Enter(earliest.Portal.Portal, earliest.Portalable.Instance, intersectT);
            portalEnter?.Invoke(new EnterCallbackData(earliest.Portal.Portal, earliest.Portalable.Instance, intersectT));
            Step(moving, portals, stepSize * (1 - tDelta), portalEnter, earliest);
        }

        private class NextPosition
        {
            public IPortalable Instance;
            public Transform2 WorldTransformNext;
            public Transform2 WorldVelocityNext;

            public NextPosition(IPortalable instance, Transform2 worldTransformNext, Transform2 worldVelocityNext)
            {
                Instance = instance;
                WorldTransformNext = worldTransformNext;
                WorldVelocityNext = worldVelocityNext;
            }
        }

        private static void SetNextVelocity(IEnumerable<IPortalCommon> moving)
        {
            List<NextPosition> nextList = new List<NextPosition>();
            foreach (IPortalCommon p in moving)
            {
                Transform2 velocity = p.WorldVelocityPrevious;

                //for (int i = 0; i <)
                //nextList.Add(new NextPosition(p));
            }
        }

        private static PortalableSweep GetEarliestCollision(List<PortalableMovement> pointMovement, List<PortalMovement> lineMovement, PortalableSweep previous)
        {
            double tMin = 1;
            const double repeatIntersectionEpsilon = 0.0005;
            PortalableSweep earliest = null;
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
                        if (time >= 0 && time < tMin)
                        {
                            //Prevent rounding portalable instance for immediately entering the portal it exited.
                            if (previous?.Portal.Portal.Linked == portal.Portal && 
                                previous?.Portalable.Instance == move.Instance && 
                                time < repeatIntersectionEpsilon)
                            {
                                continue;
                            }
                            earliest = new PortalableSweep(collisionList[0], move, portal);
                            tMin = collisionList[0].TimeProportion;
                        }
                    }
                }
            }
            return earliest;
        }
    }
}
