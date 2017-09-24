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
        public Func<Vector2i> CanvasSizeFunc { get; }
        public Vector2i CanvasSize => CanvasSizeFunc();
        public Func<Vector2i> CanvasPositionFunc { get; }
        public Vector2i CanvasPosition => CanvasPositionFunc();

        public float DpiScale { get; set; } = 1;

        public List<IRenderLayer> Layers { get; set; } = new List<IRenderLayer>();
        public Resources Resources { get; }

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

        public FakeVirtualWindow(Resources resources, Func<Vector2i> canvasSize, Func<Vector2i> canvasPosition = null)
        {
            Resources = resources;

            CanvasSizeFunc = canvasSize;
            CanvasPositionFunc = canvasPosition ?? (() => new Vector2i());
        }

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
