using OpenTK;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Game.Serialization;
using Equ;

namespace Game.Common
{
    [DataContract]
    public partial class Transform2d : MemberwiseEquatable<Transform2d>, IAlmostEqual<Transform2d, double>
    {
        const double UniformScaleEpsilon = 0.0001f;
        const double EqualityEpsilon = 0.0001f;

        [DataMember]
        public Vector2d Position { get; private set; }

        [DataMember]
        public double Rotation { get; private set; }

        [DataMember]
        public double Size { get; private set; } = 1;

        /// <summary>
        /// X-axis mirroring.
        /// </summary>
        [DataMember]
        public bool MirrorX { get; private set; }

        public Vector2d Scale => MirrorX ? new Vector2d(-Size, Size) : new Vector2d(Size, Size);

        public Transform2d()
        {
        }

        public Transform2d(Vector2d position, double rotation = 0, double size = 1, bool mirrorX = false)
        {
            DebugEx.Assert(!Vector2Ex.IsNaN(position));
            DebugEx.Assert(!double.IsNaN(rotation));
            DebugEx.Assert(!double.IsNaN(size) && !double.IsPositiveInfinity(size) && !double.IsNegativeInfinity(size));
            Position = position;
            Size = size;
            Rotation = rotation;
            MirrorX = mirrorX;
        }

        public Transform3d Get3D()
        {
            return new Transform3d(
                new Vector3d(Position), 
                new Vector3d(Scale.X, Scale.Y, 1), 
                new Quaterniond(0, 0, 1, Rotation));
        }

        public Matrix4d GetMatrix()
        {
            return Matrix4d.Scale(new Vector3d(Scale.X, Scale.Y, 1)) * 
                Matrix4d.CreateRotationZ(Rotation) * 
                Matrix4d.CreateTranslation(new Vector3d(Position));
        }

        public Vector2d GetUp() => Vector2Ex.TransformVelocity(new Vector2d(0, 1), GetMatrix());

        public Vector2d GetRight() => Vector2Ex.TransformVelocity(new Vector2d(1, 0), GetMatrix());

        /// <summary>
        /// Returns transform that is this transformed with another Transform2d.  Do not use this for velocity as it will give inappropriate results.
        /// </summary>
        /// <remarks>The method has the following property:
        /// Transform(transform).GetMatrix();
        /// approximately equals 
        /// GetMatrix() * transform.GetMatrix();</remarks>
        public Transform2d Transform(Transform2d transform)
        {
            return new Transform2d(
                Vector2Ex.Transform(Position, transform.GetMatrix()),
                transform.MirrorX ?
                    -Rotation + transform.Rotation :
                    Rotation + transform.Rotation,
                Size * transform.Size,
                MirrorX != transform.MirrorX);
        }

        /// <summary>
        /// Returns an inverted Transform2d instance.  Do not use this for velocity as it will give inappropriate results.
        /// </summary>
        /// <remarks>The method has the following property:
        /// Inverted().GetMatrix();
        /// approximately equals 
        /// GetMatrix().Inverted();</remarks>
        public Transform2d Inverted()
        {
            var invert = new Transform2d();
            if ((Scale.Y < 0) == (Scale.X < 0))
            {
                invert.Rotation = -Rotation;
            }
            else
            {
                invert.Rotation = Rotation;
            }
            invert = invert.SetScale(new Vector2d(1 / Scale.X, 1 / Scale.Y));
            Matrix4d mat = Matrix4d.CreateRotationZ(-Rotation) * Matrix4d.Scale(new Vector3d(invert.Scale));
            invert.Position = Vector2Ex.Transform(-Position, mat);
            DebugEx.Assert(Matrix4Ex.AlmostEqual(GetMatrix().Inverted(), invert.GetMatrix()));
            return invert;
        }

        public Transform2d Add(Transform2d transform)
        {
            return new Transform2d(
                Position + transform.Position,
                Rotation + transform.Rotation,
                Size + transform.Size,
                MirrorX != transform.MirrorX);
        }

        /// <summary>
        /// Returns the result of this minus another transform.
        /// </summary>
        public Transform2d Minus(Transform2d transform)
        {
            return new Transform2d(
                Position - transform.Position,
                Rotation - transform.Rotation,
                Size - transform.Size,
                MirrorX != transform.MirrorX);
        }

        /// <summary>
        /// Returns a transform that is componentwise multiplication of this with a scalar value.
        /// </summary>
        public Transform2d Multiply(double scalar)
        {
            return new Transform2d(
                Position * scalar,
                Rotation * scalar,
                Size * scalar,
                MirrorX);
        }

        public Transform2d WithPosition(Vector2d position) => new Transform2d(position, Rotation, Size, MirrorX);
        public Transform2d WithRotation(double rotation) => new Transform2d(Position, rotation, Size, MirrorX);
        public Transform2d WithSize(double size) => new Transform2d(Position, Rotation, size, MirrorX);
        public Transform2d WithMirrorX(bool mirrorX) => new Transform2d(Position, Rotation, Size, mirrorX);

        public Transform2d AddPosition(Vector2d position) => new Transform2d(position + Position, Rotation, Size, MirrorX);
        public Transform2d AddRotation(double rotation) => new Transform2d(Position, rotation + Rotation, Size, MirrorX);
        public Transform2d AddSize(double size) => new Transform2d(Position, Rotation, size + Size, MirrorX);

        public Transform2d SetScale(Vector2d scale)
        {
            DebugEx.Assert(Vector2Ex.IsReal(scale));
            DebugEx.Assert(scale.X != 0 && scale.Y != 0, "Scale vector must have non-zero components");
            DebugEx.Assert(
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

            var transform = WithSize(scale.Y).WithMirrorX(Math.Sign(scale.X) != Math.Sign(scale.Y));
            DebugEx.Assert(transform.Scale == scale);
            return transform;
        }

        public Transform2d SetScale(double size, bool mirrorX, bool mirrorY)
        {
            var newSize = mirrorY ?
                -size :
                size;
            return WithSize(newSize).WithMirrorX(mirrorX != mirrorY);
        }

        public bool AlmostEqual(Transform2d transform, double delta = EqualityEpsilon)
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

        public bool AlmostEqual(Transform2d transform, double delta, double ratioDelta)
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

        public static Transform2d CreateVelocity() => CreateVelocity(new Vector2d());

        public static Transform2d CreateVelocity(Vector2d linearVelocity, double angularVelocity = 0, double scalarVelocity = 0)
        {
            return new Transform2d(linearVelocity, angularVelocity, scalarVelocity);
        }
    }
}
