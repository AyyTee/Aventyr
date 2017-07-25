using System;
using Equ;
using Game.Common;
using OpenTK;

namespace TimeLoopInc.Editor
{
    public class PortalBuilder : MemberwiseEquatable<PortalBuilder>
    {
        public Vector2i Position { get; }
        public GridAngle Direction { get; }
        public Vector2 Center => (Vector2)Position + ((Vector2)Direction.Vector + Vector2.One) / 2;

        public PortalBuilder(Vector2i position, GridAngle direction)
        {
            Position = position;
            Direction = direction;
        }
    }
}
