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
        public Vector2i StartPosition { get; }
        [DataMember]
        public int StartTime { get; }
        [DataMember]
        public int EndTime { get; set; } = int.MaxValue;
        [DataMember]
        public List<Input> Input { get; } = new List<Input>();

        public Player(Vector2i startPosition, int startTime)
        {
            StartPosition = startPosition;
            StartTime = startTime;
        }

        public Input GetInput(int time)
        {
            return Input.ElementAtOrDefault(time - StartTime - 1) ?? new Input(null);
        }

        public IGridEntityInstant CreateInstant() => new PlayerInstant(StartPosition);

        public IGridEntity DeepClone() => (Player)MemberwiseClone();
    }
}
