using FarseerPhysics.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IActor
    {
        Body Body { get; }
        void Remove();
        Transform2 GetWorldTransform();
        Transform2 GetWorldVelocity();
        Transform2 GetTransform();
        Transform2 GetVelocity();
    }
}
