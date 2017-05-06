using Game.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.Common;
using Game.Serialization;
using Game.Rendering;

namespace TimeLoopInc
{
    public class TimePortal
    {
        public Vector2i Position { get; }
        public int TimeOffset { get; private set; }
        public TimePortal Linked { get; private set; }
        public Direction Direction { get; }

        public TimePortal(Vector2i position, Direction direction)
        {
            Position = position;
            Direction = direction;
        }

        public TimePortal DeepClone() => (TimePortal)MemberwiseClone();

        public void SetLinked(TimePortal p1)
        {
            Linked = p1;
            p1.Linked = this;
            p1.TimeOffset = -TimeOffset;
        }

        public void SetTimeOffset(int timeOffset)
        {
            TimeOffset = timeOffset;
            Linked.TimeOffset = -timeOffset;
        }
    }
}
