using Game.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using System.Collections.Immutable;

namespace Game.Rendering
{
    public class VirtualWindow : IVirtualWindow
    {
        readonly ResourceController _resourceController;
        public Vector2i CanvasSize { get; set; }
        public Vector2i CanvasPosition { get; set; }
        public float DpiScale => _resourceController.DpiScale;
        public TextureAssets Textures => _resourceController.Textures;
        public List<IRenderLayer> Layers { get; private set; } = new List<IRenderLayer>();
        public FontAssets Fonts => _resourceController.Fonts;
        public float UpdatesPerSecond => 60;
        public float RendersPerSecond => 60;

        public IImmutableSet<Key> KeyCurrent { get; private set; } = new HashSet<Key>().ToImmutableHashSet();
        public IImmutableSet<Key> KeyPrevious { get; private set; } = new HashSet<Key>().ToImmutableHashSet();

        public IImmutableSet<MouseButton> MouseCurrent { get; private set; } = new HashSet<MouseButton>().ToImmutableHashSet();
        public IImmutableSet<MouseButton> MousePrevious { get; private set; } = new HashSet<MouseButton>().ToImmutableHashSet();
        public float MouseWheel { get; private set; }
        public float MouseWheelPrevious { get; private set; }
        public Vector2 MousePosition { get; private set; }
        public Vector2 MousePositionPrevious { get; private set; }

        public bool HasFocus { get; private set; }

        public event ExitHandler OnExit;

        public VirtualWindow(ResourceController resourceController)
        {
            _resourceController = resourceController;
            _resourceController.Renderer.Windows.Add(this);
        }

        public void Update(ISet<Key> keyboardState, ISet<MouseButton> mouseState, Vector2 mousePosition, bool hasFocus, float mouseWheel)
        {
            KeyPrevious = KeyCurrent;
            MousePrevious = MouseCurrent;

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
