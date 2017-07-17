using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TimeLoopInc
{
    [DataContract]
    public class Player : IGridEntity
    {
        [DataMember]
        public Transform2i StartTransform { get; }
        [DataMember]
        public Vector2i PreviousVelocity { get; }
        [DataMember]
        public Transform2i PreviousTransform { get; }
        [DataMember]
        public int PreviousTime { get; }
        [DataMember]
        public int StartTime { get; }
        [DataMember]
        public List<MoveInput> Input { get; private set; } = new List<MoveInput>();

        public Player(Transform2i startTransform, int startTime)
        {
            DebugEx.Assert(startTime > int.MinValue);
            StartTransform = startTransform;
            StartTime = startTime;
        }

        public Player(
            Transform2i startTransform,
            int startTime,
            Vector2i previousVelocity,
            Transform2i previousTransform,
            int previousTime)
            : this(startTransform, startTime)
        {
            PreviousVelocity = previousVelocity;
            PreviousTransform = previousTransform;
            PreviousTime = previousTime;
        }

        public MoveInput GetInput(int time)
        {
            return Input.ElementAtOrDefault(time - StartTime);
        }

        public IGridEntityInstant CreateInstant() => new PlayerInstant(StartTransform, PreviousVelocity);

        public IGridEntity DeepClone()
        {
            var clone = (Player)MemberwiseClone();
            clone.Input = Input.ToList();
            return clone;
        }
    }
}
