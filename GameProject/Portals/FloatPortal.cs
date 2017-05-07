using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Game.Common;
using Game.Serialization;
using Game.Rendering;

namespace Game.Portals
{
    [DataContract, DebuggerDisplay("FloatPortal " + nameof(Name))]
    public class FloatPortal : SceneNode, IPortal, IPortalable, ISceneObject
    {
        [DataMember]
        public IPortal Linked { get; set; }
        IPortalRenderable IPortalRenderable.Linked => Linked;
        /// <summary>
        /// If OneSided is true then the portal can only be viewed through it's front side.
        /// Entities can still travel though the portal in both directions however.
        /// </summary>
        [DataMember]
        public bool OneSided { get; set; }
        [DataMember]
        public Transform2 Transform { get; set; } = new Transform2();
        [DataMember]
        public Transform2 Velocity { get; set; } = Transform2.CreateVelocity();

        public Action<EnterCallbackData, Transform2, Transform2> EnterPortal { get; set; }

        public FloatPortal(Scene scene)
            : this(scene, new Transform2())
        {
        }

        public FloatPortal(Scene scene, Transform2 transform)
            : base(scene)
        {
            SetTransform(transform);
        }

        public override IDeepClone ShallowClone()
        {
            FloatPortal clone = new FloatPortal(Scene);
            ShallowClone(clone);
            return clone;
        }

        protected void ShallowClone(FloatPortal destination)
        {
            base.ShallowClone(destination);
            destination.OneSided = OneSided;
            destination.Linked = Linked;
        }

        public override void UpdateRefs(IReadOnlyDictionary<IDeepClone, IDeepClone> cloneMap)
        {
            base.UpdateRefs(cloneMap);
            if (Linked != null && cloneMap.ContainsKey(Linked))
            {
                Linked = (IPortal)cloneMap[Linked];
            }
            else
            {
                Linked = null;
            }
        }

        public override Transform2 GetVelocity()
        {
            return Velocity.ShallowClone();
        }

        public override Transform2 GetTransform()
        {
            return Transform.ShallowClone();
        }

        public override void SetTransform(Transform2 transform)
        {
            Transform = transform.ShallowClone();
            base.SetTransform(transform);
        }

        public override void SetVelocity(Transform2 velocity)
        {
            Velocity = velocity.ShallowClone();
            base.SetVelocity(velocity);
        }

        public List<IPortal> GetPortalChildren()
        {
            return new List<IPortal>();
        }
    }
}
