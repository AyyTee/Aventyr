using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Models;
using System.Diagnostics;
using OpenTK;

namespace Game.Rendering
{
    public class TextEntity : IRenderable
    {
        public bool Visible { get; set; } = true;

        public bool DrawOverPortals { get; set; } = true;

        public bool IsPortalable { get; set; }

        public string Text { get; private set; }

        Model TextModel;

        public Transform2 Transform { get; set; } = new Transform2();

        bool Dirty = true;
        readonly FontRenderer _fontRenderer;

        public TextEntity(FontRenderer fontRenderer, Vector2 position, string text = "")
        {
            Debug.Assert(text != null);
            Text = text;
            Transform.Position = position;
            _fontRenderer = fontRenderer;
        }

        public void SetText(string text)
        {
            if (text != Text)
            {
                Text = text;
                Dirty = true;
            }
        }

        public List<Model> GetModels()
        {
            if (Dirty)
            {
                TextModel = _fontRenderer.GetModel(Text);
                Dirty = false;
            }
            return new List<Model>() { TextModel };
        }

        public Transform2 GetWorldTransform(bool ignorePortals = false) => Transform;

        public Transform2 GetWorldVelocity(bool ignorePortals = false) => Transform2.CreateVelocity();
    }
}
