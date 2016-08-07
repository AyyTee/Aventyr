using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IRenderable
    {
        List<Model> GetModels();
        Transform2 GetWorldTransform(bool ignorePortals = false);
        Transform2 GetWorldVelocity(bool ignorePortals = false);
        bool Visible { get; }
        bool DrawOverPortals { get; }
        bool IsPortalable { get; }
    }
}
