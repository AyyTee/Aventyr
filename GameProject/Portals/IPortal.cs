using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Portals
{
    public interface IPortal : IDeepClone, ISceneObject
    {
        IPortal Linked { get; }
        Transform2 GetWorldTransform();
        Transform2 GetWorldVelocity();
        bool OneSided { get; }
        PortalPath Path { get; }
        Transform2 WorldTransformPrevious { get; set; }
    }
}
