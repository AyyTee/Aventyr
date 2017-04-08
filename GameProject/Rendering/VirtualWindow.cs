using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    public class VirtualWindow : IVirtualWindow
    {
        readonly ResourceController _resourceController;
        public Size CanvasSize { get; set; }
        public Vector2 CanvasPosition { get; set; }
        public IInput Input => _resourceController.Input;
        public Dictionary<string, ITexture> Textures => _resourceController.Renderer.Textures;
        public List<IRenderLayer> Layers { get; private set; } = new List<IRenderLayer>();
        public float UpdatesPerSecond => 60;
        public float RendersPerSecond => 60;

        public VirtualWindow(ResourceController resourceController)
        {
            _resourceController = resourceController;
            _resourceController.Renderer.Windows.Add(this);
        }
    }
}
