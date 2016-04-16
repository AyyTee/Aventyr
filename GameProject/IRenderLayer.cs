using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IRenderLayer
    {
        List<IRenderable> GetRenderList();
        List<Portal> GetPortalList();
        ICamera2 GetCamera();
    }
}
