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
        Transform2i _transform;
        public Transform2i Transform
        {
            get { return _transform.ShallowClone(); }
            set { _transform = value.ShallowClone(); }
        }
        public Vector2i PreviousVelocity { get; set; }

        public PlayerInstant(Vector2i position)
        {
            Transform = new Transform2i(position);
        }

        public IGridEntityInstant DeepClone() => (PlayerInstant)MemberwiseClone();
    }
}
