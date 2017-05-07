
using Game.Rendering;
using Game.Serialization;

namespace Game.Portals
{
    public interface IPortal : IDeepClone, IPortalCommon, IPortalRenderable
    {
        new IPortal Linked { get; set; }
    }
}
