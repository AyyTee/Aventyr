using OpenTK;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Xna = Microsoft.Xna.Framework;
using System.ComponentModel;
using System.Xml;
using System.Runtime.Serialization;
using Game.Serialization;

namespace Game.Common
{
    [DataContract]
    public class Transform2D : IShallowClone<Transform2D>, IAlmostEqual<Transform2D>
    {
        [DataMember]
        Vector2d _position = new Vector2d();
        [DataMember]
        double _rotation = 0;
        /// <summary>
        /// X-axis mirroring.
        /// </summary>
        [DataMember]
        public bool MirrorX { get; set; }
        const double UniformScaleEpsilon = 0.0001f;
        const double EqualityEpsilon = 0.0001f;

        public double Rotation 
        { 
            get { return _rotation; }
            set 
            {
                Debug.Assert(!double.IsNaN(value));
                _rotation = value; 
            }
        }

        [DataMember]
        public double _size = 1;
        public double Size { get { return _size; }
            set 
            {
                Debug.Assert(!double.IsNaN(value) && !double.IsPositiveInfinity(value) && !double.IsNegativeInfinity(value));
                _size = value; 
            }
        }

        public Vector2d Scale
        {
            get 
            { 
                if (MirrorX)
                {
                    return new Vector2d(-Size, Size); 
                }
                return new Vector2d(Size, Size); 
            }
        }

        public Vector2d Position 
        { 
            get { return _position; }
            set 
            {
                Debug.Assert(!Vector2Ext.IsNaN(value));
                _position = value; 
            }
        }

        #region Constructors
        public Transform2D()
        {
        }

        public Transform2D(Vector2d position)
            : this(position, 1)
        {
        }

        public Transform2D(Vector2d position, double size)
            : this(position, size, 0f)
        {
        }

        public Transform2D(Vector2d position, double scale, double rotation)
            : this(position, scale, rotation, false)
        {
        }

        public Transform2D(Vector2d position, double scale, double rotation, bool mirrorX)
        {
            Position = position;
            Size = scale;
            Rotation = rotation;
            MirrorX = mirrorX;
        }
        #endregion

        public Transform2D ShallowClone()
        {
            Transform2D clone = new Transform2D();
            clone.Position = Position;
            clone.Size = Size;
            clone.MirrorX = MirrorX;
            clone.Rotation = Rotation;
            return clone;
        }
        
        public Matrix4d GetMatrix()
        {
            return Matrix4d.Scale(new Vector3d(Scale.X, Scale.Y, 1)) * Matrix4d.CreateRotationZ(Rotation) * Matrix4d.CreateTranslation(new Vector3d(Position));
        }

        public Vector2d GetUp(bool normalize = true)
        {
            return GetVector(new Vector2d(0, 1), normalize);
        }

        public Vector2d GetRight(bool normalize = true)
        {
            return GetVector(new Vector2d(1, 0), normalize);
        }

