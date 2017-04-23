using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.Common;

namespace Game
{
    public class FakeVirtualWindow : IVirtualWindow
    {
        public Vector2i CanvasPosition => new Vector2i();
        public Vector2i CanvasSize => new Vector2i(300, 200);

        public FakeInput Input { get; private set; } = new FakeInput();
        IInput IVirtualWindow.Input => Input;

        public List<IRenderLayer> Layers { get; private set; } = new List<IRenderLayer>();
        public TextureAssets Textures => null;
        public FontAssets Fonts => null;

        public float UpdatesPerSecond => 60;
        public float RendersPerSecond => 60;
    }
}
