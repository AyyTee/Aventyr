using OpenTK;
using OpenTK.Input;

namespace Game
{
    public interface IInput
    {
        bool Focus { get; }
        bool MouseInside { get; }
        Vector2 MousePos { get; }
        Vector2 MousePosPrev { get; }

        Vector2 GetMouseWorldPos(ICamera2 camera, Vector2 canvasSize);
        bool KeyDown(KeyBoth input);
        bool KeyDown(Key input);
        bool KeyPress(KeyBoth input);
        bool KeyPress(Key input);
        bool KeyRelease(KeyBoth input);
        bool KeyRelease(Key input);
        bool MouseDown(MouseButton Input);
        bool MousePress(MouseButton Input);
        bool MouseRelease(MouseButton Input);
        float MouseWheelDelta();
        void Update(bool hasFocus);
    }
}

namespace OpenTK.Input
{
    public enum KeyBoth { Control, Shift, Alt }
}