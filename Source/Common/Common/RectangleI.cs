using Equ;
using Game.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game.Common
{
    [DataContract]
    public struct RectangleI
    {
        [DataMember]
        public Vector2i Position { get; private set; }
        [DataMember]
        public Vector2i Size { get; private set; }

        public RectangleI(Vector2i position, Vector2i size)
        {
            Position = position;
            Size = size;
        }

        public RectangleI With(Vector2i? position = null, Vector2i? size = null)
        {
            var clone = (RectangleI)MemberwiseClone();
            clone.Position = position ?? Position;
            clone.Size = size ?? Size;
            return clone;
        }

        public static explicit operator RectangleF(RectangleI v) => new RectangleF((Vector2)v.Position, (Vector2)v.Size);
        public static explicit operator RectangleI(RectangleF v) => new RectangleI((Vector2i)v.Position, (Vector2i)v.Size);
    }
}
