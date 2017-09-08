using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using Game.Common;

namespace Game.Rendering
{
    public class GlStateManager
    {
        readonly Dictionary<EnableCap, Stack<bool>> _enableCapStacks = new Dictionary<EnableCap, Stack<bool>>();

        int _currentTexture = -1;
        RectangleF? _uvBounds = null;

        public GlStateManager()
        {
            // Only initialize the enable caps we'll be using.
            Add(EnableCap.CullFace, true);
            Add(EnableCap.DepthTest, true);
            Add(EnableCap.StencilTest, true);
            Add(EnableCap.Blend, true);
            Add(EnableCap.ScissorTest, true);
        }

        void Add(EnableCap cap, bool initialValue)
        {
            var stack = new Stack<bool>();
            stack.Push(initialValue);
            SetValue(cap, initialValue);
            _enableCapStacks.Add(cap, stack);
        }

        public StateChange Push(EnableCap enableCap, bool enable)
        {
            var previous = _enableCapStacks[enableCap].Peek();
            return new StateChange(this, enableCap, enable, previous);
        }

        public void Pop(StateChange stateChange)
        {
            stateChange.Dispose();
        }

        static void SetValue(EnableCap enableCap, bool enable)
        {
            if (enable)
            {
                GL.Enable(enableCap);
            }
            else
            {
                GL.Disable(enableCap);
            }
        }

        public void SetTexture(ITexture texture, Shader activeShader)
        {
            if (_currentTexture != texture.Id)
            {
                GL.Uniform1(activeShader.Uniforms[UberShader.MainTexture].Address, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, texture.Id);

                _currentTexture = texture.Id;
            }
        }

        public void SetUvBounds(RectangleF uvBounds, Shader activeShader)
        {
            var uvPosition = uvBounds.Position;
            var uvSize = uvBounds.Size;
            if (uvPosition != _uvBounds?.Position)
            {
                GL.Uniform2(activeShader.Uniforms[UberShader.UvPosition].Address, ref uvPosition);
            }
            if (uvSize != _uvBounds?.Size)
            {
                GL.Uniform2(activeShader.Uniforms[UberShader.UvSize].Address, ref uvSize);
            }

            _uvBounds = uvBounds;
        }

        public class StateChange : IDisposable
        {
            readonly GlStateManager _stateManager;
            public EnableCap EnableCap { get; private set; }
            public bool Current { get; private set; }
            bool _disposed;
            readonly bool _previous;
            readonly int _stackSize;

            public StateChange(GlStateManager stateManager, EnableCap enableCap, bool current, bool previous)
            {
                _stateManager = stateManager;
                EnableCap = enableCap;
                _previous = previous;
                Current = current;
                _stackSize = stateManager._enableCapStacks[enableCap].Count;
                stateManager._enableCapStacks[enableCap].Push(current);
                if (current != _previous)
                {
                    SetValue(EnableCap, current);
                }
            }

            public void Dispose()
            {
                DebugEx.Assert(!_disposed);
                if (!_disposed)
                {
                    _disposed = true;
                    _stateManager._enableCapStacks[EnableCap].Pop();
                    DebugEx.Assert(_stateManager._enableCapStacks[EnableCap].Count == _stackSize, $"{nameof(StateChange)} disposed out of order.");
                    if (Current != _previous)
                    {
                        SetValue(EnableCap, _previous);
                    }
                }
            }
        }
    }
}
