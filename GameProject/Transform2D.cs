using OpenTK;
using System;
using System.Diagnostics;

namespace Game
{
    public class Transform2D
    {
        private Matrix4 Matrix;
        private bool MatrixUpdate = true;
        private Vector2 _position = new Vector2();
        private float _rotation = 0;
        private Vector2 _scale = new Vector2(1, 1);
        private bool _fixedScale = false;

        public bool FixedScale { get { return _fixedScale; } set { _fixedScale = value; } }

        public float Rotation { get { return _rotation; } set { _rotation = value; MatrixUpdate = true; } }
        public Vector2 Scale 
        { 
            get { return _scale; } 
            set 
            { 
                if (FixedScale)
                {
                    Debug.Assert(Math.Abs(value.X) == Math.Abs(value.Y), "Transforms with fixed scale cannot have non-uniform scale.");
                }
                MatrixUpdate = true;
                _scale = value; 
            } 
        }
        public Vector2 Position { get { return _position; } set { _position = value; MatrixUpdate = true; } }

        public Transform2D()
        {
        }

        public Transform2D(Vector2 position)
        {
            Position = position;
        }

        public Transform2D(Vector2 position, Vector2 scale)
        {
            Position = position;
            Scale = scale;
        }

        public Transform2D(Vector2 position, Vector2 scale, float rotation)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public Transform2D(Transform2D transform)
        {
            Position = new Vector2(transform.Position.X, transform.Position.Y);
            Scale = new Vector2(transform.Scale.X, transform.Scale.Y);
            Rotation = transform.Rotation;
        }

        public Transform Get3D()
        {
            return new Transform(new Vector3(Position.X, Position.Y, 0), new Vector3(Scale.X, Scale.Y, 1), new Quaternion(0, 0, 1, Rotation));
        }

        public Matrix4 GetMatrix()
        {
            if (MatrixUpdate)
            {
                Matrix = Matrix4.CreateScale(new Vector3(Scale.X, Scale.Y, 1)) * Matrix4.CreateRotationZ(Rotation) * Matrix4.CreateTranslation(new Vector3(Position.X, Position.Y, 0));
                MatrixUpdate = false;
            }
            return Matrix; 
        }

        public bool IsMirrored()
        {
            return Scale.X * Scale.Y < 0;
        }

        public Vector2 GetNormal()
        {
            return new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
        }
    }
}
