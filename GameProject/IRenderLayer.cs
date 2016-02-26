using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IRenderLayer
    {
        List<Entity> GetEntityList();
        List<Portal> GetPortalList();
        Camera2 GetCamera();
    }
}
