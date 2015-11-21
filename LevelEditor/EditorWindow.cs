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
        bool loaded = false;
        Controller controller;
        Random rand = new Random();
        Stopwatch stopwatch = new Stopwatch();
        long elapsedTime;
        long microsecondsPerStep = 1000000 / 60;
        GLControl GLControl;
        bool Running;
        Thread GameLoop;
        public EditorWindow()
        {
            InitializeComponent();

            GLControl = new GLControl(new GraphicsMode(32, 24, 8, 1));
            SuspendLayout();
            GLControl.BackColor = System.Drawing.Color.Black;
            GLControl.Location = canvasPlaceholder.Location;
            //GLControl.Parent = canvasPlaceholder;
            GLControl.Name = "GLControl";
            GLControl.Size = canvasPlaceholder.Size;
            GLControl.VSync = false;
            Controls.Add(GLControl);
            ResumeLayout(false);
            PerformLayout();
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            if (!loaded)
                return;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            loaded = true;
            //glControl1.
            controller = new Controller(GLControl.ClientSize, new InputExt());
            controller.OnLoad(new EventArgs());
            stopwatch.Start();
            //Application.Idle += ApplicationIdle;
            Application.ApplicationExit += ApplicationExit;
            Application.ThreadExit += Window_Closing;
            Running = true;
            GLControl.Context.MakeCurrent(null);
            GameLoop = new Thread(new ThreadStart(ApplicationIdle));
            GameLoop.Start();
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            lock ("Loop")
            {
                Running = false;
            }
        }

        private void ApplicationExit(object sender, EventArgs e)
        {
            lock("Loop")
            {
                Running = false;
            }
            //Application.Idle -= ApplicationIdle;
        }

        //void ApplicationIdle(object sender, EventArgs e)
        void ApplicationIdle()
        {
            GLControl.MakeCurrent();
            while (Running)
            {
                stopwatch.Stop();
                elapsedTime += stopwatch.ElapsedMilliseconds * 1000;
                stopwatch.Restart();
                Console.Write(elapsedTime);
                Console.WriteLine();
                if (elapsedTime > microsecondsPerStep)
                {
                    lock("Loop")
                    {
                        if (!Running)
                        {
                            break;
                        }
                        controller.OnUpdateFrame(new FrameEventArgs(elapsedTime));
                        controller.OnRenderFrame(new FrameEventArgs(elapsedTime));
                        GLControl.SwapBuffers();
                        GLControl.Invalidate();
                        elapsedTime = 0;
                    }
                }
                Thread.Sleep(2);
            }
        }
    }
}