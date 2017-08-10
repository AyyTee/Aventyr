using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Game.Common;
using Game.Models;
using Game.Rendering;
using OpenTK;
using System.Linq;

namespace Ui
{
    public class TextBlock : IElement
    {
        public Transform2 Transform { get; set; } = new Transform2();

        public string Text { get; set; }

        public Font Font { get; set; }

        public bool Hidden { get; set; }

        public Vector2 Size { get; set; }

        public TextBlock(Transform2 transform, Font font, string text)
        {
            DebugEx.Assert(text != null);
            Transform = transform;
            Font = font;
            Text = text;
        }

        public TextBlock(out TextBlock id, Transform2 transform, Font font, string text)
            : this(transform, font, text)
        {
            id = this;
        }

        public List<Model> GetModels()
        {
            return Font != null ?
                new[] { Font.GetModel(Text) }.ToList() :
                new List<Model>();
        }

        public bool IsInside(Vector2 localPoint) => false;

        public IEnumerator<IElement> GetEnumerator() => new List<IElement>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
