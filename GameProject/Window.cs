using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Window : GameWindow
    {
        ControllerGame controller;
        public InputExt InputExt;
        public Window()
            : base((int) 800, (int) 600, new GraphicsMode(32, 24, 8, 1), "Game", GameWindowFlags.FixedWindow)
        {
            InputExt = new InputExt(this);
            controller = new ControllerGame(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            controller.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            controller.OnRenderFrame(e);
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (Focused)
            {
                InputExt.Update();
            }

            if (InputExt.KeyPress(Key.F4))
            {
                ToggleFullScreen();
            }
            else if (InputExt.KeyPress(Key.Escape))
            {
                Exit();
            }
            controller.OnUpdateFrame(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            controller.OnClosing(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            controller.OnResize(e, ClientSize);
        }

        private void ToggleFullScreen()
        {
            if (WindowState == OpenTK.WindowState.Normal)
            {
                WindowState = OpenTK.WindowState.Fullscreen;
                ClientSize = new Size(Width, Height);
            }
            else if (WindowState == OpenTK.WindowState.Fullscreen)
            {
                WindowState = OpenTK.WindowState.Normal;
                ClientSize = new Size(800, 600);
            }
        }
    }
}
