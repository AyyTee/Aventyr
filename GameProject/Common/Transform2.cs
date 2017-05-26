// This is generated from Transform2d.cs.
using OpenTK;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Game.Serialization;

namespace Game.Common
{
    [DataContract]
    public partial class Transform2 : IShallowClone<Transform2>, IAlmostEqual<Transform2, float>, IValueEquality<Transform2>
    {
        [DataMember]
        Vector2 _position;
        [DataMember]
        float _rotation;
        /// <summary>
        /// X-axis mirroring.
        /// </summary>
        [DataMember]
        public bool MirrorX { get; private set; }
        const float UniformScaleEpsilon = 0.0001f;
        const float EqualityEpsilon = 0.0001f;

        public float Rotation 
        { 
            get { return _rotation; }
            set 
            {
                Debug.Assert(!float.IsNaN(value));
                _rotation = value; 
            }
        }

        [DataMember]
        float _size = 1;
        public float Size { get { return _size; }
            private set 
            {
                Debug.Assert(!float.IsNaN(value) && !float.IsPositiveInfinity(value) && !float.IsNegativeInfinity(value));
                _size = value; 
            }
        }

        public Vector2 Scale => MirrorX ? new Vector2(-Size, Size) : new Vector2(Size, Size);

        public Vector2 Position 
        { 
            get { return _position; }
            set 
            {
                Debug.Assert(!Vector2Ex.IsNaN(value));
                _position = value; 
            }
        }

        public Transform2()
        {
        }

        public Transform2(Vector2 position, float scale = 1, float rotation = 0, bool mirrorX = false)
        {
            Position = position;
            Size = scale;
            Rotation = rotation;
            MirrorX = mirrorX;
        }

        public Transform2 ShallowClone() => (Transform2)MemberwiseClone();

        public Transform3 Get3D()
        {
            return new Transform3(new Vector3(Position), new Vector3(Scale.X, Scale.Y, 1), new Quaternion(0, 0, 1, Rotation));
        }

        public Matrix4 GetMatrix()
        {
            return Matrix4.Scale(new Vector3(Scale.X, Scale.Y, 1)) * 
                Matrix4.CreateRotationZ(Rotation) * 
                Matrix4.CreateTranslation(new Vector3(Position));
        }

        public Vector2 GetUp() => Vector2Ex.TransformVelocity(new Vector2(0, 1), GetMatrix());

        public Vector2 GetRight() => Vector2Ex.TransformVelocity(new Vector2(1, 0), GetMatrix());

        /// <summary>
        /// Returns transform that is this transformed with another Transform2.  Do not use this for velocity as it will give inappropriate results.
        /// </summary>
        /// <remarks>The method has the following property:
        /// Transform(transform).GetMatrix();
        /// approximately equals 
        /// GetMatrix() * transform.GetMatrix();</remarks>
        public Transform2 Transform(Transform2 transform)
        {
            return new Transform2(
                Vector2Ex.Transform(Position, transform.GetMatrix()),
                Size * transform.Size,
                transform.MirrorX ?
                    -Rotation + transform.Rotation :
                    Rotation + transform.Rotation,
                MirrorX != transform.MirrorX);
        }

        /// <summary>
        /// Returns an inverted Transform2 instance.  Do not use this for velocity as it will give inappropriate results.
        /// </summary>
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
            Matrix4 mat = Matrix4.CreateRotationZ(-Rotation) * Matrix4.Scale(new Vector3(invert.Scale));
            invert.Position = Vector2Ex.Transform(-Position, mat);
            Debug.Assert(Matrix4Ex.AlmostEqual(GetMatrix().Inverted(), invert.GetMatrix()));
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
        public Transform2 Multiply(float scalar)
        {
            Transform2 output = ShallowClone();
            output.Rotation = Rotation * scalar;
            output.Size = Size * scalar;
            output.Position = Position * scalar;
            return output;
        }

        public Transform2 SetPosition(Vector2 position) => new Transform2(position, Size, Rotation, MirrorX);
        public Transform2 SetRotation(float rotation) => new Transform2(Position, Size, rotation, MirrorX);
        public Transform2 SetSize(float size) => new Transform2(Position, size, Rotation, MirrorX);
        public Transform2 SetMirrorX(bool mirrorX) => new Transform2(Position, Size, Rotation, mirrorX);

        public void SetScale(Vector2 scale)
        {
            Debug.Assert(Vector2Ex.IsReal(scale));
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

            MirrorX = Math.Sign(scale.X) != Math.Sign(scale.Y);
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

        public bool AlmostEqual(Transform2 transform, float delta = EqualityEpsilon)
        {
            if (transform == null)
            {
                return false;
            }
            if (MathEx.AlmostEqual(Rotation, transform.Rotation, delta) &&
                MathEx.AlmostEqual(Scale.X, transform.Scale.X, delta) &&
                MathEx.AlmostEqual(Scale.Y, transform.Scale.Y, delta) &&
                MathEx.AlmostEqual(Position.X, transform.Position.X, delta) &&
                MathEx.AlmostEqual(Position.Y, transform.Position.Y, delta))
            {
                return true;
            }
            return false;
        }

        public bool AlmostEqual(Transform2 transform, float delta, float ratioDelta)
        {
            if (transform == null)
            {
                return false;
            }
            if (MathEx.AlmostEqual(Rotation, transform.Rotation, delta, ratioDelta) &&
                MathEx.AlmostEqual(Scale.X, transform.Scale.X, delta, ratioDelta) &&
                MathEx.AlmostEqual(Scale.Y, transform.Scale.Y, delta, ratioDelta) &&
                MathEx.AlmostEqual(Position.X, transform.Position.X, delta, ratioDelta) &&
                MathEx.AlmostEqual(Position.Y, transform.Position.Y, delta, ratioDelta))
            {
                return true;
            }
            return false;
        }

        public static Transform2 CreateVelocity() => CreateVelocity(new Vector2());

        public static Transform2 CreateVelocity(Vector2 linearVelocity, float angularVelocity = 0, float scalarVelocity = 0)
        {
            return new Transform2(linearVelocity, scalarVelocity, angularVelocity);
        }

        public bool EqualsValue(Transform2 other)
        {
            return other != null &&
                Position == other.Position &&
                Rotation == other.Rotation &&
                Scale == other.Scale;
        }
    }
}
