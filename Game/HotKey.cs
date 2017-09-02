using Equ;
using OpenTK.Input;

namespace Game
{
    public class Hotkey : MemberwiseEquatable<Hotkey>
    {
        public bool Control { get; }
        public bool Shift { get; }
        public bool Alt { get; }
        public Key Key { get; }

        public Hotkey(Key key, bool control = false, bool shift = false, bool alt = false)
        {
            Key = key;
            Control = control;
            Shift = shift;
            Alt = alt;
        }
    }
}
