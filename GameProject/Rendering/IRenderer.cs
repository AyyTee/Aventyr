using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    public interface IRenderer
    {
        Dictionary<string, ITexture> Textures { get; }
        Dictionary<string, Shader> Shaders { get; }
        List<IVirtualWindow> Windows { get; }
        void Render();
    }
}
