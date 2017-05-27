using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Common
{
    public partial class Transform2d
    {
        public static explicit operator Transform2d(Transform2 t)
        {
            return new Transform2d((Vector2d)t.Position, t.Rotation, t.Size, t.MirrorX);
        }

        public static explicit operator Transform2(Transform2d t)
        {
            return new Transform2((Vector2)t.Position, (float)t.Rotation, (float)t.Size, t.MirrorX);
        }
    }

    public partial class Transform3d
    {
        public static explicit operator Transform3d(Transform3 t)
        {
            return new Transform3d(
                (Vector3d)t.Position, 
                (Vector3d)t.Scale, 
                new Quaterniond((Vector3d)t.Rotation.Xyz, t.Rotation.W), 
                t.FixedScale);
        }

        public static explicit operator Transform3(Transform3d t)
        {
            return new Transform3(
                (Vector3)t.Position, 
                (Vector3)t.Scale,
                new Quaternion((Vector3)t.Rotation.Xyz, (float)t.Rotation.W), 
                t.FixedScale);
        }
    }
}
