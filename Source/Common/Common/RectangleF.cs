using Equ;
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
    public struct RectangleF
    {
        [DataMember]
        public Vector2 Position { get; private set; }
        [DataMember]
        public Vector2 Size { get; private set; }

        public RectangleF(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }
    }
}
