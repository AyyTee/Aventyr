using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    /// <summary>
    /// Simple implementation of IPortalable.
    /// </summary>
    public class Portalable : IPortalable
    {
        public Action<IPortal, Transform2, Transform2> EnterPortal { get; set; }
        public bool IsPortalable { get; set; }
        Transform2 _transform = new Transform2();
        Transform2 _velocity = Transform2.CreateVelocity();

        public Portalable()
        {
        }

        public Portalable(Transform2 transform, Transform2 velocity)
        {
            IsPortalable = true;
            SetTransform(transform);
            SetVelocity(velocity);
        }

        public Transform2 GetTransform()
        {
            return _transform.ShallowClone();
        }

        public Transform2 GetVelocity()
        {
            return _velocity.ShallowClone();
        }

        public void SetTransform(Transform2 transform)
        {
            _transform = transform.ShallowClone();
        }

        public void SetVelocity(Transform2 velocity)
        {
            _velocity = velocity.ShallowClone();
        }
    }
}
