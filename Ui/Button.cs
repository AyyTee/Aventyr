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
        public Action OnClick { get; }

        public Button(ElementFunc<float> x = null, ElementFunc<float> y = null, ElementFunc<float> width = null, ElementFunc<float> height = null, Action onClick = null)
            : base(x, y , width, height)
        {
            if (onClick != null)
            {
                OnClick += () => { onClick(); };
            }
        }

        public Button(out Button id, ElementFunc<float> x = null, ElementFunc<float> y = null, ElementFunc<float> width = null, ElementFunc<float> height = null, Action onClick = null)
            : this(x, y, width, height, onClick)
        {
            id = this;
        }

        public override List<Model> GetModels()
        {
            return new[]
            {
                ModelFactory.CreatePlane(this.GetSize(), Color4.Black),
            }.ToList();
        }

        public override bool IsInside(Vector2 localPoint)
        {
            return MathEx.PointInRectangle(new Vector2(), this.GetSize(), localPoint);
        }
    }
}
