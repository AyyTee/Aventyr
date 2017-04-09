using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using System.Drawing;
using OpenTK;

namespace EditorLogic
{
    public class EditorVirtualWindow : IVirtualWindow
    {
        public Size CanvasSize { get; set; }

        public IInput Input { get; private set; }

        public List<IRenderLayer> Layers { get; private set; } = new List<IRenderLayer>();

        public float RendersPerSecond => 60;

        public Dictionary<string, ITexture> Textures => Renderer.Textures;

        public IRenderer Renderer { get; private set; }

        readonly GLControl _glControl;

        public float UpdatesPerSecond => 60;

        public EditorVirtualWindow(GLControl glControl, IRenderer renderer, IInput input)
        {
            _glControl = glControl;
            CanvasSize = _glControl.ClientSize;
            Renderer = renderer;
            Input = input;
        }
    }
}
