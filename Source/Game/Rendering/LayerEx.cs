using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    public static class LayerEx
    {
        public static Layer FromScene(IScene scene)
        {
            return new Layer
            {
                Camera = scene.GetAll().OfType<ICamera2>().FirstOrDefault(),
                Portals = scene.GetAll().OfType<IPortalRenderable>().ToList(),
                Renderables = scene.GetAll().OfType<IRenderable>().ToList()
            };
        }
    }
}
