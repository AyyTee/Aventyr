using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Diagnostics;

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
        public int StartTime { get; }
        [DataMember]
        int _endTime = int.MaxValue;
        public int EndTime
        {
            get { return _endTime; }
            set
            {
                Debug.Assert(value >= StartTime);
                _endTime = value;
            }
        }
        [DataMember]
        public List<Input> Input { get; } = new List<Input>();

        public Player(Transform2i startPosition, int startTime, Vector2i previousVelocity = new Vector2i())
        {
            StartTransform = startPosition;
            PreviousVelocity = previousVelocity;
            StartTime = startTime;
        }

        public Input GetInput(int time)
        {
            return Input.ElementAtOrDefault(time - StartTime) ?? new Input(null);
        }

        public IGridEntityInstant CreateInstant() => new PlayerInstant(StartTransform, PreviousVelocity);

        public IGridEntity DeepClone() => (Player)MemberwiseClone();
    }
}
