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

        public static MoveInput CreateFromKeyboard(IVirtualWindow window)
        {
            if (window.ButtonPress(Key.W))
            {
                return new MoveInput(GridAngle.Up);
            }
            if (window.ButtonPress(Key.S))
            {
                return new MoveInput(GridAngle.Down);
            }
            if (window.ButtonPress(Key.A))
            {
                return new MoveInput(GridAngle.Left);
            }
            if (window.ButtonPress(Key.D))
            {
                return new MoveInput(GridAngle.Right);
            }
            if (window.ButtonPress(Key.Space))
            {
                return new MoveInput(null);
            }
            return null;
        }
    }
}
