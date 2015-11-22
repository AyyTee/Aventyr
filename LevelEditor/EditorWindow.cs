using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Threading;
using Game;
using System.Diagnostics;
using OpenTK;
using OpenTK.Input;

namespace LevelEditor
{
    public partial class EditorWindow : Form
    {
        Controller controller;
        Stopwatch stopwatch = new Stopwatch();
        int millisecondsPerStep = 1000 / 60;
        Thread GLThread;
        public EditorWindow()
        {
            InitializeComponent();
            fileExit.Click += exitToolStripMenuItem_Click;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            controller = new Controller(glControlExt.ClientSize, new InputExt(glControlExt));
            controller.OnLoad(new EventArgs());
            glControlExt.Context.MakeCurrent(null);
            GLThread = new Thread(new ThreadStart(GLLoop));
            GLThread.Start();
        }

        /// <summary>
        /// Loop that drives the GL canvas. Currently not thread safe when closing the application.
        /// </summary>
        void GLLoop()
        {
            glControlExt.MakeCurrent();
            while (true)
            {
                stopwatch.Restart();
                controller.InputExt.Update();
                controller.OnUpdateFrame(new FrameEventArgs());
                controller.OnRenderFrame(new FrameEventArgs());
                controller.OnResize(new EventArgs(), glControlExt.ClientSize);
                //temporary solution (hopefully) until a proper solution is made for freeing the GL context in a thread safe way
                try { glControlExt.SwapBuffers(); }
                catch {}
                glControlExt.Invalidate();

                stopwatch.Stop();
                int sleepLength = Math.Max(1, millisecondsPerStep - (int)stopwatch.ElapsedMilliseconds);
                Thread.Sleep(sleepLength);
            }
        }
    }
}