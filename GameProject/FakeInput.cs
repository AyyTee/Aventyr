using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Rendering;
using OpenTK;
using OpenTK.Input;

namespace Game
{
    public class FakeInput : IInput
    {
        public bool Focus => false;

        public bool MouseInside => false;

        public Vector2 MousePos => new Vector2();

        public Vector2 MousePosPrev => new Vector2();

        public Vector2 GetMouseWorldPos(ICamera2 camera, Vector2 canvasSize)
        {
            return new Vector2();
        }

        public bool KeyDown(Key input)
        {
            return false;
        }

        public bool KeyDown(KeyBoth input)
        {
            return false;
        }

        public bool KeyPress(Key input)
        {
            return false;
        }

        public bool KeyPress(KeyBoth input)
        {
            return false;
        }

        public bool KeyRelease(Key input)
        {
            return false;
        }

        public bool KeyRelease(KeyBoth input)
        {
            return false;
        }

        public bool MouseDown(MouseButton input)
        {
            return false;
        }

        public bool MousePress(MouseButton input)
        {
            return false;
        }

        public bool MouseRelease(MouseButton input)
        {
            return false;
        }

        public float MouseWheelDelta()
        {
            return 0;
        }

        public void Update(bool hasFocus)
        {
        }
    }
}
