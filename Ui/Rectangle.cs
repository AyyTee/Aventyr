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

namespace Ui
{
    public class Rectangle : NodeElement, IElement
    {
        public ElementFunc<Color4> ColorFunc { get; }
        [DetectLoop]
        public Color4 Color => ColorFunc(ElementArgs);

        public Rectangle(
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<float> width = null,
            ElementFunc<float> height = null,
            ElementFunc<Color4> color = null,
            ElementFunc<bool> hidden = null,
            ImmutableDictionary<(Type, string), ElementFunc<object>> style = null)
            : base(x, y, width, height, hidden, style)
        {
            ColorFunc = color ?? (_ => Color4.White);
        }

        public Rectangle(
            out Rectangle id,
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<float> width = null,
            ElementFunc<float> height = null,
            ElementFunc<Color4> color = null,
            ElementFunc<bool> hidden = null,
            ImmutableDictionary<(Type, string), ElementFunc<object>> style = null)
            : this(x, y, width, height, color, hidden, style)
        {
            id = this;
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