        private Vector2d GetVector(Vector2d vector, bool normalize)
        {
            Vector2d[] v = new Vector2d[2]  {
                new Vector2d(0, 0),
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
        /// Returns transform that is this transformed with another Transform2d.  Do not use this for velocity as it will give inappropriate results.
        /// </summary>
        /// <remarks>The method has the following property:
        /// Transform(transform).GetMatrix();
        /// approximately equals 
        /// GetMatrix() * transform.GetMatrix();</remarks>
        public Transform2D Transform(Transform2D transform)
        {
            Transform2D output = ShallowClone();
            if (transform.MirrorX)
            {
                output.Rotation *= -1;
            }
            output.Rotation += transform.Rotation;
            output.Size *= transform.Size;
            output.MirrorX = output.MirrorX != transform.MirrorX;
            output.Position = Vector2Ext.Transform(output.Position, transform.GetMatrix());
            //Debug.Assert(Matrix4Ext.AlmostEqual(output.GetMatrix(), GetMatrix() * transform.GetMatrix(), 1));
            return output;
        }

        /// <summary>
        /// Returns an inverted Transform2d instance.  Do not use this for velocity as it will give inappropriate results.
        /// </summary>
        /// <remarks>The method has the following property:
        /// Inverted().GetMatrix();
        /// approximately equals 
        /// GetMatrix().Inverted();</remarks>
        public Transform2D Inverted()
        {
            Transform2D invert = new Transform2D();
            if ((Scale.Y < 0) == (Scale.X < 0))
            {
                invert.Rotation = -Rotation;
            }
            else
            {
                invert.Rotation = Rotation;
            }
            invert.SetScale(new Vector2d(1 / Scale.X, 1 / Scale.Y));
            Matrix4d mat = Matrix4d.CreateRotationZ(-Rotation) * Matrix4d.Scale(new Vector3d(invert.Scale));
            invert.Position = Vector2Ext.Transform(-Position, mat);
            Debug.Assert(Matrix4Ext.AlmostEqual(GetMatrix().Inverted(), invert.GetMatrix()));
            return invert;
        }

        public Transform2D Add(Transform2D transform)
        {
            Transform2D output = ShallowClone();
            output.Rotation += transform.Rotation;
            output.Size += transform.Size;
            output.MirrorX = output.MirrorX != transform.MirrorX;
            output.Position += transform.Position;
            return output;
        }

        /// <summary>
        /// Returns the result of this minus another transform.
        /// </summary>
        public Transform2D Minus(Transform2D transform)
        {
            Transform2D output = ShallowClone();
            output.Rotation -= transform.Rotation;
            output.Size -= transform.Size;
            output.MirrorX = output.MirrorX != transform.MirrorX;
            output.Position -= transform.Position;
            return output;
        }

        /// <summary>
        /// Returns a transform that is componentwise multiplication of this with a scalar value.
        /// </summary>
        public Transform2D Multiply(double scalar)
        {
            Transform2D output = ShallowClone();
            output.Rotation = Rotation * scalar;
            output.Size = Size * scalar;
            output.Position = Position * scalar;
            return output;
        }

        public void SetScale(Vector2d scale)
        {
            Debug.Assert(Vector2Ext.IsReal(scale));
            Debug.Assert(scale.X != 0 && scale.Y != 0, "Scale vector must have non-zero components");
            Debug.Assert(
                Math.Abs(scale.X) - Math.Abs(scale.Y) <= UniformScaleEpsilon,
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

        public void SetScale(double size, bool mirrorX, bool mirrorY)
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

        public bool AlmostEqual(Transform2D transform)
        {
            return AlmostEqual(transform, EqualityEpsilon);
        }

        public bool AlmostEqual(Transform2D transform, double delta)
        {
            if (transform != null)
            {
                if (Math.Abs(Rotation - transform.Rotation) <= delta &&
                    Math.Abs(Scale.X - transform.Scale.X) <= delta &&
                    Math.Abs(Scale.Y - transform.Scale.Y) <= delta &&
                    Math.Abs(Position.X - transform.Position.X) <= delta &&
                    Math.Abs(Position.Y - transform.Position.Y) <= delta)
                {
                    return true;
                }
            }
            return false;
        }

        public bool AlmostEqual(Transform2D transform, double delta, double percent)
        {
            if ((Math.Abs(1 - transform.Rotation / Rotation) <= percent || Math.Abs(transform.Rotation - Rotation) <= delta) &&
                (Math.Abs(1 - transform.Scale.X / Scale.X) <= percent || Math.Abs(transform.Scale.X - Scale.X) <= delta) &&
                (Math.Abs(1 - transform.Scale.Y / Scale.Y) <= percent || Math.Abs(transform.Scale.Y - Scale.Y) <= delta) &&
                (Math.Abs(1 - transform.Position.X / Position.X) <= percent || Math.Abs(transform.Position.X - Position.X) <= delta) &&
                (Math.Abs(1 - transform.Position.Y / Position.Y) <= percent || Math.Abs(transform.Position.Y - Position.Y) <= delta))
            {
                return true;
            }
            return false;
        }

        public static Transform2D CreateVelocity()
        {
            return CreateVelocity(new Vector2d());
        }

        public static Transform2D CreateVelocity(Vector2d linearVelocity)
        {
            return CreateVelocity(linearVelocity, 0);
        }

        public static Transform2D CreateVelocity(Vector2d linearVelocity, double angularVelocity)
        {
            return CreateVelocity(linearVelocity, angularVelocity, 0);
        }

        public static Transform2D CreateVelocity(Vector2d linearVelocity, double angularVelocity, double scalarVelocity)
        {
            return new Transform2D(linearVelocity, scalarVelocity, angularVelocity);
        }

        public static bool operator ==(Transform2D left, Transform2D right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Transform2D left, Transform2D right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Transform2D)
            {
                return Equals(this, (Transform2D)obj);
            }
            return false;
        }

        public static bool Equals(Transform2D t0, Transform2D t1)
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

        public static explicit operator Transform2D(Transform2 t)
        {
            return new Transform2D((Vector2d)t.Position, t.Size, t.Rotation, t.MirrorX);
        }

        public static explicit operator Transform2(Transform2D t)
        {
            return new Transform2((Vector2)t.Position, (float)t.Size, (float)t.Rotation, t.MirrorX);
        }
    }
}
