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
        public int TimeOffset { get; set; }
        public TimePortal Linked { get; set; }

        public TimePortal(Vector2i position, int timeOffset)
        {
            Position = position;
            TimeOffset = timeOffset;
        }

        public TimePortal DeepClone() => (TimePortal)MemberwiseClone();
    }
}
