using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Game.Rendering;

namespace Game
{
    public class Window : GameWindow
    {
        public Controller Controller;
        public Input InputExt;
        public Window()
            : base(800, 600, Renderer.DefaultGraphics, "Game", GameWindowFlags.FixedWindow)
        {
            InputExt = new Input(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Context.SwapInterval = 1;
            Controller.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            Controller?.OnRenderFrame(e);
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            InputExt.Update(Focused);
            if (InputExt.KeyPress(Key.F4))
            {
                ToggleFullScreen();
            }
            else if (InputExt.KeyPress(Key.Escape))
            {
                Exit();
            }
            Controller?.OnUpdateFrame(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Controller?.OnClosing(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Controller?.OnResize(e, ClientSize);
        }

        private void ToggleFullScreen()
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Fullscreen;
                ClientSize = new Size(Width, Height);
            }
            else if (WindowState == WindowState.Fullscreen)
            {
                WindowState = WindowState.Normal;
                ClientSize = new Size(800, 600);
            }
        }
    }
}
