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
        public static void Step(IList<ProxyPortal> portals, IList<ProxyPortalable> portalables, int iterations, float stepSize)
        {
            Debug.Assert(iterations > 0);
            Debug.Assert(portals != null);
            Debug.Assert(portalables != null);

            for (int i = 0; i < iterations; i++)
            {
                foreach (ProxyPortalable portalable in portalables)
                {
                    if (portalable.Portalable is Actor)
                    {
                        SceneExt.RayCast(portalable, portals, (IPortal portal) =>
                        {
                            Portal.EnterVelocity(portal, portalable.TrueVelocity);
                        },
                        1 / iterations);
                    }
                    else
                    {
                        SceneExt.RayCast(portalable, portals, 1 / iterations);
                    }
                    portalable.Transform.Rotation += portalable.Velocity.Rotation / iterations;
                    portalable.Transform.Size *= portalable.Velocity.Size / iterations;
                }
                foreach (ProxyPortal p in portals)
                {
                    if (!Portal.IsValid(p))
                    {
                        continue;
                    }
                    if (p.WorldTransform == new Transform2())
                    {
                        continue;
                    }
                    List<Vector2> quad = new List<Vector2>();
                    quad.AddRange(Portal.GetWorldVerts(p));
                    p.WorldTransform = p.WorldTransform.Add(p.WorldVelocity.Multiply(stepSize / iterations));
                    quad.AddRange(Portal.GetWorldVerts(p));

                    foreach (ProxyPortalable portalable in portalables)
                    {
                        if (MathExt.PointInPolygon(portalable.Transform.Position, quad))
                        {
                            Portal.Enter(p, portalable);
                        }
                    }
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
