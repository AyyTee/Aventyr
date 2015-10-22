using OpenTK;
using System;
using System.Diagnostics;

namespace Game
{
    [Serializable]
    public class Transform2D
    {
        private Matrix4 Matrix;
        private bool _matrixUpdate = true;

        public bool MatrixUpdate
        {
            get 
            {
                return _matrixUpdate;
            }
        }
        private Vector2 _position = new Vector2();
        private float _rotation = 0;
        private Vector2 _scale = new Vector2(1, 1);
        private bool _uniformScale = false;
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

        public bool UniformScale 
        { 
            get { return _uniformScale; }
            set { _uniformScale = value; }
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

        public float WorldRotation
        {
            get 
            {
                float rot = 0;
                if (_parent != null)
                {
                    rot = _parent.WorldRotation;
                }
                return rot + Rotation;
            }
        }

        public Vector2 Scale 
        { 
            get { return _scale; }
            set 
            { 
                if (UniformScale)
                {
                    Debug.Assert(Math.Abs(value.X) == Math.Abs(value.Y), "Transforms with fixed scale cannot have non-uniform scale.");
                }
                Debug.Assert(!VectorExt2.IsNaN(value));
                Debug.Assert(value.X != 0 && value.Y != 0, "Scale vector must have non-zero components");
                _matrixUpdate = true;
                _scale = value; 
            } 
        }

        public Vector2 WorldScale
        {
            get
            {
                Vector2 scale = new Vector2(1, 1);
                if (_parent != null)
                {
                    scale = _parent.WorldScale;
                }
                return scale * Scale;
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

        public Vector2 WorldPosition
        {
            get
            {
                if (_parent != null)
                {
                    return VectorExt2.Transform(Position, _parent.GetWorldMatrix());
                }
                else
                {
                    return Position;
                }
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
            UniformScale = fixedScale;
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

        public Transform GetWorld3D()
        {
            return new Transform(new Vector3(WorldPosition.X, WorldPosition.Y, 0), new Vector3(WorldScale.X, WorldScale.Y, 1), new Quaternion(0, 0, 1, WorldRotation));
        }

        public Matrix4 GetMatrix()
        {
            if (MatrixUpdate)
            {
                Matrix = Matrix4.CreateScale(new Vector3(Scale.X, Scale.Y, 1)) * Matrix4.CreateRotationZ(Rotation) * Matrix4.CreateTranslation(new Vector3(Position.X, Position.Y, 0));
                _matrixUpdate = false;
            }
            return Matrix;
        }

        public Matrix4 GetWorldMatrix()
        {
            Matrix4 Matrix = Matrix4.Identity;
            if (_parent != null)
            {
                Matrix = _parent.GetMatrix();
            }
            Matrix = GetMatrix() * Matrix;//Matrix4.CreateScale(new Vector3(Scale.X, Scale.Y, 1)) * Matrix4.CreateRotationZ(Rotation) * Matrix4.CreateTranslation(new Vector3(Position.X, Position.Y, 0)) * Matrix;
            return Matrix; 
        }

        public bool IsMirrored()
        {
            return Math.Sign(Scale.X) != Math.Sign(Scale.Y);
        }

        public bool WorldIsMirrored()
        {
            return Math.Sign(WorldScale.X) != Math.Sign(WorldScale.Y);
        }

        /// <summary>
        /// Assign new position, scale, and rotation for this transform.
        /// </summary>
        public void SetLocal(Vector2 position, Vector2 scale, float rotation)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
        }

        /// <summary>
        /// Assign new position, scale, and rotation from another Transform2D instance.
        /// </summary>
        /// <param name="transform"></param>
        public void SetLocal(Transform2D transform)
        {
            SetLocal(transform.Position, transform.Scale, transform.Rotation);
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
                Debug.Assert(!VectorExt2.IsNaN((v[1] - v[0]).Normalized()), "Unable to normalize 0 length vector.");
                return (v[1] - v[0]).Normalized();
            }
            return v[1] - v[0];
        }

        public Vector2 GetWorldNormal(bool normalizeValue = true)
        {
            Vector2[] v = new Vector2[2] {
                new Vector2(0, 0),
                new Vector2(1, 0)
            };
            v = VectorExt2.Transform(v, GetWorldMatrix());
            if (normalizeValue)
            {
                Debug.Assert(!VectorExt2.IsNaN((v[1] - v[0]).Normalized()), "Unable to normalize 0 length vector.");
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
            transform.UniformScale = UniformScale;
            transform._parent = Parent;
            return transform;
        }

        public static bool operator ==(Transform2D transformFirst, Transform2D transformLast)
        {

            if ((object)transformFirst == null || (object)transformLast == null)
            {
                if ((object)transformFirst == (object)transformLast)
                {
                    return true;
                }
                return false;
            }
            if (transformFirst.Rotation == transformLast.Rotation && transformFirst.Scale == transformLast.Scale && transformFirst.Position == transformLast.Position)
            {
                return true;
            }
            return false;
        }

        public static bool operator !=(Transform2D transformFirst, Transform2D transformLast)
        {
            return !(transformFirst == transformLast);
        }
    }
}
