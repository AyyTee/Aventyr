using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    public interface IRenderWindow
    {
        Vector2i CanvasPosition { get; }
        Vector2i CanvasSize { get; }
        List<IRenderLayer> Layers { get; }
        float RendersPerSecond { get; }
    }
}
