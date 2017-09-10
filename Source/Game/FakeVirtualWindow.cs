using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.Common;
using OpenTK.Input;
using OpenTK;

namespace Game
{
    public class FakeVirtualWindow : IVirtualWindow
    {
        public Vector2i CanvasPosition => new Vector2i();
        public Vector2i CanvasSize => new Vector2i(1000, 800);
        public float DpiScale { get; set; } = 1;

        public List<IRenderLayer> Layers { get; private set; } = new List<IRenderLayer>();
        public Resources Resources { get; } = new Resources();

        public float UpdatesPerSecond => 60;
        public float RendersPerSecond => 60;

        public bool HasFocus { get; private set; } = true;

        public float MouseWheel { get; set; }
        public float MouseWheelPrevious { get; private set; }

        public Vector2 MousePosition { get; set; }
        public Vector2 MousePositionPrevious { get; private set; }

        public string KeyString { get; set; } = "";
        public IImmutableSet<Key> KeyCurrent { get; set; } = new HashSet<Key>().ToImmutableHashSet();
        public IImmutableSet<Key> KeyPrevious { get; private set; } = new HashSet<Key>().ToImmutableHashSet();

        public IImmutableSet<MouseButton> MouseCurrent { get; set; } = new HashSet<MouseButton>().ToImmutableHashSet();
        public IImmutableSet<MouseButton> MousePrevious { get; private set; } = new HashSet<MouseButton>().ToImmutableHashSet();

        public event ExitHandler OnExit;

        public void Update(string keyString, ISet<Key> keyboardState, ISet<MouseButton> mouseState, Vector2 mousePosition = new Vector2(), bool hasFocus = true, float mouseWheel = 0)
        {
            KeyPrevious = KeyCurrent;
            MousePrevious = MouseCurrent;

            KeyString = keyString;
            KeyCurrent = keyboardState.ToImmutableHashSet();
            MouseCurrent = mouseState.ToImmutableHashSet();

            HasFocus = hasFocus;

            MousePositionPrevious = MousePosition;
            MousePosition = mousePosition;

            MouseWheelPrevious = MouseWheel;
            MouseWheel = mouseWheel;
        }

        public void Exit() => OnExit?.Invoke();
    }
}
