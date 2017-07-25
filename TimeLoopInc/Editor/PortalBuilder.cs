using System;
using Equ;
using Game.Common;
using OpenTK;
using System.Runtime.Serialization;

namespace TimeLoopInc.Editor
{
    [DataContract]
    public class PortalBuilder : MemberwiseEquatable<PortalBuilder>
    {
        [DataMember]
        public Vector2i Position { get; }
        [DataMember]
        public GridAngle Direction { get; }
        public Vector2 Center => (Vector2)Position + ((Vector2)Direction.Vector + Vector2.One) / 2;

        public PortalBuilder(Vector2i position, GridAngle direction)
        {
            Position = position;
            Direction = direction;
        }
    }
}
