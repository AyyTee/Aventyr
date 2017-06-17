﻿using Game.Common;
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
        public int StartTime { get; }
        [DataMember]
        public int EndTime { get; set; } = int.MaxValue;
        [DataMember]
        public List<Input> Input { get; } = new List<Input>();

        public Player(Transform2i startPosition, int startTime)
        {
            StartTransform = startPosition;
            StartTime = startTime;
        }

        public Input GetInput(int time)
        {
            return Input.ElementAtOrDefault(time - StartTime) ?? new Input(null);
        }

        public IGridEntityInstant CreateInstant() => new PlayerInstant(StartTransform);

        public IGridEntity DeepClone() => (Player)MemberwiseClone();
    }
}
