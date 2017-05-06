using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;

namespace TimeLoopInc
{
    public class Block : IGridEntity
    {
        public int Size { get; }
        public Vector2i StartPosition { get; }
        public int StartTime { get; }
        public int EndTime { get; set; }

        public Block(Vector2i startPosition, int startTime, int size = 1)
        {
            StartPosition = startPosition;
            StartTime = startTime;
            Size = size;
        }

        public IGridEntity DeepClone() => (Block)MemberwiseClone();
    }
}
