using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    [DataContract]
    public class Layer : IRenderLayer
    {
        [DataMember]
        public bool RenderPortalViews { get; set; } = true;
        [DataMember]
        public ICamera2 Camera { get; set; }
        [DataMember]
        public List<IPortalRenderable> Portals { get; set; } = new List<IPortalRenderable>();
        [DataMember]
        public List<IRenderable> Renderables { get; set; } = new List<IRenderable>();
        [DataMember]
        public float MotionBlurFactor { get; set; } = 1 / 12f;
        /// <summary>
        /// If true, drawing order depends on depth. Otherwise drawing order is done first to last.
        /// </summary>
        [DataMember]
        public bool DepthTest { get; set; } = true;

        public Layer()
        {
        }
    }
}
