using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Portals
{
    public interface IPortal : IDeepClone, ISceneObject, IPortalCommon
    {
        IPortal Linked { get; set; }
        bool OneSided { get; }
    }
}
