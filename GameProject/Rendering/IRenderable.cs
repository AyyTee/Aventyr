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
        Transform2 GetWorldTransform();
        Transform2 GetWorldVelocity();
        bool Visible { get; }
        bool DrawOverPortals { get; }
        bool IsPortalable { get; }
    }
}
