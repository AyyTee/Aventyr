using OpenTK;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Xna = Microsoft.Xna.Framework;
using System.ComponentModel;

namespace Game
{
    [Serializable]
    public class Transform2D
    {
        Matrix4 Matrix;
        public bool MatrixUpdate { get; private set; }
        Vector2 _position = new Vector2();
        float _rotation = 0;
        Vector2 _scale = new Vector2(1, 1);
        bool _uniformScale = false;
        const float UNIFORM_SCALE_EPSILON = 0.0001f;
        const float EQUALITY_EPSILON = 0.0001f;

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
            //Parent = parent;
            MatrixUpdate = true;
        }

        /// <summary>Copy constructor</summary>
        public Transform2D(Transform2D transform)
        {
            Position = transform.Position;
            Scale = transform.Scale;
            Rotation = transform.Rotation;
            _uniformScale = transform._uniformScale;
        }
        #endregion

        public Transform3D Get3D()
        {
            return new Transform3D(new Vector3(Position), new Vector3(Scale.X, Scale.Y, 1), new Quaternion(0, 0, 1, Rotation));
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
            return Math.Sign(Scale.X) != Math.Sign(Scale.Y);
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

        public Transform2D Transform(Transform2D transform)
        {
            Transform2D output = Copy();
            output.Rotation += transform.Rotation;
            output.Scale *= transform.Scale;
            output.Position = Vector2Ext.Transform(output.Position, transform.GetMatrix());
            return output;
        }

        public Transform2D Copy()
        {
            Transform2D transform = new Transform2D(Position, Scale, Rotation);
            transform.UniformScale = UniformScale;
            return transform;
        }

        public bool Compare(Transform2D transform)
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
