using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class Player : IGridEntity
    {
        public Vector2i StartPosition { get; }
        public int StartTime { get; }
        public int EndTime { get; set; } = int.MaxValue;
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
