using Game;
using Game.Common;
using Game.Rendering;
using OpenTK;
using OpenTK.Input;
using System;
using System.Diagnostics;
using System.Threading;

namespace EditorLogic
{
    /// <summary>
    /// Drives a GLControl instance to redraw at a specified frequency.
    /// </summary>
    public class GlLoop
    {
        public Thread Thread { get; private set; }
        volatile bool _resize = false;
        volatile bool _focused;
        volatile int _updatesPerSecond = -1;
        volatile bool _isStopping;
        volatile bool _isRunning;
        Stopwatch _stopwatch = new Stopwatch();
        readonly GLControl _control;
        readonly ControllerEditor _loopControl;
        RollingAverage _average;
        readonly EditorVirtualWindow _window;

        public int UpdatesPerSecond { get { return _updatesPerSecond; } private set { _updatesPerSecond = value; } }
        public int MillisecondsPerStep => 1000 / UpdatesPerSecond;
        public bool IsStopping { get { return _isStopping; } private set { _isStopping = value; } }
        public bool IsRunning { get { return _isRunning; } private set { _isRunning = value; } }

        public GlLoop(GLControl control, ControllerEditor loopControl, EditorVirtualWindow window)
        {
            _control = control;
            IsRunning = false;
            IsStopping = false;
            _loopControl = loopControl;
            _window = window;
            /*_control.GotFocus += delegate { _focused = true; };
            _control.LostFocus += delegate { _focused = false; };*/
            _control.MouseEnter += delegate { _focused = true; };
            _control.MouseLeave += delegate { _focused = false; };
            _control.Resize += delegate { _resize = true; };
            
            //_loopControl.OnLoad(new EventArgs());
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
            //_control.Context.MakeCurrent(null);
            Thread = new Thread(new ThreadStart(Loop));
            Thread.Name = "OGL Thread";

            //_loopControl.OnLoad(new EventArgs());

            _control.Context.MakeCurrent(null);
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
                _loopControl.OnLoad(new EventArgs());
                while (!IsStopping)
                {
                    _stopwatch.Stop();
                    _average.Enqueue(_stopwatch.ElapsedMilliseconds);
                    _stopwatch.Restart();
                    if (_resize)
                    {
                        _window.CanvasSize = (Vector2i)_control.ClientSize;
                        _resize = false;
                    }

                    _window.Update(Keyboard.GetState().KeysDown(), Mouse.GetState().ButtonsDown(),  _focused, 0);
                    _loopControl.Update();
                    _window.Renderer.Render();

                    _control.SwapBuffers();
                    _control.Invalidate();

                    _stopwatch.Stop();
                    int sleepLength = Math.Max(0, MillisecondsPerStep - (int)_stopwatch.ElapsedMilliseconds);
                    _stopwatch.Start();
                    Thread.Sleep(sleepLength);
                }
                _control.Context.MakeCurrent(null);
                IsRunning = false;
                IsStopping = false;
            }
        }
    }
}
