using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;

namespace Game
{
    public class InputExt
    {
        private KeyboardState KeyCurrent, KeyPrevious;
        private MouseState MouseCurrent, MousePrevious;
        public Vector2 MousePos { get; private set; }
        bool _mouseInside;
        GameWindow Ctx;
        public bool MouseInside { get; private set; }
        public InputExt(GameWindow ctx)
        {
            Ctx = ctx;
            Update();
            Ctx.MouseEnter += delegate { _mouseInside = true; };
            Ctx.MouseLeave += delegate { _mouseInside = false; };
        }

        public InputExt(GLControl control)
        {
            control.MouseMove += control_MouseMove;
            control.MouseEnter += delegate { _mouseInside = true; };
            control.MouseLeave += delegate { _mouseInside = false; };
        }

        private void control_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MousePos = new Vector2((float)e.X, (float)e.Y);
        }

        public void Update()
        {
            KeyPrevious = KeyCurrent;
            KeyCurrent = Keyboard.GetState();
            MousePrevious = MouseCurrent;
            MouseCurrent = Mouse.GetState();
            MouseInside = _mouseInside;
            if (Ctx != null)
            {
                MousePos = new Vector2(Ctx.Mouse.X, Ctx.Mouse.Y);
            }
        }

        public bool KeyDown(Key Input)
        {
            return KeyCurrent.IsKeyDown(Input);
        }

        public bool KeyPress(Key Input)
        {
            if (KeyCurrent.IsKeyDown(Input) && KeyPrevious.IsKeyDown(Input) == false)
            {
                return true;
            }
            return false;
        }

        public bool KeyRelease(Key Input)
        {
            if (KeyCurrent.IsKeyDown(Input) == false && KeyPrevious.IsKeyDown(Input))
            {
                return true;
            }
            return false;
        }

        public bool MouseDown(MouseButton Input)
        {
            return MouseCurrent.IsButtonDown(Input);
        }

        public bool MousePress(MouseButton Input)
        {
            if (MouseCurrent.IsButtonDown(Input) && MousePrevious.IsButtonDown(Input) == false)
            {
                return true;
            }
            return false;
        }

        public bool MouseRelease(MouseButton Input)
        {
            if (MouseCurrent.IsButtonDown(Input) == false && MousePrevious.IsButtonDown(Input))
            {
                return true;
            }
            return false;
        }

        public float MouseWheelDelta()
        {
            return MouseCurrent.WheelPrecise - MousePrevious.WheelPrecise;
        }
    }
}
