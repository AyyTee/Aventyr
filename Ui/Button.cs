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

        public Button(Func<ElementArgs, Transform2> transform = null, Func<ElementArgs, float> width = null, Func<ElementArgs, float> height = null, Action onClick = null)
            : base(transform, width, height)
        {
            if (onClick != null)
            {
                OnClick += () => { onClick(); };
            }
        }

        public Button(out Button id, Func<ElementArgs, Transform2> transform = null, Func<ElementArgs, float> width = null, Func<ElementArgs, float> height = null, Action onClick = null)
            : this(transform, width, height, onClick)
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
