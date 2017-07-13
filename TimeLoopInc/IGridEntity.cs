using Game.Common;
using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Models;
using Game.Serialization;

namespace TimeLoopInc
{
    public interface IGridEntity : IDeepClone<IGridEntity>
    {
        Transform2i StartTransform { get; }
        Vector2i PreviousVelocity { get; }
        Transform2i PreviousTransform { get; }
        int StartTime { get; }

        IGridEntityInstant CreateInstant();
    }
}
