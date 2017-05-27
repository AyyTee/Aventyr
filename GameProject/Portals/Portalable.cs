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
        public Transform2 WorldTransform { get; set; }
        public Transform2 WorldVelocity { get; set; }
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

        public Transform2 GetTransform() => Transform;

        public Transform2 GetVelocity() => Velocity;

        public void SetTransform(Transform2 transform) => Transform = transform;

        public void SetVelocity(Transform2 velocity) => Velocity = velocity;

        public List<IPortal> GetPortalChildren() => new List<IPortal>();
    }
}
