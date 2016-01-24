using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public class FloatPortal : Portal
    {
        public const float EdgeMargin = 0.02f;
        public const float CollisionMargin = 0.1f;

        public FloatPortal(Scene scene)
            : base(scene)
        {
        }

        public override SceneNode Clone(Scene scene)
        {
            FloatPortal clone = new FloatPortal(scene);
            Clone(clone);
            return clone;
        }

        public override Transform2D GetVelocity()
        {
            return new Transform2D();
        }

        public override Transform2D GetTransform()
        {
            if (IsMirrored)
            {
                return new Transform2D(new Vector2(), new Vector2(1, -1), 0);
            }
            return new Transform2D();
        }
    }
}
