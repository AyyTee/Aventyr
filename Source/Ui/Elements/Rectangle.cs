using Game.Common;
using Game.Models;
using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui.Elements
{
    public class Rectangle : NodeElement
    {
        internal ElementFunc<Color4> _color;
        public Color4 Color => GetValue(_color);

        public Rectangle(
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<float> width = null,
            ElementFunc<float> height = null,
            ElementFunc<Color4> color = null,
            ElementFunc<bool> hidden = null,
            Style style = null)
            : base(x, y, width, height, hidden, style)
        {
            _color = color;
        }

        public static new Style DefaultStyle(IUiController controller)
        {
            var type = typeof(Rectangle);
            return new Style
            {
                new StyleElement(type, nameof(Color), _ => Color4.White),
            };
        }

        public override List<Model> GetModels(ModelArgs args)
        {
            return Draw.Rectangle(new Vector2(), this.GetSize(), Color).GetModels();
        }

        public override bool IsInside(Vector2 localPoint)
        {
            return MathEx.PointInRectangle(new Vector2(), this.GetSize(), localPoint) && Color.A > 0;
        }
    }
}
