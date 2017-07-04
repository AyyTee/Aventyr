﻿using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;

namespace Game.Common
{
    public static class DebugEx
    {
        public static void Assert(bool condition, string message = "")
        {
            if (!condition)
            {
                Debugger.Break();
            }
        }

        public static void Fail(string message = "")
        {
            Debugger.Break();
        }

        public static void GlAssert(string message = "")
        {
            var glError = GL.GetError();
            Assert(glError == ErrorCode.NoError);
        }
    }
}
