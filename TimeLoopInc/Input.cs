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
                return new Input(new GridAngle(3));
            }
            if (window.ButtonPress(Key.S))
            {
                return new Input(new GridAngle(1));
            }
            if (window.ButtonPress(Key.A))
            {
                return new Input(new GridAngle(2));
            }
            if (window.ButtonPress(Key.D))
            {
                return new Input(new GridAngle(0));
            }
            if (window.ButtonPress(Key.Space))
            {
                return new Input(null);
            }
            return null;
        }
    }
}
