using System;
using System.Runtime.Serialization;
using Game.Common;

namespace TimeLoopInc
{
    [DataContract]
    public class SelectInput : IInput
    {
        [DataMember]
        public Vector2i Position { get; }
        [DataMember]
        public int Time { get; }

        public SelectInput(Vector2i position, int time)
        {
            Position = position;
            Time = time;
        }
    }
}
