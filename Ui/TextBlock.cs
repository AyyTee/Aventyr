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
        public ElementFunc<string> TextFunc { get; protected set; }
        public ElementFunc<Font> FontFunc { get; protected set; }

        public TextBlock(
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null, 
            ElementFunc<Font> font = null, 
            ElementFunc<string> text = null, 
            ElementFunc<bool> hidden = null)
            : base(x, y, hidden: hidden)
        {
            WidthFunc = _ => GetSize().X;
            HeightFunc = _ => GetSize().Y;
            DebugEx.Assert(text != null);
            FontFunc = font ?? (_ => null);
            TextFunc = text ?? (_ => "");
        }

        public TextBlock(out TextBlock id, ElementFunc<float> x = null, ElementFunc<float> y = null, ElementFunc<Font> font = null, ElementFunc<string> text = null, ElementFunc<bool> hidden = null)
            : this(x, y, font, text, hidden)
        {
            id = this;
        }

        public string GetText() => TextFunc(ElementArgs);
        public Font GetFont() => FontFunc(ElementArgs);

        Vector2 GetSize() => (Vector2)(GetFont()?.GetSize(GetText(), new Font.Settings(Color4.White)) ?? new Vector2i());

        public override List<Model> GetModels()
        {
            var font = GetFont();
            return font != null ?
                new[] { font.GetModel(GetText()) }.ToList() :
                new List<Model>();
        }

        public override bool IsInside(Vector2 localPoint) => false;

        public IEnumerator<IElement> GetEnumerator() => new List<IElement>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
