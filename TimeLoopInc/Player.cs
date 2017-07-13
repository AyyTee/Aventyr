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
        public Transform2i PreviousTransform { get; }
        [DataMember]
        public int StartTime { get; }
        [DataMember]
        public List<Input> Input { get; } = new List<Input>();

        public Player(
            Transform2i startPosition, 
            int startTime, 
            Vector2i previousVelocity = new Vector2i(), 
            Transform2i previousTransform = null)
        {
            StartTransform = startPosition;
            PreviousVelocity = previousVelocity;
            PreviousTransform = previousTransform;
            StartTime = startTime;
        }

        public Input GetInput(int time)
        {
            return Input.ElementAtOrDefault(time - StartTime);
        }

        public IGridEntityInstant CreateInstant() => new PlayerInstant(StartTransform, PreviousVelocity);

        public IGridEntity DeepClone() => (Player)MemberwiseClone();
    }
}
