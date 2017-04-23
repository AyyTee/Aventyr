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
        public List<IPortal> Portals { get; set; } = new List<IPortal>();
        public List<IRenderable> Renderables { get; set; } = new List<IRenderable>();

        public Layer()
        {
        }

        public Layer(IScene scene)
        {
            Camera = scene.GetAll().OfType<ICamera2>().First();
            Portals = scene.GetAll().OfType<IPortal>().ToList();
            Renderables = scene.GetAll().OfType<IRenderable>().ToList();
        }
    }
}
