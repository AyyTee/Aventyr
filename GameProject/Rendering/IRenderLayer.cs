using System.Collections.Generic;
using Game.Portals;
using OpenTK;

namespace Game.Rendering
{
    public interface IRenderLayer
    {
        List<IRenderable> Renderables { get; }
        List<IPortal> Portals { get; }
        ICamera2 Camera { get; }
        bool RenderPortalViews { get; }
    }
}
