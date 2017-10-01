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
    public class TextBlock : Element
    {
        internal ElementFunc<string> _text;
        internal ElementFunc<Font> _font;
        internal ElementFunc<int?> _maxWidth;
        internal ElementFunc<float> _textAlignment;

        public string Text => GetValue(_text);
        public Font Font => GetValue(_font);
        public int? MaxWidth => GetValue(_maxWidth);
        public float TextAlignment => GetValue(_textAlignment);

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
            : base(
                x, 
                y, 
                args => ((TextBlock)args.Self).Size.X, 
                args => ((TextBlock)args.Self).Size.Y, 
                hidden)
        {
            DebugEx.Assert(text != null);
            _font = font;
            _text = text;
            _maxWidth = maxWidth;
            _textAlignment = textAlignment;
        }

        public static new Style DefaultStyle(IUiController controller)
        {
            var type = typeof(TextBlock);
            return new Style
            {
                new StyleElement(type, nameof(Font), _ => controller.Fonts.DefaultFont),
                new StyleElement(type, nameof(MaxWidth), _ => null),
                new StyleElement(type, nameof(TextAlignment), _ => 0f),
                new StyleElement(type, nameof(Text), _ => "")
            };
        }

        public override List<Model> GetModels(ModelArgs args)
        {
            return new[] { Font.GetModel(Text, FontSettings) }.ToList();
        }

        public override bool IsInside(Vector2 localPoint) => false;
    }
}
