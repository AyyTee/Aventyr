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
        public ElementFunc<string> TextFunc { get; }
        public ElementFunc<Font> FontFunc { get; }
        public ElementFunc<int?> MaxWidthFunc { get; }
        public ElementFunc<float> TextAlignmentFunc { get; }

        [DetectLoop]
        public string Text => TextFunc(ElementArgs);
        [DetectLoop]
        public Font Font => FontFunc(ElementArgs);
        [DetectLoop]
        public int? MaxWidth => MaxWidthFunc(ElementArgs);
        [DetectLoop]
        public float TextAlignment => TextAlignmentFunc(ElementArgs);

        Vector2 Size => (Vector2)(Font?.GetSize(Text, FontSettings) ?? new Vector2i());
        Font.Settings FontSettings => new Font.Settings(Color4.White, TextAlignment, maxWidth: MaxWidth);

        public TextBlock(
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null, 
            ElementFunc<Font> font = null, 
            ElementFunc<string> text = null,
            ElementFunc<int?> maxWidth = null,
            ElementFunc<float> textAlignment = null,
            ElementFunc<bool> hidden = null)
            : base(x, y, hidden: hidden)
        {
            WidthFunc = _ => Size.X;
            HeightFunc = _ => Size.Y;
            DebugEx.Assert(text != null);
            FontFunc = font ?? (_ => null);
            TextFunc = text ?? (_ => "");
            MaxWidthFunc = maxWidth ?? (_ => null);
            TextAlignmentFunc = textAlignment ?? (_ => 0);
        }

        public TextBlock(
            out TextBlock id, 
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null, 
            ElementFunc<Font> font = null, 
            ElementFunc<string> text = null,
            ElementFunc<int?> maxWidth = null,
            ElementFunc<float> textAlignment = null,
            ElementFunc<bool> hidden = null)
            : this(x, y, font, text, maxWidth, textAlignment, hidden)
        {
            id = this;
        }

        public override List<Model> GetModels(ModelArgs args)
        {
            var font = Font;
            return font != null ?
                new[] { font.GetModel(Text, FontSettings) }.ToList() :
                new List<Model>();
        }

        public override bool IsInside(Vector2 localPoint) => false;

        public IEnumerator<IElement> GetEnumerator() => new List<IElement>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
