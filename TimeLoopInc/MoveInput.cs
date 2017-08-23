using Game.Rendering;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using System.Runtime.Serialization;

namespace TimeLoopInc
{
    [DataContract]
    public class MoveInput : IInput
    {
        [DataMember]
        public GridAngle? Direction { get; }

        public MoveInput(GridAngle? direction)
        {
            Direction = direction;
        }

        public static MoveInput FromKeyboard(IVirtualWindow window)
        {
            if (window.ButtonPress(Key.W) || window.ButtonPress(Key.Up))
            {
                return new MoveInput(GridAngle.Up);
            }
            if (window.ButtonPress(Key.S) || window.ButtonPress(Key.Down))
            {
                return new MoveInput(GridAngle.Down);
            }
            if (window.ButtonPress(Key.A) || window.ButtonPress(Key.Left))
            {
                return new MoveInput(GridAngle.Left);
            }
            if (window.ButtonPress(Key.D) || window.ButtonPress(Key.Right))
            {
                return new MoveInput(GridAngle.Right);
            }
            if (window.ButtonPress(Key.Space))
            {
                return new MoveInput(null);
            }
            return null;
        }

        public static List<MoveInput> FromString(string input)
        {
            return input.ToUpper().Select(item =>
            {
                switch (item)
                {
                    case 'W':
                        return new MoveInput(GridAngle.Up);
                    case 'A':
                        return new MoveInput(GridAngle.Left);
                    case 'S':
                        return new MoveInput(GridAngle.Down);
                    case 'D':
                        return new MoveInput(GridAngle.Right);
                    case ' ':
                        return new MoveInput(null);
                    default:
                        DebugEx.Fail($"'{item}' is an invalid character.");
                        return null;
                }
            }).ToList();
        }
    }
}
