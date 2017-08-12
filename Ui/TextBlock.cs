using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Game.Common;
using Game.Models;
using Game.Rendering;
using OpenTK;
using System.Linq;
using OpenTK.Graphics;

namespace Ui
{
    public class TextBlock : Element, IElement
    {
        public string Text { get; set; }

        public Font Font { get; set; }

        Vector2 GetSize => (Vector2)(Font?.Size(Text, new Font.Settings(Color4.White)) ?? new Vector2i());

        public TextBlock(Func<ElementArgs, Transform2> transform = null, Font font = null, string text = "")
            : base(transform)
        {
            WidthFunc = _ => GetSize.X;
            HeightFunc = _ => GetSize.Y;
            DebugEx.Assert(text != null);
            Font = font;
            Text = text;
        }

        public TextBlock(out TextBlock id, Func<ElementArgs, Transform2> transform = null, Font font = null, string text = "")
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
