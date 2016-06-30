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
    public class Transform2 : IShallowClone<Transform2>
    {
        [DataMember]
        Vector2 _position = new Vector2();
        [DataMember]
        float _rotation = 0;
        /// <summary>
        /// X-axis mirroring.
        /// </summary>
        [DataMember]
        public bool MirrorX { get; set; }
        const float UNIFORM_SCALE_EPSILON = 0.0001f;
        const float EQUALITY_EPSILON = 0.0001f;

        public float Rotation 
        { 
            get { return _rotation; }
            set 
            {
                Debug.Assert(!double.IsNaN(value));
                _rotation = value; 
            }
        }

        [DataMember]
        public float _size = 1;
        public float Size { get { return _size; }
            set 
            {
                Debug.Assert(!float.IsNaN(value) && !float.IsPositiveInfinity(value) && !float.IsNegativeInfinity(value));
                _size = value; 
            }
        }

        public Vector2 Scale
        {
            get 
            { 
                if (MirrorX)
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

        public Transform2(Vector2 position, float size)
            : this(position, size, 0f)
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

        public Transform2(Xna.Vector2 position, float scale, float rotation)
            : this(Vector2Ext.ConvertTo(position), scale, rotation)
        {
        }

        public Transform2(Vector2 position, float scale, float rotation, bool mirrorX)
        {
            Position = position;
            Size = scale;
            Rotation = rotation;
            MirrorX = mirrorX;
        }
        #endregion

        public Transform2 ShallowClone()
        {
            Transform2 clone = new Transform2();
            clone.Position = Position;
            clone.Size = Size;
            clone.MirrorX = MirrorX;
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

        /// <summary>
        /// Returns transform that is this transformed with another Transform2.
        /// </summary>
        /// <remarks>The method has the following property:
        /// Transform(transform).GetMatrix();
        /// approximately equals 
        /// GetMatrix() * transform.GetMatrix();</remarks>
        public Transform2 Transform(Transform2 transform)
        {
            Transform2 output = ShallowClone();
            if (transform.MirrorX)
            {
                output.Rotation *= -1;
            }
            output.Rotation += transform.Rotation;
            output.Size *= transform.Size;
            output.MirrorX = output.MirrorX != transform.MirrorX;
            output.Position = Vector2Ext.Transform(output.Position, transform.GetMatrix());
            Debug.Assert(Matrix4Ext.AlmostEqual(output.GetMatrix(), GetMatrix() * transform.GetMatrix()));
            return output;
        }

        /// <summary>
        /// Returns an inverted Transform2 instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>The method has the following property:
        /// Inverted().GetMatrix();
        /// approximately equals 
        /// GetMatrix().Inverted();</remarks>
        public Transform2 Inverted()
        {
            Transform2 invert = new Transform2();
            if ((Scale.Y < 0) == (Scale.X < 0))
            {
                invert.Rotation = -Rotation;
            }
            else
            {
                invert.Rotation = Rotation;
            }
            invert.SetScale(new Vector2(1 / Scale.X, 1 / Scale.Y));
            Matrix4 mat = Matrix4.CreateRotationZ(-Rotation) * Matrix4.CreateScale(new Vector3(invert.Scale));
            invert.Position = Vector2Ext.Transform(-Position, mat);
            Debug.Assert(Matrix4Ext.AlmostEqual(GetMatrix().Inverted(), invert.GetMatrix()));
            return invert;
        }

        public Transform2 Add(Transform2 transform)
        {
            Transform2 output = ShallowClone();
            output.Rotation += transform.Rotation;
            output.Size += transform.Size;
            output.MirrorX = output.MirrorX != transform.MirrorX;
            output.Position += transform.Position;
            return output;
        }

        /// <summary>
        /// Returns the result of this minus another transform.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public Transform2 Minus(Transform2 transform)
        {
            Transform2 output = ShallowClone();
            output.Rotation -= transform.Rotation;
            output.Size -= transform.Size;
            output.MirrorX = output.MirrorX != transform.MirrorX;
            output.Position -= transform.Position;
            return output;
        }

        /// <summary>
        /// Returns a transform that is componentwise multiplication of this with a scalar value.
        /// </summary>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public Transform2 Multiply(float scalar)
        {
            Transform2 output = ShallowClone();
            output.Rotation = Rotation * scalar;
            output.Size = Size * scalar;
            output.Position = Position * scalar;
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

            MirrorX = false;
            if (Math.Sign(scale.X) != Math.Sign(scale.Y))
            {
                MirrorX = true;
            }
            Size = scale.Y;
            Debug.Assert(Scale == scale);
        }

        public void SetScale(float size, bool mirrorX, bool mirrorY)
        {
            Size = size;
            if (mirrorX != mirrorY)
            {
                MirrorX = true;
            }
            if (mirrorY)
            {
                Size *= -1;
            }
        }

        public bool AlmostEqual(Transform2 transform)
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

        public static Transform2 CreateVelocity()
        {
            return CreateVelocity(new Vector2());
        }

        public static Transform2 CreateVelocity(Vector2 linearVelocity)
        {
            return CreateVelocity(linearVelocity, 0);
        }

        public static Transform2 CreateVelocity(Vector2 linearVelocity, float angularVelocity)
        {
            return CreateVelocity(linearVelocity, angularVelocity, 0);
        }

        public static Transform2 CreateVelocity(Vector2 linearVelocity, float angularVelocity, float scalarVelocity)
        {
            return new Transform2(linearVelocity, scalarVelocity, angularVelocity);
        }

        public static bool operator ==(Transform2 left, Transform2 right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Transform2 left, Transform2 right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Transform2)
            {
                return Equals(this, (Transform2)obj);
            }
            return false;
        }

        public static bool Equals(Transform2 t0, Transform2 t1)
        {
            if (((object)t0) == null && ((object)t1) == null)
            {
                return true;
            }
            else if (((object)t0) == null || ((object)t1) == null)
            {
                return false;
            }

            return t0.Position == t1.Position && 
                t0.Rotation == t1.Rotation && 
                t0.Scale == t1.Scale;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ 
                Rotation.GetHashCode() ^ 
                Size.GetHashCode() ^ 
                MirrorX.GetHashCode() ^
                Position.GetHashCode();
        }

        public static void SetPosition(ITransformable2 transformable, Vector2 position)
        {
            Transform2 transform = transformable.GetTransform();
            transform.Position = position;
            transformable.SetTransform(transform);
        }

        public static void SetRotation(ITransformable2 transformable, float rotation)
        {
            Transform2 transform = transformable.GetTransform();
            transform.Rotation = rotation;
            transformable.SetTransform(transform);
        }

        public static void SetScale(ITransformable2 transformable, Vector2 scale)
        {
            Transform2 transform = transformable.GetTransform();
            transform.SetScale(scale);
            transformable.SetTransform(transform);
        }

        public static void SetSize(ITransformable2 transformable, float size)
        {
            Transform2 transform = transformable.GetTransform();
            transform.Size = size;
            transformable.SetTransform(transform);
        }

        public static void SetScale(ITransformable2 transformable, float size, bool mirrorX, bool mirrorY)
        {
            Transform2 transform = transformable.GetTransform();
            transform.SetScale(size, mirrorX, mirrorY);
            transformable.SetTransform(transform);
        }

        public static Vector2 GetPosition(ITransformable2 transformable)
        {
            return transformable.GetTransform().Position;
        }

        public static float GetRotation(ITransformable2 transformable)
        {
            return transformable.GetTransform().Rotation;
        }

        public static Vector2 GetScale(ITransformable2 transformable)
        {
            return transformable.GetTransform().Scale;
        }

        public static float GetSize(ITransformable2 transformable)
        {
            return transformable.GetTransform().Size;
        }
    }
}
