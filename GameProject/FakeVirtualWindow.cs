﻿using Game.Rendering;
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
        public Vector2i CanvasSize => new Vector2i(300, 200);

        public List<IRenderLayer> Layers { get; private set; } = new List<IRenderLayer>();
        public TextureAssets Textures => null;
        public FontAssets Fonts => null;

        public float UpdatesPerSecond => 60;
        public float RendersPerSecond => 60;

        public bool HasFocus { get; private set; } = true;

        public float MouseWheel { get; set; }
        public float MouseWheelDelta { get; private set; }

        public Vector2 MousePosition { get; set; }
        public Vector2 MousePositionPrevious { get; private set; }

        public IImmutableSet<Key> KeyCurrent { get; set; } = new HashSet<Key>().ToImmutableHashSet();
        public IImmutableSet<Key> KeyPrevious { get; private set; } = new HashSet<Key>().ToImmutableHashSet();

        public IImmutableSet<MouseButton> MouseCurrent { get; set; } = new HashSet<MouseButton>().ToImmutableHashSet();
        public IImmutableSet<MouseButton> MousePrevious { get; private set; } = new HashSet<MouseButton>().ToImmutableHashSet();

        public void Update(ISet<Key> keyboardState, ISet<MouseButton> mouseState, Vector2 mousePosition, bool hasFocus = true, float mouseWheelDelta = 0)
        {
            KeyPrevious = KeyCurrent;
            MousePrevious = MouseCurrent;

            KeyCurrent = keyboardState.ToImmutableHashSet();
            MouseCurrent = mouseState.ToImmutableHashSet();

            HasFocus = hasFocus;

            MousePositionPrevious = MousePosition;
            MousePosition = mousePosition;

            MouseWheelDelta = mouseWheelDelta;
            MouseWheel += mouseWheelDelta;
        }
    }
}
