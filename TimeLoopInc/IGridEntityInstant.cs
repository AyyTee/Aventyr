using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public interface IGridEntityInstant : IDeepClone<IGridEntityInstant>
    {
        Transform2i Transform { get; set; }
        Vector2i PreviousVelocity { get; set; }
    }
}
