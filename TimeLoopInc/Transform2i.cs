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
    public class Transform2i : MemberwiseEquatable<Transform2i>, IShallowClone<Transform2i>
    {
        [DataMember]
        public Vector2i Position { get; private set; }
        [DataMember]
        public Direction Rotation { get; private set; }
        [DataMember]
        public int Size { get; private set; }
        [DataMember]
        public bool MirrorX { get; private set; }

        public Vector2i Scale => MirrorX ? new Vector2i(-Size, Size) : new Vector2i(Size, Size);

        public Transform2i(Vector2i position, Direction rotation = Direction.Right, int size = 1, bool mirrorX = false)
        {
            Debug.Assert(size != 0);
            Position = position;
            Rotation = rotation;
            Size = size;
            MirrorX = mirrorX;
        }

        public Transform2 ToTransform2()
        {
            return new Transform2((Vector2)Position, Size, (float)DirectionEx.ToAngle(Rotation), MirrorX);
        }

        public Transform2i ShallowClone() => (Transform2i)MemberwiseClone();

        public static Transform2i RoundTransform2(Transform2 transform)
        {
            return new Transform2i(
                (Vector2i)transform.Position.SnapToGrid(Vector2.One), 
                Direction.Right,//transform.Rotation / (Math.PI / 2), 
                (int)Math.Round(transform.Size), 
                transform.MirrorX);
        }

        public Transform2i SetPosition(Vector2i position) => new Transform2i(position, Rotation, Size, MirrorX);
        public Transform2i SetRotation(Direction rotation) => new Transform2i(Position, rotation, Size, MirrorX);
        public Transform2i SetSize(int size) => new Transform2i(Position, Rotation, size, MirrorX);
        public Transform2i SetMirrorX(bool mirrorX) => new Transform2i(Position, Rotation, Size, mirrorX);
    }
}
