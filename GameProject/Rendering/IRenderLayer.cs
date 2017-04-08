using System.Collections.Generic;
using Game.Portals;
using OpenTK;

namespace Game.Rendering
{
    public interface IRenderLayer
    {
        List<IRenderable> GetRenderList();
        List<IPortal> GetPortalList();
        ICamera2 GetCamera();
        Matrix3 ViewportOffset { get; }
    }
}
