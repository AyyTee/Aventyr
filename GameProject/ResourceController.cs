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

namespace Game
{
    public class ResourceController : IClientSizeProvider
    {
        public List<int> TextureGarbage = new List<int>();

        readonly GameWindow _window;

        public Vector2i ClientSize => (Vector2i)_window.ClientSize;

        public IRenderer Renderer { get; private set; }

        public FontAssets Fonts;
        public TextureAssets Textures;

        public IInput Input { get; private set; }

        readonly List<ControllerData> _controllers = new List<ControllerData>();
        readonly Stopwatch _stopwatch = new Stopwatch();

        public static readonly GraphicsMode DefaultGraphics = new GraphicsMode(32, 24, 8, 1);

        /// <summary>
        /// Keep pointless messages from the Poly2Tri library out of the console window.
        /// </summary>
        public static StreamWriter TrashLog = new StreamWriter(Stream.Null);

        readonly SoundSystem _soundSystem;
        bool _soundEnabled;

        public ResourceController(Vector2i windowSize, string windowName = "Game")
        {
            _window = new GameWindow(windowSize.X, windowSize.Y, DefaultGraphics, windowName, GameWindowFlags.FixedWindow);
            Textures = new TextureAssets();
            Renderer = new Renderer(this, Textures);
            Input = new Input(_window);
            Fonts = new FontAssets();
            Textures = new TextureAssets();

            _soundEnabled = false;
            if (_soundEnabled)
            {
                _soundSystem = new SoundSystem();
                _soundSystem.Initialize();
                _soundSystem.Start();
            }

            _window.UpdateFrame += (_, __) => { Update(); };
            _window.RenderFrame += (_, __) => { Render(); };
        }

        public void Run()
        {
            _stopwatch.Start();
            _window.Run(60, 60);
        }

        void Render()
        {
            Renderer.Render();
            _window.SwapBuffers();
        }

        void Update()
        {
            Input.Update(_window.Focused);
            if (Input.KeyPress(Key.F4))
            {
                ToggleFullScreen();
            }
            else if (Input.KeyPress(Key.Escape))
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

        class ControllerData
        {
            public IUpdateable Controller;
            public double LastUpdate;
        }
    }
}
