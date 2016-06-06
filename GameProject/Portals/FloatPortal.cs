using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract, DebuggerDisplay("FloatPortal {Name}")]
    public class FloatPortal : SceneNode, IPortal, ITransformable2
    {
        [DataMember]
        public IPortal Linked { get; set; }
        /// <summary>
        /// If OneSided is true then the portal can only be viewed through it's front side.
        /// Entities can still travel though the portal in both directions however.
        /// </summary>
        [DataMember]
        public bool OneSided { get; set; }
        [DataMember]
        public bool IsMirrored { get { return GetTransform().MirrorX; } }
        [DataMember]
        Transform2 _transform = new Transform2();
        public override bool IgnoreScale { get { return true; } }

        public const float EdgeMargin = 0.02f;
        public const float CollisionMargin = 0.1f;

        public FloatPortal(Scene scene)
            : base(scene)
        {
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
            return new Transform2();
        }

        public override Transform2 GetTransform()
        {
            return _transform.ShallowClone();
        }

        public void SetTransform(Transform2 transform)
        {
            _transform = transform.ShallowClone();
        }
    }
}
