using Game.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    public class VirtualWindow : IVirtualWindow
    {
        readonly ResourceController _resourceController;
        public Size CanvasSize { get; set; }
        public Point CanvasPosition { get; set; }
        public IInput Input => _resourceController.Input;
        public TextureAssets Textures => _resourceController.Textures;
        public List<IRenderLayer> Layers { get; private set; } = new List<IRenderLayer>();
        public FontAssets Fonts => _resourceController.Fonts;
        public float UpdatesPerSecond => 60;
        public float RendersPerSecond => 60;

        public VirtualWindow(ResourceController resourceController)
        {
            _resourceController = resourceController;
            _resourceController.Renderer.Windows.Add(this);
        }
    }
}
