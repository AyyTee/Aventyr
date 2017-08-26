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
        public ElementFunc<string> TextFunc { get; }
        public ElementFunc<Font> FontFunc { get; }
        public ElementFunc<int?> MaxWidthFunc { get; }
        public ElementFunc<float> TextAlignmentFunc { get; }

        [DetectLoop]
        public string Text => InvokeFunc(TextFunc, nameof(Text));
        [DetectLoop]
        public Font Font => InvokeFunc(FontFunc, nameof(Font));
        [DetectLoop]
        public int? MaxWidth => InvokeFunc(MaxWidthFunc, nameof(MaxWidth));
        [DetectLoop]
        public float TextAlignment => InvokeFunc(TextAlignmentFunc, nameof(TextAlignment));

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
            FontFunc = font;
            TextFunc = text ?? (_ => "");
            MaxWidthFunc = maxWidth;
            TextAlignmentFunc = textAlignment;
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

        public static new Style DefaultStyle(IUiController controller)
        {
            var type = typeof(TextBlock);
            return new Style
            {
                new StyleElement(type, nameof(Font), _ => controller.Fonts),
                new StyleElement(type, nameof(MaxWidth), _ => null),
                new StyleElement(type, nameof(TextAlignment), _ => 0f)
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

        public static TextBlock DefaultStyle(IVirtualWindow window)
        {
            return new TextBlock(_ => 0, _ => 0, _ => window.Fonts.Inconsolata, _ => "", _ => null, _ => 0f, _ => false);
        }
    }
}
