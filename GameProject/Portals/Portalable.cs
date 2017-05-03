using System;
using System.Collections.Generic;
using Game.Common;

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
        public PortalPath Path { get; set; } = new PortalPath();
        Transform2 _worldTransformPrevious;
        public Transform2 WorldTransform
        {
            get { return _worldTransformPrevious?.ShallowClone(); }
            set { _worldTransformPrevious = value?.ShallowClone(); }
        }
        Transform2 _worldVelocityPrevious;
        public Transform2 WorldVelocity
        {
            get { return _worldVelocityPrevious?.ShallowClone(); }
            set { _worldVelocityPrevious = value?.ShallowClone(); }
        }
        public IPortalCommon Parent { get; set; }
        public List<IPortalCommon> Children { get; } = new List<IPortalCommon>();
        public Scene Scene { get; private set; }
        IScene IPortalCommon.Scene => Scene;

        public Portalable(Scene scene)
        {
            Scene = scene;
        }

        public Portalable(Scene scene, Transform2 transform, Transform2 velocity)
            : this(scene)
        {
            SetTransform(transform);
            SetVelocity(velocity);
        }

        public Transform2 GetTransform() => Transform.ShallowClone();

        public Transform2 GetVelocity() => Velocity.ShallowClone();

        public void SetTransform(Transform2 transform) => Transform = transform.ShallowClone();

        public void SetVelocity(Transform2 velocity) => Velocity = velocity.ShallowClone();

        public List<IPortal> GetPortalChildren() => new List<IPortal>();
    }
}
