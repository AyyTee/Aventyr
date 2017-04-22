using Game.Rendering;
using OpenTK.Input;
using OpenTK;
using OpenTK.Platform;

namespace Game
{
    public class Input : IInput
    {
        KeyboardState _keyCurrent, _keyPrevious;
        MouseState _mouseCurrent, _mousePrevious;
        Vector2 _mousePos;
        float _wheelDelta;
        float _wheelDeltaPrev;
        public Vector2 MousePos { get; private set; }
        public Vector2 MousePosPrev { get; private set; }
        bool _mouseInside;
        public bool Focus { get; private set; }

        readonly GameWindow _ctx;
        readonly GLControl _control;
        public bool MouseInside { get; private set; }
        public Input(GameWindow ctx)
        {
            _ctx = ctx;
            Update(true);
            _ctx.MouseEnter += delegate { _mouseInside = true; };
            _ctx.MouseLeave += delegate { _mouseInside = false; };
            _ctx.MouseWheel += Ctx_MouseWheel;
        }

        public Input(GLControl control)
        {
            _control = control;
            control.MouseMove += control_MouseMove;
            control.MouseLeave += delegate { _mouseInside = false; };
            control.MouseEnter += delegate { _mouseInside = true; };
            control.MouseWheel += control_MouseWheel;
        }

        void Ctx_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _wheelDelta += e.DeltaPrecise;
        }

        void control_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (((GLControl)sender).Focus())
            {
                _wheelDelta += (float)e.Delta / 120;
            }
        }

        void control_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _mousePos = new Vector2(e.X, e.Y);
        }

        public Vector2 GetMouseWorldPos(ICamera2 camera, Vector2 canvasSize)
        {
            return camera.ScreenToWorld(MousePos, canvasSize);
        }

        public void Update(bool hasFocus)
        {
            Focus = hasFocus;
            _keyPrevious = _keyCurrent;
            _keyCurrent = Keyboard.GetState();
            _mousePrevious = _mouseCurrent;
            _mouseCurrent = Mouse.GetState();
            MouseInside = _mouseInside;
            MousePosPrev = MousePos;
            MousePos = _mousePos;
            _wheelDeltaPrev = _wheelDelta;
            _wheelDelta = 0;
            if (_ctx != null)
            {
                MousePos = new Vector2(_ctx.Mouse.X, _ctx.Mouse.Y);
            }
        }

        public bool KeyDown(Key input)
        {
            return _keyCurrent.IsKeyDown(input) && Focus;
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
            return _keyCurrent.IsKeyDown(input) && 
                !_keyPrevious.IsKeyDown(input) && 
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
            return !_keyCurrent.IsKeyDown(input) && 
                _keyPrevious.IsKeyDown(input) && 
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
            return _mouseCurrent.IsButtonDown(input) && Focus;
        }

        public bool MousePress(MouseButton input)
        {
            return _mouseCurrent.IsButtonDown(input) && 
                !_mousePrevious.IsButtonDown(input) && 
                Focus;
        }

        public bool MouseRelease(MouseButton input)
        {
            return !_mouseCurrent.IsButtonDown(input) && 
                _mousePrevious.IsButtonDown(input) && 
                Focus;
        }

        public float MouseWheelDelta()
        {
            return _wheelDeltaPrev;
        }
    }
}
