using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class BlockInstant : IGridEntityInstant
    {
        public Transform2i Transform { get; set; }
        public Vector2i PreviousVelocity { get; set; }
        public bool IsPushed { get; set; }

        public BlockInstant(Transform2i transform)
        {
            Transform = transform;
        }

        public IGridEntityInstant DeepClone() => (BlockInstant)MemberwiseClone();
    }
}
