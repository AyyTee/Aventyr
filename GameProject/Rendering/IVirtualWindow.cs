using Game.Common;
using System.Collections.Generic;

namespace Game.Rendering
{
    public interface IVirtualWindow
    {
        Point CanvasPosition { get; }
        Size CanvasSize { get; }
        IInput Input { get; }
        Dictionary<string, ITexture> Textures { get; }
        List<IRenderLayer> Layers { get; }
        Dictionary<string, Font> Fonts { get; }
        float UpdatesPerSecond { get; }
        float RendersPerSecond { get; }
    }
}