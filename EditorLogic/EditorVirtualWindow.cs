using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using OpenTK;
using Game.Common;

namespace EditorLogic
{
    public class EditorVirtualWindow : IVirtualWindow
    {
        public Vector2i CanvasPosition => new Vector2i();
        public Vector2i CanvasSize { get; set; }

        public IInput Input { get; private set; }

        public List<IRenderLayer> Layers { get; private set; } = new List<IRenderLayer>();

        public TextureAssets Textures { get; private set; }

        public IRenderer Renderer { get; private set; }

        public FontAssets Fonts { get; private set; }

        readonly GLControl _glControl;

        public float UpdatesPerSecond => 60;
        public float RendersPerSecond => 60;

        public EditorVirtualWindow(GLControl glControl, IRenderer renderer, IInput input, TextureAssets textures)
        {
            _glControl = glControl;
            CanvasSize = (Vector2i)_glControl.ClientSize;
            Renderer = renderer;
            Input = input;
            Textures = textures;

            renderer.Windows.Add(this);
        }
    }
}
