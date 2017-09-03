using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Common
{
    public partial class Transform3d
    {
        Matrix4d _matrix;
        bool _matrixUpdate = true;
        Vector3d _position;
        Quaterniond _rotation = new Quaterniond(0, 0, 1, 0);
        Vector3d _scale = new Vector3d(1, 1, 1);

        public bool FixedScale { get; private set; }

        public Quaterniond Rotation {
            get { return _rotation; }
            set { _rotation = value; _matrixUpdate = true; }
        }
        public Vector3d Scale
        {
            get { return _scale; }
            set
            {
                if (FixedScale)
                {
                    DebugEx.Assert(Math.Abs(value.X) == Math.Abs(value.Y) && Math.Abs(value.Y) == Math.Abs(value.Z), "Transforms with fixed scale cannot have non-uniform scale.");
                }
                _matrixUpdate = true;
                _scale = value;
            }
        }
        public Vector3d Position {
            get { return _position; }
            set { _position = value; _matrixUpdate = true; }
        }

        public Transform3d()
        {
        }

        public Transform3d(Vector3d position)
        {
            Position = position;
        }

        public Transform3d(Vector3d position, Vector3d scale)
        {
            Position = position;
            Scale = scale;
        }

        public Transform3d(Vector3d position, Vector3d scale, Quaterniond rotation, bool fixedScale = false)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
            FixedScale = fixedScale;
        }

        public Transform3d ShallowClone()
        {
            return new Transform3d
            {
                Rotation = new Quaterniond(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W),
                Position = new Vector3d(Position),
                Scale = new Vector3d(Scale),
                FixedScale = FixedScale
            };
        }

        public Matrix4d GetMatrix()
        {
            if (_matrixUpdate)
            {
                _matrix = Matrix4d.Scale(Scale) * Matrix4d.CreateFromAxisAngle(new Vector3d(Rotation.X, Rotation.Y, Rotation.Z), Rotation.W) * Matrix4d.CreateTranslation(Position);
                _matrixUpdate = false;
            }
            return _matrix;
        }

        public static Transform3d Lerp(Transform3d a, Transform3d b, float t)
        {
            return new Transform3d
            {
                Position = Vector3d.Lerp(a.Position, b.Position, t),
                Scale = Vector3d.Lerp(a.Scale, b.Scale, t),
                Rotation = Quaterniond.Slerp(a.Rotation, b.Rotation, t)
            };
        }
    }
}
