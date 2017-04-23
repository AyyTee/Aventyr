using Game.Common;
using OpenTK;
using System.Collections.Generic;

namespace Game.Rendering
{
    public interface IVirtualWindow
    {
        Vector2i CanvasPosition { get; }
        Vector2i CanvasSize { get; }
        IInput Input { get; }
        TextureAssets Textures { get; }
        List<IRenderLayer> Layers { get; }
        FontAssets Fonts { get; }
        float UpdatesPerSecond { get; }
        float RendersPerSecond { get; }
    }
}