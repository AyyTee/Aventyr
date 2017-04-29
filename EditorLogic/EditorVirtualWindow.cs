using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using OpenTK;
using Game.Common;
using OpenTK.Input;
using System.Collections.Immutable;

namespace EditorLogic
{
    public class EditorVirtualWindow : IVirtualWindow
    {
        public Vector2i CanvasPosition => new Vector2i();
        public Vector2i CanvasSize { get; set; }

        public List<IRenderLayer> Layers { get; private set; } = new List<IRenderLayer>();

        public TextureAssets Textures { get; private set; }

        public IRenderer Renderer { get; private set; }

        public FontAssets Fonts { get; private set; }

        readonly GLControl _glControl;

        public float UpdatesPerSecond => 60;
        public float RendersPerSecond => 60;

        public bool HasFocus { get; private set; }

        public IImmutableSet<Key> KeyCurrent { get; private set; } = new HashSet<Key>().ToImmutableHashSet();
        public IImmutableSet<Key> KeyPrevious { get; private set; } = new HashSet<Key>().ToImmutableHashSet();

        public IImmutableSet<MouseButton> MouseCurrent { get; private set; } = new HashSet<MouseButton>().ToImmutableHashSet();
        public IImmutableSet<MouseButton> MousePrevious { get; private set; } = new HashSet<MouseButton>().ToImmutableHashSet();

        public float MouseWheel { get; private set; }
        public float MouseWheelPrevious { get; private set; }
        public Vector2 MousePosition { get; private set; }
        public Vector2 MousePositionPrevious { get; private set; }

        Vector2 _mousePos;

        public EditorVirtualWindow(GLControl glControl, IRenderer renderer, TextureAssets textures)
        {
            _glControl = glControl;
            CanvasSize = (Vector2i)_glControl.ClientSize;
            Renderer = renderer;
            Textures = textures;

            _glControl.MouseMove += (_, e) => { _mousePos = new Vector2(e.X, e.Y); };

            renderer.Windows.Add(this);
        }

        public void Update(ISet<Key> keyboardState, ISet<MouseButton> mouseState, bool hasFocus, float mouseWheel)
        {
            KeyPrevious = KeyCurrent;
            MousePrevious = MouseCurrent;

            KeyCurrent = keyboardState.ToImmutableHashSet();
            MouseCurrent = mouseState.ToImmutableHashSet();

            HasFocus = hasFocus;

            MousePositionPrevious = MousePosition;
            MousePosition = _mousePos;

            MouseWheelPrevious = MouseWheel;
            MouseWheel = mouseWheel;
        }
    }
}
