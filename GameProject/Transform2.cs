using OpenTK;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Xna = Microsoft.Xna.Framework;
using System.ComponentModel;
using System.Xml;
using System.Runtime.Serialization;

namespace Game
{
    [DataContract]
    public class Transform2
    {
        [DataMember]
        Vector2 _position = new Vector2();
        [DataMember]
        float _rotation = 0;
        [DataMember]
        public bool IsMirrored { get; set; }
        const float UNIFORM_SCALE_EPSILON = 0.0001f;
        const float EQUALITY_EPSILON = 0.0001f;

        public float Rotation 
        { 
            get { return _rotation; }
            set 
            {
                Debug.Assert(!Double.IsNaN(value));
                _rotation = value; 
            }
        }

        [DataMember]
        public float _size = 1;
        public float Size { get { return _size; } 
            set 
            {
                Debug.Assert(value != 0);
                Debug.Assert(!float.IsNaN(value) && !float.IsPositiveInfinity(value) && !float.IsNegativeInfinity(value));
                _size = value; 
            }
        }

        public Vector2 Scale
        {
            get 
            { 
                if (IsMirrored)
                {
                    return new Vector2(-Size, Size); 
                }
                return new Vector2(Size, Size); 
            }
        }

        public Vector2 Position 
        { 
            get { return _position; }
            set 
            {
                Debug.Assert(!Vector2Ext.IsNaN(value));
                _position = value; 
            }
        }

        #region Constructors
        public Transform2()
        {
        }

        public Transform2(Vector2 position)
            : this(position, 1)
        {
        }

        public Transform2(Vector2 position, float scale)
            : this(position, scale, 0f)
        {
        }

        public Transform2(Vector2 position, float scale, float rotation)
            : this(position, scale, rotation, false)
        {
        }

        public Transform2(Xna.Vector2 position)
            : this(Vector2Ext.ConvertTo(position))
        {
        }

        public Transform2(Xna.Vector2 position, float rotation)
            : this(Vector2Ext.ConvertTo(position), 1, rotation)
        {
        }

        public Transform2(Vector2 position, float scale, float rotation, bool isMirrored)
        {
            Position = position;
            Size = scale;
            Rotation = rotation;
            IsMirrored = isMirrored;
        }
        #endregion

        public Transform2 Clone()
        {
            Transform2 clone = new Transform2();
            clone.Position = Position;
            clone.Size = Size;
            clone.IsMirrored = IsMirrored;
            clone.Rotation = Rotation;
            return clone;
        }

        public Transform3 Get3D()
        {
            return new Transform3(new Vector3(Position), new Vector3(Scale.X, Scale.Y, 1), new Quaternion(0, 0, 1, Rotation));
        }
        
        public Matrix4 GetMatrix()
        {
            return Matrix4.CreateScale(new Vector3(Scale.X, Scale.Y, 1)) * Matrix4.CreateRotationZ(Rotation) * Matrix4.CreateTranslation(new Vector3(Position));
        }

        public Vector2 GetUp(bool normalize = true)
        {
            return GetVector(new Vector2(0, 1), normalize);
        }

        public Vector2 GetRight(bool normalize = true)
        {
            return GetVector(new Vector2(1, 0), normalize);
        }

        private Vector2 GetVector(Vector2 vector, bool normalize)
        {
            Vector2[] v = new Vector2[2] {
                new Vector2(0, 0),
                vector
            };
            v = Vector2Ext.Transform(v, GetMatrix());
            if (normalize)
            {
                Debug.Assert(!Vector2Ext.IsNaN((v[1] - v[0]).Normalized()), "Unable to normalize 0 length vector.");
                return (v[1] - v[0]).Normalized();
            }
            return v[1] - v[0];
        }

        public Transform2 Transform(Transform2 transform)
        {
            Transform2 output = Clone();
            output.Rotation += transform.Rotation;
            output.Size *= transform.Size;
            output.IsMirrored = output.IsMirrored != transform.IsMirrored;
            output.Position = Vector2Ext.Transform(output.Position, transform.GetMatrix());
            return output;
        }

        public Transform2 Add(Transform2 transform)
        {
            Transform2 output = Clone();
            output.Rotation += transform.Rotation;
            output.Size *= transform.Size;
            output.IsMirrored = output.IsMirrored != transform.IsMirrored;
            output.Position += transform.Position;
            return output;
        }

        /// <summary>Subtracts transfrom from this.</summary>
        public Transform2 Subtract(Transform2 transform)
        {
            Transform2 output = Clone();
            output.Rotation -= transform.Rotation;
            output.Size /= transform.Size;
            output.IsMirrored = output.IsMirrored != transform.IsMirrored;
            output.Position -= transform.Position;
            return output;
        }

        public void SetScale(Vector2 scale)
        {
            Debug.Assert(Vector2Ext.IsReal(scale));
            Debug.Assert(scale.X != 0 && scale.Y != 0, "Scale vector must have non-zero components");
            Debug.Assert(
                Math.Abs(scale.X) - Math.Abs(scale.Y) <= UNIFORM_SCALE_EPSILON,
                "Transforms with fixed scale cannot have non-uniform scale.");

            if (scale.Y > 0)
            {
                scale.Y = Math.Abs(scale.X);
            }
            else
            {
                scale.Y = -Math.Abs(scale.X);
            }

            IsMirrored = false;
            if (Math.Sign(scale.X) != Math.Sign(scale.Y))
            {
                IsMirrored = true;
            }
            Size = scale.Y;
            Debug.Assert(Scale == scale);
        }

        public void SetScale(float size, bool mirrorX, bool mirrorY)
        {
            Size = size;
            if (mirrorX != mirrorY)
            {
                IsMirrored = true;
            }
            if (mirrorY)
            {
                Size *= -1;
            }
        }

        public bool Compare(Transform2 transform)
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

        public static void SetPosition(ITransform2 transformable, Vector2 position)
        {
            Transform2 transform = transformable.GetTransform();
            transform.Position = position;
            transformable.SetTransform(transform);
        }

        public static void SetRotation(ITransform2 transformable, float rotation)
        {
            Transform2 transform = transformable.GetTransform();
            transform.Rotation = rotation;
            transformable.SetTransform(transform);
        }

        public static void SetScale(ITransform2 transformable, Vector2 scale)
        {
            Transform2 transform = transformable.GetTransform();
            transform.SetScale(scale);
            transformable.SetTransform(transform);
        }

        public static void SetSize(ITransform2 transformable, float size)
        {
            Transform2 transform = transformable.GetTransform();
            transform.Size = size;
            transformable.SetTransform(transform);
        }

        public static void SetScale(ITransform2 transformable, float size, bool mirrorX, bool mirrorY)
        {
            Transform2 transform = transformable.GetTransform();
            transform.SetScale(size, mirrorX, mirrorY);
            transformable.SetTransform(transform);
        }

        public static Vector2 GetPosition(ITransform2 transformable)
        {
            return transformable.GetTransform().Position;
        }

        public static float GetRotation(ITransform2 transformable)
        {
            return transformable.GetTransform().Rotation;
        }

        public static Vector2 GetScale(ITransform2 transformable)
        {
            return transformable.GetTransform().Scale;
        }

        public static float GetSize(ITransform2 transformable)
        {
            return transformable.GetTransform().Size;
        }
    }
}
