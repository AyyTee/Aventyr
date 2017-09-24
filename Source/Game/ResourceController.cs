using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using Game.Rendering;
using Cgen.Audio;
using OpenTK.Input;
using OpenTK;
using OpenTK.Graphics;
using System.Diagnostics;
using Game.Common;
using Newtonsoft.Json;
using Game.Serialization;

namespace Game
{
    public class ResourceController
    {
        public List<int> TextureGarbage = new List<int>();

        readonly GameWindow _window;

        public Vector2i ClientSize => (Vector2i)_window.ClientSize;
        public float DpiScale { get; }

        public IRenderer Renderer { get; private set; }

        public Resources Resources;

        bool _lockInput = false;
        string KeyString = "";
        KeyboardState _virtualKeyboardState = new KeyboardState();
        MouseState _virtualMouseState = new MouseState();
        Vector2 _virtualMousePos = new Vector2();

        KeyboardState _keyboardStatePrevious = new KeyboardState();
        KeyboardState _keyboardState = new KeyboardState();
        MouseState _mouseState = new MouseState();
        Vector2 _mousePos = new Vector2();

        readonly List<ControllerData> _controllers = new List<ControllerData>();
        readonly Stopwatch _stopwatch = new Stopwatch();

        public static readonly GraphicsMode DefaultGraphics = new GraphicsMode(32, 24, 8, 0);

        readonly SoundSystem _soundSystem;
        bool _soundEnabled;

        public ResourceController(Vector2i windowSize, string windowName = "Game")
        {
            _window = GetWindow(windowSize, windowName);

            DpiScale = ClientSize.X / (float)windowSize.X;

            var data = File.ReadAllText(Path.Combine(Resources.ResourcePath, "Assets.json"));
            Resources = Serializer.Deserialize<Resources>(data);
            Renderer = new Renderer(() => (Vector2i)_window.ClientSize, Resources);
            

            _soundEnabled = false;
            if (_soundEnabled)
            {
                _soundSystem = new SoundSystem();
                _soundSystem.Initialize();
                _soundSystem.Start();
            }

            _window.UpdateFrame += (_, __) => { Update(); };
            _window.RenderFrame += (_, __) => { Render(); };
            _window.KeyPress += (_, e) => { KeyString += e.KeyChar; };
            _window.KeyDown += (_, e) =>
            {
                if (e.Key == Key.BackSpace)
                {
                    KeyString += '\b';
                }
            };
        }

        public static GameWindow GetWindow(Vector2i size, string windowName = "")
        {
            var window = new GameWindow(
                size.X,
                size.Y,
                DefaultGraphics,
                windowName,
                GameWindowFlags.FixedWindow,
                DisplayDevice.Default,
                3,
                3,
                GraphicsContextFlags.ForwardCompatible);
            DebugEx.GlAssert();
            return window;
        }

        public void Run()
        {
            _stopwatch.Start();
            _window.Run(60, 60);
        }

        void Render()
        {
            foreach (var controller in _controllers)
            {
                double time = _stopwatch.ElapsedMilliseconds / 1000.0;
                double timeDelta = time - controller.LastRender;
                controller.LastRender = time;
                controller.Controller.Render(timeDelta);
            }

            Renderer.Render();
            _window.SwapBuffers();
        }

        void Update()
        {
            bool hasFocus = _window.Focused;

            _keyboardStatePrevious = _keyboardState;

            _keyboardState = Keyboard.GetState();
            _mouseState = Mouse.GetState();
            _mousePos = new Vector2(_window.Mouse.X, _window.Mouse.Y);

            if (_keyboardState.IsKeyDown(Key.ControlLeft) && _keyboardStatePrevious.IsKeyUp(Key.I) && _keyboardState.IsKeyDown(Key.I))
            {
                _lockInput = !_lockInput;
            }

            if (!_lockInput)
            {
                _virtualKeyboardState = _keyboardState;
                _virtualMouseState = _mouseState;
                _virtualMousePos = _mousePos;
            }

            //if (keyboardState.IsKeyDown )
            //if (Input.KeyPress(Key.F4))
            //{
            //    ToggleFullScreen();
            //}
            if (_keyboardState.IsKeyDown(Key.Escape))
            {
                _window.Exit();
            }

            lock (Texture.LockDelete)
            {
                foreach (int iboElement in TextureGarbage.ToArray())
                {
                    int a = iboElement;
                    GL.DeleteTextures(1, ref a);
                }
                TextureGarbage.Clear();
            }

            foreach (var window in Renderer.Windows.OfType<VirtualWindow>())
            {
                window.Update(
                    KeyString,
                    _virtualKeyboardState.KeysDown(),
                    _virtualMouseState.ButtonsDown(),
                    new Vector2(_virtualMousePos.X - window.CanvasPosition.X, _virtualMousePos.Y/* + window.CanvasPosition.Y*/),
                    hasFocus,
                    _virtualMouseState.WheelPrecise);
            }
            KeyString = "";

            foreach (var controller in _controllers)
            {
                double time = _stopwatch.ElapsedMilliseconds / 1000.0;
                double timeDelta = time - controller.LastUpdate;
                controller.LastUpdate = time;
                controller.Controller.Update(timeDelta);
            }
        }

        public void AddController(IUpdateable controller)
        {
            _controllers.Add(new ControllerData { Controller = controller });
        }

        void ToggleFullScreen()
        {
            if (_window.WindowState == WindowState.Normal)
            {
                _window.WindowState = WindowState.Fullscreen;
                _window.ClientSize = new System.Drawing.Size(800, 600);
            }
            else if (_window.WindowState == WindowState.Fullscreen)
            {
                _window.WindowState = WindowState.Normal;
                _window.ClientSize = new System.Drawing.Size(800, 600);
            }
        }

        public void Exit() => _window.Exit();

        class ControllerData
        {
            public IUpdateable Controller;
            public double LastUpdate;
            public double LastRender;
        }
    }
}
