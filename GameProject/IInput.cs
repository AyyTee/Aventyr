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
        bool KeyDown(InputExt.KeyBoth input);
        bool KeyDown(Key input);
        bool KeyPress(InputExt.KeyBoth input);
        bool KeyPress(Key input);
        bool KeyRelease(InputExt.KeyBoth input);
        bool KeyRelease(Key input);
        bool MouseDown(MouseButton Input);
        bool MousePress(MouseButton Input);
        bool MouseRelease(MouseButton Input);
        float MouseWheelDelta();
        void Update(bool hasFocus);
    }
}