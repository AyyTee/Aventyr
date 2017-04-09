using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using System.Drawing;

namespace Game
{
    public class FakeVirtualWindow : IVirtualWindow
    {
        public Size CanvasSize => new Size(300, 200);

        public IInput Input { get; private set; } = new FakeInput();

        public List<IRenderLayer> Layers { get; private set; } = new List<IRenderLayer>();

        public float RendersPerSecond => 60;

        public Dictionary<string, ITexture> Textures => new Dictionary<string, ITexture>();

        public float UpdatesPerSecond => 60;
    }
}
