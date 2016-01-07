using Game;
using OpenTK;
using System;
using System.Diagnostics;
using System.Threading;

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
        public int UpdatesPerSecond { get; private set; }
        public int MillisecondsPerStep { get { return 1000 / UpdatesPerSecond; } }
        Stopwatch stopwatch = new Stopwatch();
        readonly GLControl _control;
        readonly Controller _loopControl;
        RollingAverage _average;
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
            UpdatesPerSecond = updatesPerSecond;
            _average = new RollingAverage(60, MillisecondsPerStep);
            _control.Context.MakeCurrent(null);
            Thread = new Thread(new ThreadStart(Loop));
            Thread.Start();
        }

        /// <summary>
        /// Get the average time between loops in milliseconds
        /// </summary>
        public float GetAverage()
        {
            return _average.GetAverage();
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
                    stopwatch.Stop();
                    _average.Enqueue(stopwatch.ElapsedMilliseconds);
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
                    int sleepLength = Math.Max(0, MillisecondsPerStep - (int)stopwatch.ElapsedMilliseconds);
                    stopwatch.Start();
                    Thread.Sleep(sleepLength);
                }
                _control.Context.MakeCurrent(null);
                IsRunning = false;
                IsStopping = false;
            }
        }
    }
}
