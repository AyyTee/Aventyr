using System.Collections.Generic;
using Game.Portals;

namespace Game.Rendering
{
    public interface IRenderLayer
    {
        List<IRenderable> GetRenderList();
        List<IPortal> GetPortalList();
        ICamera2 GetCamera();
    }
}
