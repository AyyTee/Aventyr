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
        public readonly Direction? Heading;

        public Input(Direction? heading)
        {
            Heading = heading;
        }

        public static Input CreateFromKeyboard(IVirtualWindow window)
        {
            if (window.ButtonPress(Key.W))
            {
                return new Input(Direction.Up);
            }
            if (window.ButtonPress(Key.S))
            {
                return new Input(Direction.Down);
            }
            if (window.ButtonPress(Key.A))
            {
                return new Input(Direction.Left);
            }
            if (window.ButtonPress(Key.D))
            {
                return new Input(Direction.Right);
            }
            if (window.ButtonPress(Key.Space))
            {
                return new Input(null);
            }
            return null;
        }
    }
}
