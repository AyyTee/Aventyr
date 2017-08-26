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

namespace Ui
{
    public class Button : NodeElement, IElement
    {
        internal OnClickHandler OnClick { get; }
        internal OnHoverHandler OnHover { get; }
        internal ElementFunc<bool> EnabledFunc { get; }
        [DetectLoop]
        public bool Enabled => EnabledFunc(ElementArgs);

        public Button(
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null, 
            ElementFunc<float> width = null, 
            ElementFunc<float> height = null,
            OnClickHandler onClick = null, 
            OnHoverHandler onHover = null,
            ElementFunc<bool> enabled = null,
            ElementFunc<bool> hidden = null,
            ImmutableDictionary<(Type, string), ElementFunc<object>> style = null)
            : base(x, y , width, height, hidden, style)
        {
            OnClick = onClick ?? (_ => { });
            OnHover = onHover ?? (_ => { });
            EnabledFunc = enabled ?? (_ => true);
        }

        public Button(
            out Button id, 
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null, 
            ElementFunc<float> width = null, 
            ElementFunc<float> height = null,
            OnClickHandler onClick = null,
            OnHoverHandler onHover = null,
            ElementFunc<bool> enabled = null,
            ElementFunc<bool> hidden = null,
            ImmutableDictionary<(Type, string), ElementFunc<object>> style = null)
            : this(x, y, width, height, onClick, onHover, enabled, hidden, style)
        {
            id = this;
        }

        public override List<Model> GetModels(ModelArgs args)
        {
            return new[]
            {
                ModelFactory.CreatePlane(this.GetSize(), Enabled ? Color4.Black : new Color4(0, 0, 0, 0.5f)),
            }.ToList();
        }

        public override bool IsInside(Vector2 localPoint)
        {
            return MathEx.PointInRectangle(new Vector2(), this.GetSize(), localPoint);
        }
    }
}
