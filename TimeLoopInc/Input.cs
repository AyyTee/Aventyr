using Game.Rendering;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class Input
    {
        public readonly GridAngle? Direction;

        public Input(GridAngle? direction)
        {
            Direction = direction;
        }

        public static Input CreateFromKeyboard(IVirtualWindow window)
        {
            if (window.ButtonPress(Key.W))
            {
                return new Input(GridAngle.Up);
            }
            if (window.ButtonPress(Key.S))
            {
                return new Input(GridAngle.Down);
            }
            if (window.ButtonPress(Key.A))
            {
                return new Input(GridAngle.Left);
            }
            if (window.ButtonPress(Key.D))
            {
                return new Input(GridAngle.Right);
            }
            if (window.ButtonPress(Key.Space))
            {
                return new Input(null);
            }
            return null;
        }
    }
}
