﻿using Game;
using Game.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using System.Runtime.Serialization;

namespace GameTests
{
    [DataContract]
    public class NodePortalable : SceneNode, IPortalable
    {
        [DataMember]
        public Transform2 Transform { get; set; } = new Transform2();
        [DataMember]
        public Transform2 Velocity { get; set; } = Transform2.CreateVelocity();
        public Action<EnterCallbackData, Transform2, Transform2> EnterPortal { get; set; }

        public NodePortalable(Scene scene)
            : base(scene)
        {
        }

        public List<IPortal> GetPortalChildren()
        {
            return Children.OfType<IPortal>().ToList();
        }

        public override Transform2 GetTransform() => Transform;

        public override Transform2 GetVelocity() => Velocity;

        public override void SetTransform(Transform2 transform)
        {
            Transform = transform;
            base.SetTransform(transform);
        }

        public override void SetVelocity(Transform2 velocity)
        {
            Velocity = velocity;
            base.SetVelocity(velocity);
        }
    }
}
