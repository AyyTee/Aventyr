using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Common
{
    public interface IGetWorldTransformVelocity
    {
        Transform2 WorldTransform { get; }
        Transform2 WorldVelocity { get; }
    }
}
