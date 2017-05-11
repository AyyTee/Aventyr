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
        Vector2i StartPosition { get; }
        int StartTime { get; }
        int EndTime { get; }

        IGridEntityInstant CreateInstant(); 
    }
}
