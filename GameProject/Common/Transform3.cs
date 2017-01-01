using System;
using System.Diagnostics;
using OpenTK;

namespace Game.Common
{
    public class Transform3
    {
        private Matrix4 _matrix;
        private bool _matrixUpdate = true;
        private Vector3 _position = new Vector3();
        private Quaternion _rotation = new Quaternion(0, 0, 1, 0);
        private Vector3 _scale = new Vector3(1, 1, 1);

        public bool FixedScale { get; private set; }

        public Quaternion Rotation { get { return _rotation; } set { _rotation = value; _matrixUpdate = true;  } }
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
        public Vector3 Position { get { return _position; } set { _position = value; _matrixUpdate = true; } }

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

        public Transform3(Vector3 position, Vector3 scale, Quaternion rotation)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
        }

        public Transform3(Vector3 position, Vector3 scale, Quaternion rotation, bool fixedScale)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
            FixedScale = fixedScale;
        }

        public Transform3 ShallowClone()
        {
            Transform3 clone = new Transform3();
            clone.Rotation = new Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W);
            clone.Position = new Vector3(Position);
            clone.Scale = new Vector3(Scale);
            clone.FixedScale = FixedScale;
            return clone;
        }

        public Matrix4 GetMatrix()
        {
            if (_matrixUpdate)
            {
                _matrix = Matrix4.CreateScale(Scale) * Matrix4.CreateFromAxisAngle(new Vector3(Rotation.X, Rotation.Y, Rotation.Z), Rotation.W) * Matrix4.CreateTranslation(Position);
                _matrixUpdate = false;
            }
            return _matrix;
        }

        public static Transform3 Lerp(Transform3 a, Transform3 b, float t)
        {
            Transform3 c = new Transform3();
            c.Position = Vector3.Lerp(a.Position, b.Position, t);
            c.Scale = Vector3.Lerp(a.Scale, b.Scale, t);
            c.Rotation = Quaternion.Slerp(a.Rotation, b.Rotation, t);
            return c;
        }

        /// <summary>
        /// Projects a copy of Transform projected onto the XY-plane, the rotation simply uses the Quaternion's W (theta) value
        /// </summary>
        /*public Transform2 Get2D()
        {
            return new Transform2(new Vector2(Position.X, Position.Y), new Vector2(Scale.X, Scale.Y), Rotation.W);
        }*/

        /// <summary>
        /// Returns true if this has the same position, rotation, and scale as another Transform.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public bool AlmostEqual(Transform3 transform)
        {
            return Position == transform.Position && Scale == transform.Scale && Rotation == transform.Rotation;
        }
    }
}
