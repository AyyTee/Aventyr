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
        public Vector2 Alignment { get; private set; }

        Model TextModel;
        [DataMember]
        public Transform2 Transform { get; set; } = new Transform2();

        public Transform2 WorldTransform => Transform;
        public Transform2 WorldVelocity => Transform2.CreateVelocity();

        bool Dirty = true;
        readonly Font _fontRenderer;

        public TextEntity(Font fontRenderer, Vector2 position, string text = "") : this(fontRenderer, position, new Vector2(), text)
        {
        }

        public TextEntity(Font fontRenderer, Vector2 position, Vector2 alignment, string text = "")
        {
            Debug.Assert(text != null);
            Text = text;
            Transform.Position = position;
            _fontRenderer = fontRenderer;
            Alignment = alignment;
        }

        public void SetText(string text)
        {
            if (text != Text)
            {
                Text = text;
                Dirty = true;
            }
        }

        public void SetAlignment(Vector2 alignment)
        {
            if (alignment != Alignment)
            {
                Alignment = alignment;
                Dirty = true;
            }
        }

        public List<Model> GetModels()
        {
            if (Dirty)
            {
                TextModel = _fontRenderer?.GetModel(Text, Alignment);
                Dirty = false;
            }
            return new List<Model>() { TextModel };
        }
    }
}
