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

namespace Ui.Elements
{
    public class TextBlock : Element, IElement
    {
        internal ElementFunc<string> _text;
        internal ElementFunc<Font> _font;
        internal ElementFunc<int?> _maxWidth;
        internal ElementFunc<float> _textAlignment;

        [DetectLoop]
        public string Text => InvokeFunc(_text);
        [DetectLoop]
        public Font Font => InvokeFunc(_font);
        [DetectLoop]
        public int? MaxWidth => InvokeFunc(_maxWidth);
        [DetectLoop]
        public float TextAlignment => InvokeFunc(_textAlignment);

        Vector2 Size => (Vector2)(Font?.GetSize(Text, FontSettings) ?? new Vector2i());
        Font.Settings FontSettings => new Font.Settings(Color4.White, TextAlignment, maxWidth: MaxWidth);

        public TextBlock(
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<string> text = null,
            ElementFunc<Font> font = null,
            ElementFunc<int?> maxWidth = null,
            ElementFunc<float> textAlignment = null,
            ElementFunc<bool> hidden = null)
            : base(x, y, hidden: hidden)
        {
            _width = _ => Size.X;
            _height = _ => Size.Y;
            DebugEx.Assert(text != null);
            _font = font;
            _text = text;
            _maxWidth = maxWidth;
            _textAlignment = textAlignment;
        }

        public TextBlock(
            out TextBlock id,
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<string> text = null,
            ElementFunc<Font> font = null,
            ElementFunc<int?> maxWidth = null,
            ElementFunc<float> textAlignment = null,
            ElementFunc<bool> hidden = null)
            : this(x, y, text, font, maxWidth, textAlignment, hidden)
        {
            id = this;
        }

        public static new Style DefaultStyle(IUiController controller)
        {
            var type = typeof(TextBlock);
            return new Style
            {
                new StyleElement(type, nameof(Font), _ => controller.Fonts.Inconsolata),
                new StyleElement(type, nameof(MaxWidth), _ => null),
                new StyleElement(type, nameof(TextAlignment), _ => 0f),
                new StyleElement(type, nameof(Text), _ => "")
            };
        }

        public override List<Model> GetModels(ModelArgs args)
        {
            var font = Font;
            return font != null ?
                new[] { font.GetModel(Text, FontSettings) }.ToList() :
                new List<Model>();
        }

        public override bool IsInside(Vector2 localPoint) => false;
    }
}
