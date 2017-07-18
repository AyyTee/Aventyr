using System;
using Game.Common;

namespace TimeLoopInc.Editor
{
    public class PortalBuilder
    {
        public Vector2i Position { get; }
        public GridAngle Direction { get; }

        public PortalBuilder(Vector2i position, GridAngle direction)
        {
            Position = position;
            Direction = direction;
        }
    }
}
