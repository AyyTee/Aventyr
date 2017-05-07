using System.Collections.Generic;
using Game.Portals;
using OpenTK;

namespace Game.Rendering
{
    public interface IRenderLayer
    {
        List<IRenderable> Renderables { get; }
        List<IPortalRenderable> Portals { get; }
        ICamera2 Camera { get; }
        bool RenderPortalViews { get; }
    }

    public static class IRenderLayerEx
    {
        public static void DrawText(this IRenderLayer layer, Font font, Vector2 position, string text)
        {
            layer.Renderables.Add(new TextEntity(font, position, text));
        }
    }
}
