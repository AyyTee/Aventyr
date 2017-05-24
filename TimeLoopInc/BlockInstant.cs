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
        Transform2i _transform;
        public Transform2i Transform
        {
            get { return _transform.ShallowClone(); }
            set { _transform = value.ShallowClone(); }
        }
        public Vector2i PreviousVelocity { get; set; }
        public bool IsPushed { get; set; }

        public BlockInstant(Vector2i position)
        {
            Transform = new Transform2i(position);
        }

        public IGridEntityInstant DeepClone() => (BlockInstant)MemberwiseClone();
    }
}
