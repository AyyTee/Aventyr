using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    public class Layer : IRenderLayer
    {
        public bool RenderPortalViews { get; set; } = true;
        public ICamera2 Camera { get; set; }
        public List<IPortalRenderable> Portals { get; set; } = new List<IPortalRenderable>();
        public List<IRenderable> Renderables { get; set; } = new List<IRenderable>();
        public float MotionBlurFactor { get; set; } = 1 / 12f;
        /// <summary>
        /// If true, drawing order depends on depth. Otherwise drawing order is done first to last.
        /// </summary>
        public bool DepthTest { get; set; } = true;

        public Layer()
        {
        }
    }
}
