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
        Vector2 _scale = new Vector2(1, 1);
        [DataMember]
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
            }
        }

        public Vector2 Scale 
        { 
            get { return _scale; }
            set 
            {
                Debug.Assert(Vector2Ext.IsReal(value));
                Debug.Assert(value.X != 0 && value.Y != 0, "Scale vector must have non-zero components");
                if (UniformScale)
                {
                    Debug.Assert(
                        Math.Abs(value.X) - Math.Abs(value.Y) <= UNIFORM_SCALE_EPSILON, 
                        "Transforms with fixed scale cannot have non-uniform scale.");
                    value.Y = Math.Sign(value.Y) * Math.Abs(value.X);
                }
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
            }
        }

        #region Constructors
        public Transform2()
        {
        }

        public Transform2(Vector2 position)
            : this(position, new Vector2(1,1), 0f, false)
        {
        }

        public Transform2(Vector2 position, Vector2 scale)
            : this(position, scale, 0f, false)
        {
        }

        public Transform2(Vector2 position, float rotation)
            : this(position, new Vector2(1, 1), rotation, false)
        {
        }

        public Transform2(Vector2 position, Vector2 scale, float rotation)
            :this(position, scale, rotation, false)
        {
        }

        public Transform2(Xna.Vector2 position, float rotation)
            : this(Vector2Ext.ConvertTo(position), new Vector2(1, 1), rotation, false)
        {
        }

        public Transform2(Vector2 position, Vector2 scale, float rotation, bool fixedScale)
        {
            UniformScale = fixedScale;
            Position = position;
            Scale = scale;
            Rotation = rotation;
        }

        /// <summary>Copy constructor</summary>
        public Transform2(Transform2 transform)
        {
            Position = transform.Position;
            Scale = transform.Scale;
            Rotation = transform.Rotation;
            _uniformScale = transform._uniformScale;
        }
        #endregion

        public Transform3 Get3D()
        {
            return new Transform3(new Vector3(Position), new Vector3(Scale.X, Scale.Y, 1), new Quaternion(0, 0, 1, Rotation));
        }
        
        public Matrix4 GetMatrix()
        {
            return Matrix4.CreateScale(new Vector3(Scale.X, Scale.Y, 1)) * Matrix4.CreateRotationZ(Rotation) * Matrix4.CreateTranslation(new Vector3(Position));
        }
        
        public bool IsMirrored()
        {
            return Math.Sign(Scale.X) != Math.Sign(Scale.Y);
        }

        public Vector2 GetUp(bool normalizeValue = true)
        {
            return GetVector(new Vector2(0, 1), normalizeValue);
        }

        public Vector2 GetRight(bool normalizeValue = true)
        {
            return GetVector(new Vector2(1, 0), normalizeValue);
        }

        private Vector2 GetVector(Vector2 vector, bool normalizeValue)
        {
            Vector2[] v = new Vector2[2] {
                new Vector2(0, 0),
                vector
            };
            v = Vector2Ext.Transform(v, GetMatrix());
            if (normalizeValue)
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
            output.Scale *= transform.Scale;
            output.Position = Vector2Ext.Transform(output.Position, transform.GetMatrix());
            return output;
        }

        public Transform2 Add(Transform2 transform)
        {
            Transform2 output = Clone();
            output.Rotation += transform.Rotation;
            output.Scale *= transform.Scale;
            output.Position += transform.Position;
            return output;
        }

        /// <summary>Subtracts transfrom from this.</summary>
        public Transform2 Subtract(Transform2 transform)
        {
            Transform2 output = Clone();
            output.Rotation -= transform.Rotation;
            output.Scale = Vector2.Divide(output.Scale, transform.Scale);
            output.Position -= transform.Position;
            return output;
        }

        public Transform2 Clone()
        {
            Transform2 transform = new Transform2(Position, Scale, Rotation, UniformScale);
            return transform;
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
            transform.Scale = scale;
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
    }
}
