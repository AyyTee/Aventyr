using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game.Common
{
    [DataContract]
    public class RectangleI
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
    }
}
