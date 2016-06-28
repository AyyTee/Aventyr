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
        private class PointMovement
        {
            public Line StartEnd;
            public IPortalable Instance;
            public PointMovement(IPortalable instance, Line startEnd)
            {
                Instance = instance;
                StartEnd = startEnd;
            }
        }

        private class LineMovement
        {
            public Line Start;
            public Line End;
            public IPortal Portal;
            public LineMovement(IPortal portal, Line start, Line end)
            {
                Portal = portal;
                Start = start;
                End = end;
            }
        }

        public static void Step(IEnumerable<IPortalable> moving, IEnumerable<IPortal> portals, float stepSize)
        {
            List<PointMovement> points = new List<PointMovement>();
            List<LineMovement> lines = new List<LineMovement>();

            foreach (IPortal p in portals)
            {
                lines.Add(new LineMovement(p, new Line(Portal.GetWorldVerts(p)), new Line()));
            }
            foreach (IPortalable p in moving)
            {
                Line startEnd = new Line(
                    p.GetTransform().Position,
                    p.GetTransform().Position + p.GetVelocity().Position);
                points.Add(new PointMovement(p, startEnd));
            }
            foreach (LineMovement line in lines)
            {
                line.End = new Line(Portal.GetWorldVerts(line.Portal));
            }

            foreach (PointMovement p in points)
            {
                if (p.Instance.IsPortalable)
                {

                }
            }
        }

        public static void Step(IList<ProxyPortal> portals, IList<ProxyPortalable> portalables, int iterations, float stepSize)
        {
            Debug.Assert(iterations > 0);
            Debug.Assert(portals != null);
            Debug.Assert(portalables != null);

            float iterationLength = stepSize / iterations;
            for (int i = 0; i < iterations; i++)
            {
                foreach (ProxyPortalable portalable in portalables)
                {
                    //Note that at very high iterations, portalable instance can fail to enter a portal 
                    //due to their velocity being less than Portal.EnterMinDistance.
                    portalable.Transform.Rotation += portalable.Velocity.Rotation * iterationLength;
                    portalable.Transform.Size += portalable.Velocity.Size * iterationLength;
                    SceneExt.RayCast(portalable, portals, (IPortal portal) =>
                    {
                        portalable.TrueVelocity = Portal.EnterVelocity(portal, portalable.TrueVelocity);
                        portalable.Portalable.EnterPortal?.Invoke(portal, null, null);
                    },
                    iterationLength);
                }
                foreach (ProxyPortal p in portals)
                {
                    if (!Portal.IsValid(p))
                    {
                        continue;
                    }
                    if (p.WorldVelocity == new Transform2())
                    {
                        continue;
                    }
                    List<Vector2> quad = new List<Vector2>();
                    quad.AddRange(Portal.GetWorldVerts(p));

                    p.WorldTransform = p.WorldTransform.Add(p.WorldVelocity.Multiply(iterationLength));

                    //Make the size of the quad slightly larger than it really is to prevent a situation where the portal 
                    //"pushes" away a portalable instance.
                    //Note that there is still a failure case if the portal is rotating slowly but not translating.
                    Vector2 margin = Vector2.Zero;
                    if (p.WorldVelocity.Position != Vector2.Zero)
                    {
                        margin = p.WorldVelocity.Position.Normalized() * Portal.EnterMinDistance * 2;
                    }
                    p.WorldTransform.Position += margin;
                    //Reverse the order of the next vertice pair so that this forms a quadrilateral.
                    quad.AddRange(Portal.GetWorldVerts(p).Reverse());

                    foreach (ProxyPortalable portalable in portalables)
                    {
                        if (MathExt.PointInPolygon(portalable.Transform.Position, quad))
                        {
                            Portal.Enter(p, portalable);
                            portalable.TrueVelocity = Portal.EnterVelocity(p, portalable.TrueVelocity);
                        }
                    }
                    p.WorldTransform.Position -= margin;
                }
            }

            foreach (ProxyPortalable portalable in portalables)
            {
                portalable.Portalable.SetTransform(portalable.Transform);
                if (portalable.Portalable is Actor)
                {
                    portalable.Portalable.SetVelocity(portalable.TrueVelocity);
                }
                else
                {
                    portalable.Portalable.SetVelocity(portalable.Velocity);
                }
            }
        }
    }
}
