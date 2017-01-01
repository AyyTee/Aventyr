using System.Collections.Generic;
using Game.Common;
using Game.Models;

namespace Game.Rendering
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
