// This is generated from Transform2d.cs.
using OpenTK;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Game.Serialization;

namespace Game.Common
{
    [DataContract]
    public partial class Transform2 : IAlmostEqual<Transform2, float>, IValueEquality<Transform2>
    {
        const float UniformScaleEpsilon = 0.0001f;
        const float EqualityEpsilon = 0.0001f;

        [DataMember]
        public Vector2 Position { get; private set; }

        [DataMember]
        public float Rotation { get; private set; }

        [DataMember]
        public float Size { get; private set; } = 1;

        /// <summary>
        /// X-axis mirroring.
        /// </summary>
        [DataMember]
        public bool MirrorX { get; private set; }

        public Vector2 Scale => MirrorX ? 
            new Vector2(-Size, Size) : 
            new Vector2(Size, Size);

        public Transform2()
        {
        }

        public Transform2(Vector2 position, float rotation = 0, float size = 1, bool mirrorX = false)
        {
            Debug.Assert(!Vector2Ex.IsNaN(position));
            Debug.Assert(!float.IsNaN(rotation));
            Debug.Assert(!float.IsNaN(size) && !float.IsPositiveInfinity(size) && !float.IsNegativeInfinity(size));
            Position = position;
            Size = size;
            Rotation = rotation;
            MirrorX = mirrorX;
        }

        public Transform3 Get3D()
        {
            return new Transform3(
                new Vector3(Position), 
                new Vector3(Scale.X, Scale.Y, 1), 
                new Quaternion(0, 0, 1, Rotation));
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
                transform.MirrorX ?
                    -Rotation + transform.Rotation :
                    Rotation + transform.Rotation,
                Size * transform.Size,
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
            var invert = new Transform2();
            if ((Scale.Y < 0) == (Scale.X < 0))
            {
                invert.Rotation = -Rotation;
            }
            else
            {
                invert.Rotation = Rotation;
            }
            invert = invert.SetScale(new Vector2(1 / Scale.X, 1 / Scale.Y));
            Matrix4 mat = Matrix4.CreateRotationZ(-Rotation) * Matrix4.Scale(new Vector3(invert.Scale));
            invert.Position = Vector2Ex.Transform(-Position, mat);
            Debug.Assert(Matrix4Ex.AlmostEqual(GetMatrix().Inverted(), invert.GetMatrix()));
            return invert;
        }

        public Transform2 Add(Transform2 transform)
        {
            return new Transform2(
                Position + transform.Position,
                Rotation + transform.Rotation,
                Size + transform.Size,
                MirrorX != transform.MirrorX);
        }

        /// <summary>
        /// Returns the result of this minus another transform.
        /// </summary>
        public Transform2 Minus(Transform2 transform)
        {
            return new Transform2(
                Position - transform.Position,
                Rotation - transform.Rotation,
                Size - transform.Size,
                MirrorX != transform.MirrorX);
        }

        /// <summary>
        /// Returns a transform that is componentwise multiplication of this with a scalar value.
        /// </summary>
        public Transform2 Multiply(float scalar)
        {
            return new Transform2(
                Position * scalar,
                Rotation * scalar,
                Size * scalar,
                MirrorX);
        }

        public Transform2 SetPosition(Vector2 position) => new Transform2(position, Rotation, Size, MirrorX);
        public Transform2 SetRotation(float rotation) => new Transform2(Position, rotation, Size, MirrorX);
        public Transform2 SetSize(float size) => new Transform2(Position, Rotation, size, MirrorX);
        public Transform2 SetMirrorX(bool mirrorX) => new Transform2(Position, Rotation, Size, mirrorX);

        public Transform2 AddPosition(Vector2 position) => new Transform2(position + Position, Rotation, Size, MirrorX);
        public Transform2 AddRotation(float rotation) => new Transform2(Position, rotation + Rotation, Size, MirrorX);
        public Transform2 AddSize(float size) => new Transform2(Position, Rotation, size + Size, MirrorX);

        public Transform2 SetScale(Vector2 scale)
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

            var transform = SetSize(scale.Y).SetMirrorX(Math.Sign(scale.X) != Math.Sign(scale.Y));
            Debug.Assert(transform.Scale == scale);
            return transform;
        }

        public Transform2 SetScale(float size, bool mirrorX, bool mirrorY)
        {
            var newSize = mirrorY ?
                -size :
                size;
            return SetSize(newSize).SetMirrorX(mirrorX != mirrorY);
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
            return new Transform2(linearVelocity, angularVelocity, scalarVelocity);
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
