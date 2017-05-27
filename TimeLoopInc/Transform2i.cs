using Equ;
using Game.Common;
using Game.Serialization;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    [DataContract]
    public class Transform2i : MemberwiseEquatable<Transform2i>
    {
        [DataMember]
        public Vector2i Position { get; private set; }
        [DataMember]
        public GridAngle Direction { get; private set; }
        [DataMember]
        public int Size { get; private set; }
        [DataMember]
        public bool MirrorX { get; private set; }

        public double Angle => Direction.Radians;

        public Vector2i Scale => MirrorX ? new Vector2i(-Size, Size) : new Vector2i(Size, Size);

        public Transform2i(Vector2i position = new Vector2i(), GridAngle gridRotation = new GridAngle(), int size = 1, bool mirrorX = false)
        {
            Debug.Assert(size != 0);
            Position = position;
            Direction = gridRotation;
            Size = size;
            MirrorX = mirrorX;
        }

        public Transform2d ToTransform2d()
        {
            return new Transform2d((Vector2d)Position, (float)Direction.Radians, Size, MirrorX);
        }

        public static Transform2i RoundTransform2d(Transform2d transform)
        {
            return new Transform2i(
                (Vector2i)transform.Position.Round(Vector2d.One), 
                new GridAngle((int)(transform.Rotation / (Math.PI / 2))), 
                (int)Math.Round(transform.Size), 
                transform.MirrorX);
        }

        public Transform2i SetPosition(Vector2i position) => new Transform2i(position, Direction, Size, MirrorX);
        public Transform2i SetRotation(GridAngle rotation) => new Transform2i(Position, rotation, Size, MirrorX);
        public Transform2i SetSize(int size) => new Transform2i(Position, Direction, size, MirrorX);
        public Transform2i SetMirrorX(bool mirrorX) => new Transform2i(Position, Direction, Size, mirrorX);

        public Matrix4d GetMatrix()
        {
            return Matrix4d.Scale(new Vector3d(Scale.X, Scale.Y, 1)) *
                Matrix4d.CreateRotationZ(Direction.Radians) *
                Matrix4d.CreateTranslation(new Vector3d((Vector2d)Position));
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
    }
}
