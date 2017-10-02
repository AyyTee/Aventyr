using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Game;
using Game.Common;
using Game.Models;
using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;
using Ui.Args;

namespace Ui.Elements
{
    public class Button : NodeElement, IClickable
    {
        public ElementAction<ClickArgs> OnClick { get; }
        public ElementAction<HoverArgs> OnHover { get; }

        internal ElementFunc<Color4> _color;
        public Color4 Color => GetValue(_color);
        internal ElementFunc<Color4> _disabledColor;
        public Color4 DisabledColor => GetValue(_disabledColor);
        internal ElementFunc<Color4> _hoverColor;
        public Color4 HoverColor => GetValue(_hoverColor);
        internal ElementFunc<Color4> _borderColor;
        public Color4 BorderColor => GetValue(_borderColor);

        internal ElementFunc<bool> _enabled;
        public bool Enabled => GetValue(_enabled);

        public Button(
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null, 
            ElementFunc<float> width = null, 
            ElementFunc<float> height = null,
            ElementAction<ClickArgs> onClick = null,
            ElementAction<HoverArgs> onHover = null,
            ElementFunc<bool> enabled = null,
            ElementFunc<bool> hidden = null,
            ElementFunc<Color4> color = null,
            ElementFunc<Color4> disabledColor = null,
            ElementFunc<Color4> hoverColor = null,
            ElementFunc<Color4> borderColor = null,
            Style style = null)
            : base(x, y , width, height, hidden, style)
        {
            OnClick = onClick ?? (_ => { });
            OnHover = onHover ?? (_ => { });

            _color = color;
            _disabledColor = disabledColor;
            _hoverColor = hoverColor;
            _borderColor = borderColor;

            _enabled = enabled;
        }

        public override Style RootStyle()
        {
            var defaultStyle = base.RootStyle();

            return defaultStyle.With(new Style
            {
                new StyleElement<Button, bool>(nameof(Enabled), _ => true),
                new StyleElement<Button, Color4>(nameof(Color), _ => new Color4(0.4f, 0.4f, 0.8f, 1)),
                new StyleElement<Button, Color4>(nameof(DisabledColor), _ => new Color4(0.3f, 0.3f, 0.6f, 0.8f)),
                new StyleElement<Button, Color4>(nameof(HoverColor), arg => ((Button)arg.Self).Color.AddRgb(0.1f, 0.1f, 0.1f)),
                new StyleElement<Button, Color4>(nameof(BorderColor), _ => new Color4(1, 1, 1, 0.5f)),
            });
        }

        public override List<Model> GetModels(ModelArgs args)
        {
            var buttonColor = !Enabled ?
                DisabledColor:
                args.Controller.Hovered == this ?
                    HoverColor :
                    Color;

            return new[]
            {
                ModelFactory.CreatePlane(this.GetSize(), buttonColor),
                ModelFactory.CreateRectangleOutline(new Vector2(), this.GetSize(), BorderColor, 2),
            }.ToList();
        }

        public override bool IsInside(Vector2 localPoint)
        {
            return MathEx.PointInRectangle(new Vector2(), this.GetSize(), localPoint);
        }
    }
}
