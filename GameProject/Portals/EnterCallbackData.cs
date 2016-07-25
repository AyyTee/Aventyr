using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Portals
{
    /// <summary>
    /// Callback data often provided when entering a portal.
    /// </summary>
    public struct EnterCallbackData
    {
        public IPortal EntrancePortal;
        public IPortalable Instance;
        public double PortalT;

        public EnterCallbackData(IPortal entrancePortal, IPortalable instance, double portalT)
        {
            EntrancePortal = entrancePortal;
            Instance = instance;
            PortalT = portalT;
        }
    }
}
