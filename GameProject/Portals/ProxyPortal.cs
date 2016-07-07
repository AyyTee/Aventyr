using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Portals
{
    /// <summary>
    /// Represents a portal but with a different transform and velocity.
    /// </summary>
    public class ProxyPortal : IPortal
    {
        public Transform2 WorldTransformPrevious { get; set; }
        public readonly IPortal Portal;
        public Transform2 WorldTransform;
        public Transform2 WorldVelocity;

        public IPortal Linked { get; set; }

        public bool OneSided { get { return Portal.OneSided; } }
        
        public PortalPath Path { get { return Portal.Path; } }

        public ProxyPortal(IPortal portal)
            : this(portal, portal.GetWorldTransform(), portal.GetWorldVelocity())
        {
        }

        public ProxyPortal(IPortal portal, Transform2 transform, Transform2 velocity)
        {
            Debug.Assert(portal != null);
            Portal = portal;
            WorldTransform = transform != null ? transform : new Transform2();
            WorldVelocity = velocity != null ? velocity : new Transform2();
            Linked = portal.Linked;
        }

        public HashSet<IDeepClone> GetCloneableRefs()
        {
            return Portal.GetCloneableRefs();
        }

        public Transform2 GetWorldTransform()
        {
            return WorldTransform.ShallowClone();
        }

        public Transform2 GetWorldVelocity()
        {
            return WorldVelocity.ShallowClone();
        }

        public IDeepClone ShallowClone()
        {
            return new ProxyPortal(Portal, WorldTransform, WorldVelocity);
        }

        public void UpdateRefs(IReadOnlyDictionary<IDeepClone, IDeepClone> cloneMap)
        {
            Portal.UpdateRefs(cloneMap);
        }
    }
}
