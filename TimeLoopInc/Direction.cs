using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public enum Direction
    {
        Right = 0, Up = 1, Left = 2, Down = 3
    }

    public static class DirectionEx
    {
        public static Vector2i ToVector(Direction? heading)
        {
            switch (heading)
            {
                case Direction.Right: return new Vector2i(1, 0);
                case Direction.Up: return new Vector2i(0, 1);
                case Direction.Left: return new Vector2i(-1, 0);
                case Direction.Down: return new Vector2i(0, -1);
                case null: return new Vector2i();
                default: throw new Exception();
            }
        }

        /// <summary>
        /// Returns a rotated copy of direction.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="turnAmount">Number of 90 degree turns. Positive values turn left.</param>
        /// <returns></returns>
        public static Direction Rotate(this Direction direction, int turnAmount)
        {
            return (Direction)MathExt.ValueWrap((int)direction + turnAmount, 4);
        }
    }
}
