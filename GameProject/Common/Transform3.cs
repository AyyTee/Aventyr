// This is generated from Transform3d.cs.
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Common
{
    public partial class Transform3
    {
        Matrix4 _matrix;
        bool _matrixUpdate = true;
        Vector3 _position;
        Quaternion _rotation = new Quaternion(0, 0, 1, 0);
        Vector3 _scale = new Vector3(1, 1, 1);

        public bool FixedScale { get; private set; }

        public Quaternion Rotation {
            get { return _rotation; }
            set { _rotation = value; _matrixUpdate = true; }
        }
        public Vector3 Scale
        {
            get { return _scale; }
            set
            {
                if (FixedScale)
                {
                    Debug.Assert(Math.Abs(value.X) == Math.Abs(value.Y) && Math.Abs(value.Y) == Math.Abs(value.Z), "Transforms with fixed scale cannot have non-uniform scale.");
                }
                _matrixUpdate = true;
                _scale = value;
            }
        }
        public Vector3 Position {
            get { return _position; }
            set { _position = value; _matrixUpdate = true; }
        }

        public Transform3()
        {
        }

        public Transform3(Vector3 position)
        {
            Position = position;
        }

        public Transform3(Vector3 position, Vector3 scale)
        {
            Position = position;
            Scale = scale;
        }

        public Transform3(Vector3 position, Vector3 scale, Quaternion rotation, bool fixedScale = false)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
            FixedScale = fixedScale;
        }

        public Transform3 ShallowClone()
        {
            return new Transform3
            {
                Rotation = new Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W),
                Position = new Vector3(Position),
                Scale = new Vector3(Scale),
                FixedScale = FixedScale
            };
        }

        public Matrix4 GetMatrix()
        {
            if (_matrixUpdate)
            {
                _matrix = Matrix4.Scale(Scale) * Matrix4.CreateFromAxisAngle(new Vector3(Rotation.X, Rotation.Y, Rotation.Z), Rotation.W) * Matrix4.CreateTranslation(Position);
                _matrixUpdate = false;
            }
            return _matrix;
        }

        public static Transform3 Lerp(Transform3 a, Transform3 b, float t)
        {
            return new Transform3
            {
                Position = Vector3.Lerp(a.Position, b.Position, t),
                Scale = Vector3.Lerp(a.Scale, b.Scale, t),
                Rotation = Quaternion.Slerp(a.Rotation, b.Rotation, t)
            };
        }
    }
}
