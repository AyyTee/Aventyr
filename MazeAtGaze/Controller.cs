using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using OpenTK;
using Tobii.EyeX.Framework;
using Game.Rendering;
using Game.Common;
using EyeXFramework;
using System.Diagnostics;
using Tobii.EyeX.Client;
using System.Runtime.InteropServices;
using OpenTK.Graphics;
using Game.Models;

namespace MazeAtGaze
{
    public class Controller : IUpdateable
    {
        EyeXHost _host;
        Vector2d PlayerPos;
        Vector2d BallPos;
        Vector2d BallVelocity;
        Vector2d NextPlayerPos;
        object _gazeStreamLock = new object();
        readonly IVirtualWindow _window;
        ICamera2 _camera;
        Model _level;
        Model _finish;
        private Model _background;

        public Controller(IVirtualWindow window, EyeXHost host)
        {
            _window = window;
            _host = host;
            _host.Start();
            var gazeStream = _host.CreateGazePointDataStream(GazePointDataMode.LightlyFiltered);
            gazeStream.Next += GazeStream_Next;

            _camera = new SimpleCamera(new Transform2(new Vector2(), 0, 20), (float)_window.CanvasSize.XRatio);

            _level = ModelFactory.CreatePolygon(
                new[] {
                    new Vector2(0, 250),
                    new Vector2(704, 250),
                    new Vector2(644, 471),
                    new Vector2(563, 551),
                    new Vector2(478, 470),
                    new Vector2(89, 470),
                    new Vector2(90, 692),
                    new Vector2(326, 700),
                    new Vector2(400, 630),
                    new Vector2(460, 700),
                    new Vector2(766, 700),
                    new Vector2(900, 70),
                    new Vector2(0, 70),
                    new Vector2(0, 0),
                    new Vector2(1000, 0),
                    new Vector2(1000, 800),
                    new Vector2(0, 800),
                });
            _level.Transform.Scale = new Vector3(20 / (float)800, -20 / (float)800, 1);
            _level.Transform.Position = new Vector3(-12.5f, 10, 0);
            _level.Color = new Color4(0.2f, 0.2f, 0.2f, 2f);

            _finish = ModelFactory.CreateGrid(new Vector2i(10, 10), new Vector2(0.5f), Color4.GhostWhite, Color4.Black);
            _finish.Transform.Position = new Vector3(-10, -7, -1);

            _background = ModelFactory.CreatePlane(new Vector2(30), new Vector3(-15, -15, -2), Color4.SkyBlue);
        }

        public void Render(double timeDelta)
        {
            _window.Layers.Clear();
            var layer = new Layer();
            _window.Layers.Add(layer);
            layer.Renderables.Add(new Renderable() { Models = new List<Model> { _level, _finish, _background } });
            layer.DrawCircle((Vector2)PlayerPos, 0.1f, Color4.Red, 2);
            layer.DrawCircle((Vector2)BallPos, 1f, Color4.Black, 1);
            //layer.DrawRectangle((Vector2)PlayerPos, (Vector2)PlayerPos + new Vector2(1, 1), Color4.Beige);
            layer.Camera = _camera;

            //var gui = new Layer()
            //{
            //    DepthTest = false,
            //    Camera = new HudCamera2(_window.CanvasSize)
            //};
            //_window.Layers.Add(gui);
            
            //gui.DrawRectangle(new Vector2(100, 100), (Vector2)_window.CanvasSize, Color4.Black);
            //gui.DrawText(_window.Fonts.Inconsolata, new Vector2(_window.CanvasSize.X/2, _window.CanvasSize.Y/2), "test test test test test");
        }

        public void Update(double timeDelta)
        {
            lock (_gazeStreamLock)
            {
                PlayerPos = (Vector2d)_camera.ClipToWorld((Vector2)NextPlayerPos);
            }

            BallVelocity.Y -= 0.0005f;
            var delta = BallPos - PlayerPos;
            BallVelocity += delta.Normalized() * Math.Max(0, 3 - delta.Length) / 1000;
            BallPos += BallVelocity;

            if (_window.ButtonDown(OpenTK.Input.Key.R))
            {
                BallPos = new Vector2d(-10, 7);
                BallVelocity = new Vector2d();
            }
        }

        void GazeStream_Next(object sender, GazePointEventArgs e)
        {
            var handle = Process.GetCurrentProcess().MainWindowHandle;
            var rect = GetWindowBounds(handle);
            Vector2d pos = Vector2d.Divide(new Vector2d(e.X - rect.X, e.Y - rect.Y), _window.DpiScale);
            pos = Vector2d.Divide(pos - (Vector2d)_window.CanvasPosition, (Vector2d)_window.CanvasSize) * 2 - Vector2d.One;
            pos.Y *= -1;
            lock (_gazeStreamLock)
            {
                NextPlayerPos = pos;
            }
        }

        static Rect GetWindowBounds(IntPtr windowHandle)
        {
            NativeRect nativeNativeRect;
            if (GetWindowRect(windowHandle, out nativeNativeRect))
                return new Rect
                {
                    X = nativeNativeRect.Left,
                    Y = nativeNativeRect.Top,
                    Width = nativeNativeRect.Right,
                    Height = nativeNativeRect.Bottom
                };

            return new Rect { X = 0, Y = 0, Width = 1000, Height = 1000 };
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, out NativeRect nativeRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
