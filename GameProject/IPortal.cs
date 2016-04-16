using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IPortal : IDeepClone
    {
        IPortal Linked { get; }
        Transform2 GetWorldTransform();
        Transform2 GetWorldVelocity();
        bool OneSided { get; }
        bool IsMirrored { get; }
    }
}
