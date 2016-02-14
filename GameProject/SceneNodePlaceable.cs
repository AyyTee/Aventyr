using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public class SceneNodePlaceable : SceneNode, ITransform2
    {
        public delegate void _portalEnter(SceneNodePlaceable placeable, Portal portalEnter);
        public event _portalEnter PortalEnter;
        [DataMember]
        Transform2 _transform = new Transform2();
        [DataMember]
        Transform2 _velocity = new Transform2();
        /// <summary>
        /// Whether or not this entity will interact with portals when intersecting them
        /// </summary>
        [DataMember]
        public bool IsPortalable { get; set; }

        public SceneNodePlaceable(Scene scene)
            : base (scene)
        {
        }

        public override IDeepClone ShallowClone()
        {
            SceneNodePlaceable clone = new SceneNodePlaceable(Scene);
            ShallowClone(clone);
            return clone;
        }

        protected override void ShallowClone(SceneNode destination)
        {
            base.ShallowClone(destination);
            SceneNodePlaceable destinationCast = (SceneNodePlaceable)destination;
            destinationCast.SetTransform(GetTransform());
        }

        public virtual void SetTransform(Transform2 transform)
        {
            _transform = transform.Clone();
        }

        public void PortalEnterInvoke(Portal portalEnter)
        {
            if (PortalEnter != null)
            {
                PortalEnter(this, portalEnter);
            }
        }

        public override Transform2 GetVelocity()
        {
            return _velocity.Clone();
        }

        public void SetVelocity(Transform2 transform)
        {
            _velocity = transform.Clone();
        }

        public override Transform2 GetTransform()
        {
            return _transform.Clone();
        }
    }
}
