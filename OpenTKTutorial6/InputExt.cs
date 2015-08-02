using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;

namespace Game
{
    class InputExt
    {
        public KeyboardState KeyCurrent, KeyPrevious;
        public MouseState MouseCurrent, MousePrevious;
        private GameWindow Ctx;
        public InputExt(GameWindow Ctx)
        {
            this.Ctx = Ctx;
            KeyCurrent = Keyboard.GetState();
            KeyPrevious = KeyCurrent;
            MouseCurrent = Mouse.GetState();
            MousePrevious = MouseCurrent;
        }
        public void Step() 
        {
            KeyPrevious = KeyCurrent;
            KeyCurrent = Keyboard.GetState();
            MousePrevious = MouseCurrent;
            MouseCurrent = Mouse.GetState();
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
        public Vector2d MousePosition()
        {
            return new Vector2d(Ctx.Mouse.X, Ctx.Mouse.Y);
        }
    }
}
