
using Game.Serialization;

namespace Game.Portals
{
    public interface IPortal : IDeepClone, IPortalCommon
    {
        IPortal Linked { get; set; }
        bool OneSided { get; }
    }
}
