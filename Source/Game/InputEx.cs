using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class InputEx
    {
        readonly static Key[] _keyEnumeration = Enum.GetValues(typeof(Key)).Cast<Key>().ToArray();
        readonly static MouseButton[] _mouseEnumeration = Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>().ToArray();

        public static HashSet<Key> KeysDown(this KeyboardState keyboardState)
        {
            var keyState = new HashSet<Key>();
            foreach (var key in _keyEnumeration)
            {
                if (keyboardState.IsKeyDown(key))
                {
                    keyState.Add(key);
                }
            }
            return keyState;
        }

        public static HashSet<MouseButton> ButtonsDown(this MouseState mouseState)
        {
            var mouseButtons = new HashSet<MouseButton>();
            foreach (var mouse in _mouseEnumeration)
            {
                if (mouseState.IsButtonDown(mouse))
                {
                    mouseButtons.Add(mouse);
                }
            }
            return mouseButtons;
        }
    }
}
