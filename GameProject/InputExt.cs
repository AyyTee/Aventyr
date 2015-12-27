using System;
using System.Windows;
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
        KeyboardState KeyCurrent, KeyPrevious;
        MouseState MouseCurrent, MousePrevious;
        public Vector2 _mousePos;
        public Vector2 MousePos { get; private set; }
        public Vector2 MousePosPrev { get; private set; }
        bool _mouseInside;

        public enum KeyBoth { Control, Shift, Alt }

        GameWindow Ctx;
        GLControl Control;
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
            Control = control;
            control.MouseMove += control_MouseMove;
            control.MouseLeave += delegate { _mouseInside = false; };
            control.MouseEnter += delegate { _mouseInside = true; };
        }

        private void control_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _mousePos = new Vector2((float)e.X, (float)e.Y);
        }

        public void Update()
        {
            KeyPrevious = KeyCurrent;
            KeyCurrent = Keyboard.GetState();
            MousePrevious = MouseCurrent;
            MouseCurrent = Mouse.GetState();
            //Point mousePoint = System.Windows.Input.Mouse.GetPosition(ParentControl);
            MouseInside = _mouseInside;
            MousePosPrev = MousePos;
            MousePos = _mousePos; //new Vector2((float)mousePoint.X, (float)mousePoint.Y);//
            if (Ctx != null)
            {
                MousePos = new Vector2(Ctx.Mouse.X, Ctx.Mouse.Y);
            }
        }

        public bool KeyDown(Key input)
        {
            return KeyCurrent.IsKeyDown(input);
        }

        public bool KeyDown(KeyBoth input)
        {
            switch (input)
            {
                case KeyBoth.Control:
                    return KeyDown(Key.ControlLeft) || KeyDown(Key.ControlRight);
                case KeyBoth.Shift:
                    return KeyDown(Key.ShiftLeft) || KeyDown(Key.ShiftRight);
                case KeyBoth.Alt:
                    return KeyDown(Key.AltLeft) || KeyDown(Key.AltRight);
                default:
                    return false;
            }
        }

        public bool KeyPress(Key input)
        {
            if (KeyCurrent.IsKeyDown(input) && KeyPrevious.IsKeyDown(input) == false)
            {
                return true;
            }
            return false;
        }

        public bool KeyPress(KeyBoth input)
        {
            switch (input)
            {
                case KeyBoth.Control:
                    return KeyPress(Key.ControlLeft) || KeyPress(Key.ControlRight);
                case KeyBoth.Shift:
                    return KeyPress(Key.ShiftLeft) || KeyPress(Key.ShiftRight);
                case KeyBoth.Alt:
                    return KeyPress(Key.AltLeft) || KeyPress(Key.AltRight);
                default:
                    return false;
            }
        }

        public bool KeyRelease(Key input)
        {
            if (KeyCurrent.IsKeyDown(input) == false && KeyPrevious.IsKeyDown(input))
            {
                return true;
            }
            return false;
        }

        public bool KeyRelease(KeyBoth input)
        {
            switch (input)
            {
                case KeyBoth.Control:
                    return KeyRelease(Key.ControlLeft) || KeyRelease(Key.ControlRight);
                case KeyBoth.Shift:
                    return KeyRelease(Key.ShiftLeft) || KeyRelease(Key.ShiftRight);
                case KeyBoth.Alt:
                    return KeyRelease(Key.AltLeft) || KeyRelease(Key.AltRight);
                default:
                    return false;
            }
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
