using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WPFControls;

namespace Editor
{
    /// <summary>
    /// Drives a GLControl instance to redraw at a specified frequency.
    /// </summary>
    public class GLLoop
    {
        public Thread Thread { get; private set; }
        bool _resize = false;
        bool _focused;
        int millisecondsPerStep;
        Stopwatch stopwatch = new Stopwatch();
        GLControl _control;
        Controller _loopControl;
        public bool IsStopping { get; private set; }
        public bool IsRunning { get; private set; }

        public GLLoop(GLControl control, Controller loopControl)
        {
            _control = control;
            IsRunning = false;
            IsStopping = false;
            _loopControl = loopControl;
            _loopControl.OnLoad(new EventArgs());
            /*_control.GotFocus += delegate { _focused = true; };
            _control.LostFocus += delegate { _focused = false; };*/
            _control.MouseEnter += delegate { _focused = true; };
            _control.MouseLeave += delegate { _focused = false; };
            _control.Resize += delegate { _resize = true; };
        }

        /// <summary>
        /// Starts the logic loop with a specified number of updates per second.  The loop will continue until Stop is called.
        /// </summary>
        /// <param name="updatesPerSecond"></param>
        public void Run(int updatesPerSecond)
        {
            Debug.Assert(updatesPerSecond > 0 && updatesPerSecond <= 200, "Updates per second must be between 0 and 200.");
            Debug.Assert(IsRunning == false);
            millisecondsPerStep = 1000 / updatesPerSecond;
            _control.Context.MakeCurrent(null);
            Thread = new Thread(new ThreadStart(Loop));
            Thread.Start();
        }

        /// <summary>
        /// Tell the loop to stop.  This won't immediately stop it however.  
        /// Instead, IsStopping or IsRunning can be used to verify when the loop has stopped.
        /// </summary>
        public void Stop()
        {
            IsStopping = true;
        }

        /// <summary>
        /// Logic loop that drives the GL canvas.
        /// </summary>
        void Loop()
        {
            lock (this)
            {
                IsRunning = true;
                _control.MakeCurrent();
                while (!IsStopping)
                {
                    stopwatch.Restart();
                    if (_resize)
                    {
                        _loopControl.OnResize(new EventArgs(), _control.ClientSize);
                        _resize = false;
                    }
                    if (_focused)
                    {
                        _loopControl.InputExt.Update();
                    }
                    _loopControl.OnUpdateFrame(new FrameEventArgs());
                    _loopControl.OnRenderFrame(new FrameEventArgs());

                    _control.SwapBuffers();
                    _control.Invalidate();

                    stopwatch.Stop();
                    int sleepLength = Math.Max(0, millisecondsPerStep - (int)stopwatch.ElapsedMilliseconds);
                    Thread.Sleep(sleepLength);
                }
                _control.Context.MakeCurrent(null);
                IsRunning = false;
                IsStopping = false;
            }
        }
    }
}
