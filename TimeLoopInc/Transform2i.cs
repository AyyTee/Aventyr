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

        public Transform2i(Vector2i position, GridAngle gridRotation = new GridAngle(), int size = 1, bool mirrorX = false)
        {
            Debug.Assert(size != 0);
            Position = position;
            Direction = gridRotation;
            Size = size;
            MirrorX = mirrorX;
        }

        public Transform2 ToTransform2()
        {
            return new Transform2((Vector2)Position, Size, (float)Direction.Radians, MirrorX);
        }

        public static Transform2i RoundTransform2(Transform2 transform)
        {
            return new Transform2i(
                (Vector2i)transform.Position.SnapToGrid(Vector2.One), 
                new GridAngle((int)MathEx.Round(transform.Rotation, Math.PI / 2)), 
                (int)Math.Round(transform.Size), 
                transform.MirrorX);
        }

        public Transform2i SetPosition(Vector2i position) => new Transform2i(position, Direction, Size, MirrorX);
        public Transform2i SetRotation(GridAngle rotation) => new Transform2i(Position, rotation, Size, MirrorX);
        public Transform2i SetSize(int size) => new Transform2i(Position, Direction, size, MirrorX);
        public Transform2i SetMirrorX(bool mirrorX) => new Transform2i(Position, Direction, Size, mirrorX);
    }
}
