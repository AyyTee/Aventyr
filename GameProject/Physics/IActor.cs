using FarseerPhysics.Dynamics;
using Game.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IActor : IWall, ISceneObject, IPortalable
    {
        Body Body { get; }
        void Remove();
        Transform2 GetWorldTransform();
        Transform2 GetWorldVelocity();
    }
}
