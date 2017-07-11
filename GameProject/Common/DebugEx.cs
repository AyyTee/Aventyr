using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;

namespace Game.Common
{
    public static class DebugEx
    {
        public delegate void FailDelegate(string message);
        public static event FailDelegate FailEvent;

        public static void Assert(bool condition, string message = "")
        {
            if (!condition)
            {
                _fail(message);
            }
        }

        public static void Fail(string message = "")
        {
            _fail(message);
        }

        static void _fail(string message)
        {
            FailEvent?.Invoke(message);
            Debugger.Break();
        }

        public static void GlAssert(string message = "")
        {
            var glError = GL.GetError();
            Assert(glError == ErrorCode.NoError, message);
        }
    }
}
