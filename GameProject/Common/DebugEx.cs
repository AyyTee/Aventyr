using System;
using System.Diagnostics;

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
    }
}
