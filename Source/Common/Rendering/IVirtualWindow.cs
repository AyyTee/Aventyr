using Game.Common;
using OpenTK;
using OpenTK.Input;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Game.Rendering
{
    public interface IVirtualWindow
    {
        Vector2i CanvasPosition { get; }
        Vector2i CanvasSize { get; }
        float DpiScale { get; }
        Resources Textures { get; }
        List<IRenderLayer> Layers { get; }
        Resources Fonts { get; }
        float UpdatesPerSecond { get; }
        float RendersPerSecond { get; }

        bool HasFocus { get; }
        string KeyString { get; }
        IImmutableSet<Key> KeyCurrent { get; }
        IImmutableSet<Key> KeyPrevious { get; }
        IImmutableSet<MouseButton> MouseCurrent { get; }
        IImmutableSet<MouseButton> MousePrevious { get; }
        float MouseWheel { get; }
        float MouseWheelPrevious { get; }
        Vector2 MousePosition { get; }
        Vector2 MousePositionPrevious { get; }

        void Exit();
        event ExitHandler OnExit;
    }

    public delegate void ExitHandler();

    public enum KeyBoth { Control, Shift, Alt }

    public static class IVirtualWindowEx
    {
        public static bool MouseInside(this IVirtualWindow window)
        {
            return window.MousePosition.X >= 0 &&
                window.MousePosition.X < window.CanvasSize.X &&
                window.MousePosition.Y >= 0 &&
                window.MousePosition.Y < window.CanvasSize.Y;
        }

        public static float MouseWheelDelta(this IVirtualWindow window)
        {
            return window.MouseWheel - window.MouseWheelPrevious;
        }

        public static bool ButtonDown(this IVirtualWindow window, Key input)
        {
            return window.KeyCurrent.Contains(input) && window.HasFocus;
        }

        public static bool ButtonDown(this IVirtualWindow window, KeyBoth input)
        {
            switch (input)
            {
                case KeyBoth.Control:
                    return window.ButtonDown(Key.ControlLeft) || window.ButtonDown(Key.ControlRight);
                case KeyBoth.Shift:
                    return window.ButtonDown(Key.ShiftLeft) || window.ButtonDown(Key.ShiftRight);
                case KeyBoth.Alt:
                    return window.ButtonDown(Key.AltLeft) || window.ButtonDown(Key.AltRight);
                default:
                    return false;
            }
        }

        static bool ModifierKeysDown(this IVirtualWindow window, Hotkey hotkey)
        {
            if (hotkey.Control && !window.ButtonDown(KeyBoth.Control))
            {
                return false;
            }
            if (hotkey.Shift && !window.ButtonDown(KeyBoth.Shift))
            {
                return false;
            }
            if (hotkey.Alt && !window.ButtonDown(KeyBoth.Alt))
            {
                return false;
            }
            return true;
        }

        public static bool HotkeyDown(this IVirtualWindow window, Hotkey hotkey)
        {
            return hotkey == null ?
                false :
                window.ButtonDown(hotkey.Key) && window.ModifierKeysDown(hotkey);
        }

        public static bool ButtonDown(this IVirtualWindow window, MouseButton input)
        {
            return window.MouseCurrent.Contains(input) && window.HasFocus;
        }

        public static bool ButtonPress(this IVirtualWindow window, Key input)
        {
            return window.KeyCurrent.Contains(input) &&
                !window.KeyPrevious.Contains(input) &&
                window.HasFocus;
        }

        public static bool ButtonPress(this IVirtualWindow window, KeyBoth input)
        {
            switch (input)
            {
                case KeyBoth.Control:
                    return window.ButtonPress(Key.ControlLeft) || window.ButtonPress(Key.ControlRight);
                case KeyBoth.Shift:
                    return window.ButtonPress(Key.ShiftLeft) || window.ButtonPress(Key.ShiftRight);
                case KeyBoth.Alt:
                    return window.ButtonPress(Key.AltLeft) || window.ButtonPress(Key.AltRight);
                default:
                    return false;
            }
        }

        public static bool HotkeyPress(this IVirtualWindow window, Hotkey hotkey)
        {
            return hotkey == null ? 
                false :
                window.ButtonPress(hotkey.Key) && window.ModifierKeysDown(hotkey);
        }

        public static bool ButtonPress(this IVirtualWindow window, MouseButton input)
        {
            return window.MouseCurrent.Contains(input) &&
                !window.MousePrevious.Contains(input) &&
                window.HasFocus;
        }

        public static bool ButtonRelease(this IVirtualWindow window, Key input)
        {
            return !window.KeyCurrent.Contains(input) &&
                window.KeyPrevious.Contains(input) &&
                window.HasFocus;
        }

        public static bool ButtonRelease(this IVirtualWindow window, KeyBoth input)
        {
            switch (input)
            {
                case KeyBoth.Control:
                    return window.ButtonRelease(Key.ControlLeft) || window.ButtonRelease(Key.ControlRight);
                case KeyBoth.Shift:
                    return window.ButtonRelease(Key.ShiftLeft) || window.ButtonRelease(Key.ShiftRight);
                case KeyBoth.Alt:
                    return window.ButtonRelease(Key.AltLeft) || window.ButtonRelease(Key.AltRight);
                default:
                    return false;
            }
        }

        public static bool HotkeyRelease(this IVirtualWindow window, Hotkey hotkey)
        {
            return hotkey == null ?
                false :
                window.ButtonRelease(hotkey.Key) && window.ModifierKeysDown(hotkey);
        }

        public static bool ButtonRelease(this IVirtualWindow window, MouseButton input)
        {
            return !window.MouseCurrent.Contains(input) &&
                window.MousePrevious.Contains(input) &&
                window.HasFocus;
        }

        public static Vector2 MouseWorldPos(this IVirtualWindow window, ICamera2 camera)
        {
            return camera.ScreenToWorld(window.MousePosition, window.CanvasSize);
        }
    }
}