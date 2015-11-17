using OpenTK;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Xna = Microsoft.Xna.Framework;

namespace Game
{
    [Serializable]
    public class Transform2D
    {
        private Matrix4 Matrix;
        public bool MatrixUpdate { get; private set; }
        private Vector2 _position = new Vector2();
        private float _rotation = 0;
        private Vector2 _scale = new Vector2(1, 1);
        private bool _uniformScale = false;
        private const float UNIFORM_SCALE_EPSILON = 0.0001f;
        private const float EQUALITY_EPSILON = 0.0001f;
        private Transform2D _parent = null;

        public Transform2D Parent
        {
            get { return _parent; }
            set 
            {
                Transform2D parentPrev = value;
                _parent = value;
                if (ParentLoopExists())
                {
                    _parent = parentPrev;
                    throw (new Exception("Cannot make circlular dependencies in parents."));
                }
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
                MatrixUpdate = true; 
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
                Debug.Assert(!Vector2Ext.IsNaN(value));
                Debug.Assert(value.X != 0 && value.Y != 0, "Scale vector must have non-zero components");
                if (UniformScale)
                {
                    Debug.Assert(
                        Math.Abs(value.X) - Math.Abs(value.Y) <= UNIFORM_SCALE_EPSILON, 
                        "Transforms with fixed scale cannot have non-uniform scale.");
                    value.Y = Math.Sign(value.Y) * Math.Abs(value.X);
                }
                MatrixUpdate = true;
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
                Debug.Assert(!Vector2Ext.IsNaN(value));
                _position = value; 
                MatrixUpdate = true; 
            }
        }

        public Vector2 WorldPosition
        {
            get
            {
                if (_parent != null)
                {
                    return Vector2Ext.Transform(Position, _parent.GetWorldMatrix());
                }
                else
                {
                    return Position;
                }
            }
        }

        #region constructors
        public Transform2D()
        {
            MatrixUpdate = true;
        }

        public Transform2D(Vector2 position)
            : this(position, new Vector2(1,1), 0f, null, false)
        {
        }

        public Transform2D(Vector2 position, Vector2 scale)
            : this(position, scale, 0f, null, false)
        {
        }

        public Transform2D(Vector2 position, float rotation)
            : this(position, new Vector2(1, 1), rotation, null, false)
        {
        }

        public Transform2D(Vector2 position, Vector2 scale, float rotation)
            :this(position, scale, rotation, null, false)
        {
        }

        public Transform2D(Xna.Vector2 position, float rotation)
            : this(Vector2Ext.ConvertTo(position), new Vector2(1, 1), rotation, null, false)
        {
        }

        public Transform2D(Vector2 position, Vector2 scale, float rotation, Transform2D parent, bool fixedScale)
        {
            UniformScale = fixedScale;
            Position = position;
            Scale = scale;
            Rotation = rotation;
            Parent = parent;
            MatrixUpdate = true;
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
        #endregion

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
                MatrixUpdate = false;
            }
            return Matrix;
        }

        public Matrix4 GetWorldMatrix()
        {
            Matrix4 Matrix = Matrix4.Identity;
            if (_parent != null)
            {
                Matrix = _parent.GetWorldMatrix();
            }
            Matrix = GetMatrix() * Matrix;//Matrix4.CreateScale(new Vector3(Scale.X, Scale.Y, 1)) * Matrix4.CreateRotationZ(Rotation) * Matrix4.CreateTranslation(new Vector3(Position.X, Position.Y, 0)) * Matrix;
            return Matrix; 
        }

        public bool IsMirrored()
        {
            return Math.Sign(Scale.X) != Math.Sign(Scale.Y);
        }

        public bool IsWorldMirrored()
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
        /// Other properties and fields are not copied.  If transform is null then nothing is changed.
        /// </summary>
        public void SetLocal(Transform2D transform)
        {
            if (transform != null)
            {
                SetLocal(transform.Position, transform.Scale, transform.Rotation);
            }
        }

        public Vector2 GetNormal(bool normalizeValue = true)
        {
            Vector2[] v = new Vector2[2] {
                new Vector2(0, 0),
                new Vector2(1, 0)
            };
            v = Vector2Ext.Transform(v, GetMatrix());
            if (normalizeValue)
            {
                Debug.Assert(!Vector2Ext.IsNaN((v[1] - v[0]).Normalized()), "Unable to normalize 0 length vector.");
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
            v = Vector2Ext.Transform(v, GetWorldMatrix());
            if (normalizeValue)
            {
                Debug.Assert(!Vector2Ext.IsNaN((v[1] - v[0]).Normalized()), "Unable to normalize 0 length vector.");
                return (v[1] - v[0]).Normalized();
            }
            return v[1] - v[0];
        }

        public Vector2 WorldToLocal(Vector2 v)
        {
            return Vector2Ext.Transform(v, GetMatrix().Inverted());
        }

        public Vector2[] WorldToLocal(Vector2[] v)
        {
            return Vector2Ext.Transform(v, GetMatrix().Inverted());
        }

        public Vector2 LocalToWorld(Vector2 v)
        {
            return Vector2Ext.Transform(v, GetMatrix());
        }

        public Vector2[] LocalToWorld(Vector2[] v)
        {
            return Vector2Ext.Transform(v, GetMatrix());
        }

        public Transform2D Copy()
        {
            Transform2D transform = new Transform2D(Position, Scale, Rotation);
            transform.UniformScale = UniformScale;
            transform._parent = Parent;
            return transform;
        }

        /// <summary>
        /// Returns true if there is a loop in the Parent dependencies.
        /// </summary>
        /// <returns></returns>
        private bool ParentLoopExists()
        {
            const int DONT_CARE = 0;
            Dictionary<Transform2D, int> map = new Dictionary<Transform2D, int>();
            Transform2D transform = this;
            while (transform._parent != null)
            {
                transform = transform._parent;
                if (map.ContainsKey(transform))
                {
                    return true;
                }
                map.Add(transform, DONT_CARE);
            }
            return false;
        }

        public bool LocalEquals(Transform2D transform)
        {
            if (transform != null)
            {
                if (Math.Abs(Rotation - transform.Rotation) <= EQUALITY_EPSILON &&
                    Math.Abs(Scale.X - transform.Scale.X) <= EQUALITY_EPSILON &&
                    Math.Abs(Scale.Y - transform.Scale.Y) <= EQUALITY_EPSILON &&
                    Math.Abs(Position.X - transform.Position.X) <= EQUALITY_EPSILON &&
                    Math.Abs(Position.Y - transform.Position.Y) <= EQUALITY_EPSILON)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
