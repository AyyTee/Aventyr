using System.Collections.Generic;
using System.Drawing;

namespace Game.Rendering
{
    public interface IVirtualWindow
    {
        Point CanvasPosition { get; }
        Size CanvasSize { get; }
        IInput Input { get; }
        Dictionary<string, ITexture> Textures { get; }
        List<IRenderLayer> Layers { get; }
        FontRenderer FontRenderer { get; }
        float UpdatesPerSecond { get; }
        float RendersPerSecond { get; }
    }
}