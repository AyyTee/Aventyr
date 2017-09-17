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
        bool _dirty = true;
        Vector3d _position;
        Quaterniond _rotation = new Quaterniond(0, 0, 1, 0);
        Vector3d _scale = new Vector3d(1, 1, 1);

        public Quaterniond Rotation {
            get => _rotation;
            set { _rotation = value; _dirty = true; }
        }
        public Vector3d Scale
        {
            get => _scale;
            set
            {
                _dirty = true;
                _scale = value;
            }
        }
        public Vector3d Position {
            get => _position;
            set { _position = value; _dirty = true; }
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

        public Transform3d(Vector3d position, Vector3d scale, Quaterniond rotation)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
        }

        public Transform3d ShallowClone()
        {
            return new Transform3d
            {
                Rotation = new Quaterniond(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W),
                Position = new Vector3d(Position),
                Scale = new Vector3d(Scale),
            };
        }

        public Matrix4d GetMatrix()
        {
            if (_dirty)
            {
#pragma warning disable
                _matrix = Matrix4d.Scale(Scale) * Matrix4d.CreateFromAxisAngle(new Vector3d(Rotation.X, Rotation.Y, Rotation.Z), Rotation.W) * Matrix4d.CreateTranslation(Position);
#pragma warning restore
                _dirty = false;
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
