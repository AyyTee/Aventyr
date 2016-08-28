using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Portals
{
    /// <summary>
    /// Simple implementation of IPortalable.
    /// </summary>
    public class Portalable : IPortalable
    {
        public Action<EnterCallbackData, Transform2, Transform2> EnterPortal { get; set; }
        public bool IsPortalable { get; set; } = true;
        public Transform2 Transform { get; set; } = new Transform2();
        public Transform2 Velocity { get; set; } = Transform2.CreateVelocity();

        public Portalable()
        {
        }

        public Portalable(Transform2 transform, Transform2 velocity)
        {
            SetTransform(transform);
            SetVelocity(velocity);
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

        public List<IPortal> GetPortalChildren()
        {
            return new List<IPortal>();
        }
    }
}
