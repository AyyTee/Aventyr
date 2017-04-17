using Game.Common;
using System.Collections.Generic;

namespace Game.Rendering
{
    public interface IVirtualWindow
    {
        Point CanvasPosition { get; }
        Size CanvasSize { get; }
        IInput Input { get; }
        TextureAssets Textures { get; }
        List<IRenderLayer> Layers { get; }
        FontAssets Fonts { get; }
        float UpdatesPerSecond { get; }
        float RendersPerSecond { get; }
    }
}