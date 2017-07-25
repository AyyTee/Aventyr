using Equ;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc.Editor
{
    public class HotKey : MemberwiseEquatable<HotKey>
    {
        public bool Control { get; }
        public bool Shift { get; }
        public bool Alt { get; }
        public Key Key { get; }

        public HotKey(Key key, bool control = false, bool shift = false, bool alt = false)
        {
            Key = key;
            Control = control;
            Shift = shift;
            Alt = alt;
        }
    }
}
