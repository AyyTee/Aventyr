using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class PlayerInstant : IGridEntityInstant
    {
        public Vector2i Position { get; set; }
        public Vector2i PreviousPosition { get; set; }

        public PlayerInstant(Vector2i position)
        {
            Position = position;
            PreviousPosition = position;
        }

        public IGridEntityInstant DeepClone() => (PlayerInstant)MemberwiseClone();
    }
}
