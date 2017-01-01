using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Rendering;
using OpenTK;
using OpenTK.Input;

namespace TankGameTests
{
    public class FakeInput : IInput
    {
        public HashSet<Key> KeyCurrent = new HashSet<Key>();
        HashSet<Key> _keyPrevious = new HashSet<Key>();
        public HashSet<MouseButton> MouseCurrent = new HashSet<MouseButton>();
        HashSet<MouseButton> _mousePrevious = new HashSet<MouseButton>();
        public Vector2 _mousePos;
        public float WheelDelta = 0;
        float _wheelDeltaPrev = 0;
        public Vector2 MousePos { get; set; }
        public Vector2 MousePosPrev { get; private set; }

        public bool Focus { get; set; } = true;
        public bool MouseInside { get; set; } = true;

        public FakeInput()
        {
        }

        public Vector2 GetMouseWorldPos(ICamera2 camera, Vector2 canvasSize)
        {
            return CameraExt.ScreenToWorld(camera, MousePos, canvasSize);
        }

        public void Update(bool hasFocus)
        {
            Focus = hasFocus;
            _keyPrevious = new HashSet<Key>(KeyCurrent);
            KeyCurrent = new HashSet<Key>();
            _mousePrevious = new HashSet<MouseButton>(MouseCurrent);
            MouseCurrent = new HashSet<MouseButton>();
            MousePosPrev = MousePos;
            MousePos = _mousePos;
            _wheelDeltaPrev = WheelDelta;
            WheelDelta = 0;
        }

        public bool KeyDown(Key input)
        {
            return KeyCurrent.Contains(input) && Focus;
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
            return KeyCurrent.Contains(input) && 
                !_keyPrevious.Contains(input) && 
                Focus;
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
            return !KeyCurrent.Contains(input) &&
                _keyPrevious.Contains(input) &&
                Focus;
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

        public bool MouseDown(MouseButton input)
        {
            return MouseCurrent.Contains(input) && Focus;
        }

        public bool MousePress(MouseButton input)
        {
            return MouseCurrent.Contains(input) &&
                !_mousePrevious.Contains(input) &&
                Focus;
        }

        public bool MouseRelease(MouseButton input)
        {
            return !MouseCurrent.Contains(input) &&
                _mousePrevious.Contains(input) &&
                Focus;
        }

        public float MouseWheelDelta()
        {
            return _wheelDeltaPrev;
        }
    }
}
