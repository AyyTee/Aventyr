using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Models;
using System.Diagnostics;
using OpenTK;
using System.Runtime.Serialization;
using OpenTK.Graphics;

namespace Game.Rendering
{
    [DataContract]
    public class TextEntity : IRenderable
    {
        [DataMember]
        public bool Visible { get; set; } = true;
        [DataMember]
        public bool DrawOverPortals { get; set; } = true;
        [DataMember]
        public bool IsPortalable { get; set; }
        [DataMember]
        public string Text { get; private set; }
        [DataMember]
        public Color4 Color { get; private set; }
        [DataMember]
        public float AlignX { get; private set; }
        [DataMember]
        public int LineSpacing { get; private set; }
        [DataMember]
        public List<ClipPath> ClipPaths { get; private set; } = new List<ClipPath>();

        Model TextModel;
        [DataMember]
        public Transform2 WorldTransform { get; set; } = new Transform2();
        public Transform2 WorldVelocity => Transform2.CreateVelocity();

        bool Dirty = true;
        readonly Font _fontRenderer;

        public TextEntity(Font fontRenderer, Vector2 position, string text, float alignment = 0, int lineSpacing = 0)
            : this(fontRenderer, position, text, Color4.White, alignment, lineSpacing)
        {
        }

        public TextEntity(Font fontRenderer, Vector2 position, string text, Color4 color, float alignment = 0, int lineSpacing = 0)
        {
            DebugEx.Assert(text != null);
            Text = text;
            Color = color;
            WorldTransform = new Transform2(position);
            _fontRenderer = fontRenderer;
            AlignX = alignment;
            LineSpacing = lineSpacing;
        }

        public void SetText(string text)
        {
            if (text != Text)
            {
                Text = text;
                Dirty = true;
            }
        }

        public void SetAlignment(float alignX)
        {
            if (alignX != AlignX)
            {
                AlignX = alignX;
                Dirty = true;
            }
        }

        public List<Model> GetModels()
        {
            if (Dirty)
            {
                TextModel = _fontRenderer?.GetModel(Text, Color, AlignX, LineSpacing);
                Dirty = false;
            }
            return TextModel == null ?
                new List<Model>() :
                new List<Model> { TextModel };
        }
    }
}
