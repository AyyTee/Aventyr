using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Portals;

namespace Game.Rendering
{
    public class Layer : IRenderLayer
    {
        public bool RenderPortalViews { get; set; } = true;
        public ICamera2 Camera { get; set; }
        public List<IPortal> Portals { get; set; }
        public List<IRenderable> Renderables { get; set; }

        public Layer()
        {
        }

        public Layer(IScene scene)
        {
            Camera = scene.GetCamera();
            Portals = scene.GetAll().OfType<IPortal>().ToList();
            Renderables = scene.GetAll().OfType<IRenderable>().ToList();
        }
    }
}
