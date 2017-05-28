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
        public List<IPortalRenderable> Portals { get; set; } = new List<IPortalRenderable>();
        public List<IRenderable> Renderables { get; set; } = new List<IRenderable>();
        /// <summary>
        /// If true, drawing order depends on depth. Otherwise drawing order is done first to last.
        /// </summary>
        public bool DepthTest { get; set; } = true;

        public Layer()
        {
        }

        public Layer(IScene scene)
        {
            Camera = scene.GetAll().OfType<ICamera2>().FirstOrDefault();
            Portals = scene.GetAll().OfType<IPortalRenderable>().ToList();
            Renderables = scene.GetAll().OfType<IRenderable>().ToList();
        }
    }
}
