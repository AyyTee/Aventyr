using System.Collections.Generic;
using Game.Common;
using Game.Models;

namespace Game.Rendering
{
    public interface IRenderable : IGetWorldTransformVelocity
    {
        List<Model> GetModels();
        bool Visible { get; }
        bool DrawOverPortals { get; }
        bool IsPortalable { get; }
    }
}
