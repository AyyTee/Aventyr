using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class ProxyPortalable : IPortalable
    {
        public readonly IPortalable Portalable;
        /// <summary>
        /// Included as a hack to make the SimulationStep code work for physics bodies.
        /// </summary>
        public Transform2 TrueVelocity = new Transform2();
        public Transform2 Transform = new Transform2();
        public Transform2 Velocity = new Transform2();

        public ProxyPortalable(IPortalable portalable)
            : this(portalable, portalable.GetTransform(), portalable.GetVelocity())
        {
        }

        public ProxyPortalable(IPortalable portalable, Transform2 transform, Transform2 velocity)
        {
            Portalable = portalable;
            Transform = transform;
            Velocity = velocity;
        }

        public Transform2 GetTransform()
        {
            return Transform.ShallowClone();
        }

        public Transform2 GetVelocity()
        {
            return Velocity.ShallowClone();
        }

        public void SetTransform(Transform2 transform)
        {
            Transform = transform.ShallowClone();
        }

        public void SetVelocity(Transform2 velocity)
        {
            Velocity = velocity.ShallowClone();
        }
    }
}
