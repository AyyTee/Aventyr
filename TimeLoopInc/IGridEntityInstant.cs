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
        Vector2i Position { get; set; }
        Vector2i PreviousPosition { get; set; }
    }

    public static class IGridEntityInstantEx
    {
        public static void SetPosition(this IGridEntityInstant entity, Vector2i position)
        {
            entity.PreviousPosition = entity.Position;
            entity.Position = position;
        }

        public static void AddPosition(this IGridEntityInstant entity, Vector2i offset)
        {
            entity.PreviousPosition = entity.Position;
            entity.Position += offset;
        }
    }
}
