using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Portals
{
    public class PortalPath : IShallowClone<PortalPath>
    {
        public List<IPortal> Portals = new List<IPortal>();

        public PortalPath()
        {
        }

        public Transform2 GetPortalTransform()
        {
            Transform2 t = new Transform2();
            for (int i = 0; i < Portals.Count; i++)
            {
                Transform2 portal = Portal.GetLinkedTransform(Portals[i]);
                //t.MirrorX = !t.MirrorX;
                t = t.Transform(portal);
            }
            return t;
        }

        public void RemoveAfter(int index)
        {
            Portals.RemoveRange(index, Portals.Count - index);
        }

        public PortalPath ShallowClone()
        {
            PortalPath clone = new PortalPath();
            clone.Portals = new List<IPortal>(Portals);
            return clone;
        }

        /// <summary>
        /// Add a portal to the portal list.  
        /// If the portal is the exit for the previous portal then the two are removed from the list.
        /// </summary>
        public void Enter(IPortal portal)
        {
            ProxyPortal proxyPortal = portal as ProxyPortal;
            if (proxyPortal != null)
            {
                portal = proxyPortal.Portal;
            }
            //If the last portal in the path is the same as this portal's exit portal then the two portals negate eachother.
            if (Portals.Count != 0 && portal == Portals.Last().Linked)
            {
                Portals.RemoveAt(Portals.Count - 1);
            }
            else
            {
                Portals.Add(portal);
            }
        }
    }
}
