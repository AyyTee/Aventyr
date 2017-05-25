using OpenTK;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Game.Serialization;

namespace Game.Common
{
    [DataContract]
    public partial class Transform2d : IShallowClone<Transform2d>, IAlmostEqual<Transform2d, double>, IValueEquality<Transform2d>
    {
        [DataMember]
        Vector2d _position;
        [DataMember]
        double _rotation;
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

        public Vector2d Scale => MirrorX ? new Vector2d(-Size, Size) : new Vector2d(Size, Size);

        public Vector2d Position 
        { 
            get { return _position; }
            set 
            {
                Debug.Assert(!Vector2Ex.IsNaN(value));
                _position = value; 
            }
        }

        public Transform2d()
        {
        }

        public Transform2d(Vector2d position, double scale = 1, double rotation = 0, bool mirrorX = false)
        {
            Position = position;
            Size = scale;
            Rotation = rotation;
            MirrorX = mirrorX;
        }

        public Transform2d ShallowClone() => (Transform2d)MemberwiseClone();

        public Transform3d Get3D()
        {
            return new Transform3d(new Vector3d(Position), new Vector3d(Scale.X, Scale.Y, 1), new Quaterniond(0, 0, 1, Rotation));
        }

        public Matrix4d GetMatrix()
        {
            return Matrix4d.Scale(new Vector3d(Scale.X, Scale.Y, 1)) * 
                Matrix4d.CreateRotationZ(Rotation) * 
                Matrix4d.CreateTranslation(new Vector3d(Position));
        }

        public Vector2d GetUp(bool normalize = true) => GetVector(new Vector2d(0, 1), normalize);

        public Vector2d GetRight(bool normalize = true) => GetVector(new Vector2d(1, 0), normalize);

        Vector2d GetVector(Vector2d vector, bool normalize)
        {
            Vector2d[] v = {
                new Vector2d(0, 0),
                vector
            };
            v = Vector2Ex.Transform(v, GetMatrix());
            if (normalize)
            {
                Debug.Assert(!Vector2Ex.IsNaN((v[1] - v[0]).Normalized()), "Unable to normalize 0 length vector.");
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
        public Transform2d Transform(Transform2d transform)
        {
            Transform2d output = ShallowClone();
            if (transform.MirrorX)
            {
                output.Rotation *= -1;
            }
            output.Rotation += transform.Rotation;
            output.Size *= transform.Size;
            output.MirrorX = output.MirrorX != transform.MirrorX;
            output.Position = Vector2Ex.Transform(output.Position, transform.GetMatrix());
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
        public Transform2d Inverted()
        {
            Transform2d invert = new Transform2d();
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
            invert.Position = Vector2Ex.Transform(-Position, mat);
            Debug.Assert(Matrix4Ex.AlmostEqual(GetMatrix().Inverted(), invert.GetMatrix()));
            return invert;
        }

        public Transform2d Add(Transform2d transform)
        {
            Transform2d output = ShallowClone();
            output.Rotation += transform.Rotation;
            output.Size += transform.Size;
            output.MirrorX = output.MirrorX != transform.MirrorX;
            output.Position += transform.Position;
            return output;
        }

        /// <summary>
        /// Returns the result of this minus another transform.
        /// </summary>
        public Transform2d Minus(Transform2d transform)
        {
            Transform2d output = ShallowClone();
            output.Rotation -= transform.Rotation;
            output.Size -= transform.Size;
            output.MirrorX = output.MirrorX != transform.MirrorX;
            output.Position -= transform.Position;
            return output;
        }

        /// <summary>
        /// Returns a transform that is componentwise multiplication of this with a scalar value.
        /// </summary>
        public Transform2d Multiply(double scalar)
        {
            Transform2d output = ShallowClone();
            output.Rotation = Rotation * scalar;
            output.Size = Size * scalar;
            output.Position = Position * scalar;
            return output;
        }

        public void SetScale(Vector2d scale)
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
            return new Transform2d(linearVelocity, scalarVelocity, angularVelocity);
        }

        public bool EqualsValue(Transform2d other)
        {
            return other != null &&
                Position == other.Position &&
                Rotation == other.Rotation &&
                Scale == other.Scale;
        }
    }
}
