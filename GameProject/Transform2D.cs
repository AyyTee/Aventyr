using OpenTK;
using System;
using System.Diagnostics;

namespace Game
{
    public class Transform2D
    {
        private Matrix4 Matrix;
        private bool _matrixUpdate = true;

        public bool MatrixUpdate
        {
            get 
            {
                if (_matrixUpdate)
                {
                    return _matrixUpdate;
                }
                if (_parent != null)
                {
                    if (Parent.MatrixUpdate)
                    {
                        return Parent.MatrixUpdate;
                    }
                }
                return false;
            }
        }
        private Vector2 _position = new Vector2();
        private float _rotation = 0;
        private Vector2 _scale = new Vector2(1, 1);
        private bool _fixedScale = false;
        private Transform2D _parent = null;

        public Transform2D Parent
        {
            get { return _parent; }
            set 
            {
                _matrixUpdate = true;
                _parent = value; 
            }
        }

        public bool FixedScale 
        { 
            get { return _fixedScale; }
            set { _fixedScale = value; }
        }

        public float Rotation 
        { 
            get { return _rotation; }
            set 
            {
                Debug.Assert(!Double.IsNaN(value));
                _rotation = value; 
                _matrixUpdate = true; 
            } 
        }
        public Vector2 Scale 
        { 
            get { return _scale; }
            set 
            { 
                if (FixedScale)
                {
                    Debug.Assert(Math.Abs(value.X) == Math.Abs(value.Y), "Transforms with fixed scale cannot have non-uniform scale.");
                }
                Debug.Assert(!VectorExt2.IsNaN(value));
                Debug.Assert(value.X != 0 && value.Y != 0, "Scale vector must have non-zero componenets");
                _matrixUpdate = true;
                _scale = value; 
            } 
        }
        public Vector2 Position 
        { 
            get { return _position; } 
            set 
            {
                Debug.Assert(!VectorExt2.IsNaN(value));
                _position = value; 
                _matrixUpdate = true; 
            } 
        }

        public Transform2D()
        {
        }

        public Transform2D(Vector2 position)
            : this(position, new Vector2(1,1), 0f, null, false)
        {
        }

        public Transform2D(Vector2 position, Vector2 scale)
            : this(position, scale, 0f, null, false)
        {
        }

        public Transform2D(Vector2 position, Vector2 scale, float rotation)
            :this(position, scale, rotation, null, false)
        {
        }

        public Transform2D(Vector2 position, Vector2 scale, float rotation, Transform2D parent, bool fixedScale)
        {
            FixedScale = fixedScale;
            Position = position;
            Scale = scale;
            Rotation = rotation;
            _parent = parent;
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
                Matrix = Matrix4.Identity;
                if (_parent != null)
                {
                    Matrix = _parent.GetMatrix();
                }
                Matrix = Matrix4.CreateScale(new Vector3(Scale.X, Scale.Y, 1)) * Matrix4.CreateRotationZ(Rotation) * Matrix4.CreateTranslation(new Vector3(Position.X, Position.Y, 0)) * Matrix;
                _matrixUpdate = false;
            }
            return Matrix; 
        }

        public bool IsMirrored()
        {
            return Scale.X * Scale.Y < 0;
        }

        public Vector2 GetNormal(bool normalizeValue = true)
        {
            Vector2[] v = new Vector2[2] {
                new Vector2(0, 0),
                new Vector2(1, 0)
            };
            v = VectorExt2.Transform(v, GetMatrix());
            if (normalizeValue)
            {
                return (v[1] - v[0]).Normalized();
            }
            return v[1] - v[0];
        }

        public Vector2 WorldToLocal(Vector2 v)
        {
            return VectorExt2.Transform(v, GetMatrix().Inverted());
        }

        public Vector2[] WorldToLocal(Vector2[] v)
        {
            return VectorExt2.Transform(v, GetMatrix().Inverted());
        }

        public Vector2 LocalToWorld(Vector2 v)
        {
            return VectorExt2.Transform(v, GetMatrix());
        }

        public Vector2[] LocalToWorld(Vector2[] v)
        {
            return VectorExt2.Transform(v, GetMatrix());
        }

        public Transform2D Copy()
        {
            Transform2D transform = new Transform2D(Position, Scale, Rotation);
            transform.FixedScale = FixedScale;
            transform._parent = Parent;
            return transform;
        }
    }
}
