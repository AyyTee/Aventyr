﻿using System;
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
        public IPortalCommon Parent { get; set; }
        public List<IPortalCommon> Children { get; private set; } = new List<IPortalCommon>();
        public readonly IPortal Portal;
        public Transform2 WorldTransform;
        public Transform2 WorldVelocity;

        public IPortal Linked { get; set; }

        public bool OneSided { get { return Portal.OneSided; } }
        
        public PortalPath Path { get { return Portal.Path; } set { Portal.Path = value; } }

        public IScene Scene { get { return Portal.Scene; } }

        public ProxyPortal(IPortal portal)
            : this(portal, portal.WorldTransformPrevious, portal.WorldVelocityPrevious)
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

        public Transform2 GetWorldTransform(bool ignorePortals = false)
        {
            return WorldTransform.ShallowClone();
        }

        public Transform2 GetWorldVelocity(bool ignorePortals = false)
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

        public Transform2 GetTransform()
        {
            return Portal.GetTransform();
        }

        public Transform2 GetVelocity()
        {
            return Portal.GetVelocity();
        }
    }
}
