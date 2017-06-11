using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    [DataContract]
    public struct GridAngle
    {
        /// <summary>
        /// Angle in terms of number of 1/4 rotations.
        /// </summary>
        [DataMember]
        public readonly int Value;
        /// <summary>
        /// Angle in radians.
        /// </summary>
        public double Radians => Value * Math.PI / 2;
        public Vector2i Vector
        {
            get
            {
                switch (MathEx.ValueWrap(Value, CardinalDirections))
                {
                    case 0: return new Vector2i(1, 0);
                    case 1: return new Vector2i(0, -1);
                    case 2: return new Vector2i(-1, 0);
                    case 3: return new Vector2i(0, 1);
                    default: throw new Exception("Execution should not have reached this point.");
                }
            }
        }
        public const int CardinalDirections = 4;

        public GridAngle(int gridAngle) => Value = gridAngle;

        public static GridAngle Right => new GridAngle(0);
        public static GridAngle Down => new GridAngle(1);
        public static GridAngle Left => new GridAngle(2);
        public static GridAngle Up => new GridAngle(3);
    }
}
