using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IEntity
    {
        List<Model> GetModels();
        List<ClipModel> GetClipModels(int depth);
        Transform2 GetWorldTransform();
        bool Visible { get; }
        bool DrawOverPortals { get; }
        bool IsPortalable { get; }
    }
}
