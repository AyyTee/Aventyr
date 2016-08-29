using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Portals
{
    public class ProxyPortalable : IPortalable
    {
        public readonly IPortalable Portalable;
        /// <summary>
        /// Included as a hack to make the SimulationStep code work for physics bodies.
        /// </summary>
        public Transform2 TrueVelocity = new Transform2();
        public Transform2 Transform { get; set; } = new Transform2();
        public Transform2 Velocity { get; set; } = Transform2.CreateVelocity();
        public PortalPath Path { get; set; }
        public IPortalCommon Parent { get; set; }
        public List<IPortalCommon> Children { get; private set; } = new List<IPortalCommon>();
        Transform2 _worldTransformPrevious = new Transform2();
        public Transform2 WorldTransformPrevious
        {
            get { return _worldTransformPrevious.ShallowClone(); }
            set { _worldTransformPrevious = value.ShallowClone(); }
        }
        Transform2 _worldVelocityPrevious = Transform2.CreateVelocity();
        public Transform2 WorldVelocityPrevious
        {
            get { return _worldVelocityPrevious.ShallowClone(); }
            set { _worldVelocityPrevious = value.ShallowClone(); }
        }
        public Action<EnterCallbackData, Transform2, Transform2> EnterPortal { get; set; }
        public bool IsPortalable { get { return Portalable.IsPortalable; } }

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

        public List<IPortal> GetPortalChildren()
        {
            return Portalable.GetPortalChildren();
        }
    }
}
