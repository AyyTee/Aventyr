using Game.Common;
using Game.Models;
using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui
{
    public abstract class Element
    {
        public ElementArgs ElementArgs { get; set; }

        internal ElementFunc<float> XFunc { get; set; }
        internal ElementFunc<float> YFunc { get; set; }
        internal ElementFunc<float> WidthFunc { get; set; }
        internal ElementFunc<float> HeightFunc { get; set; }
        internal ElementFunc<bool> HiddenFunc { get; set; }

        public Element(
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<float> width = null,
            ElementFunc<float> height = null,
            ElementFunc<bool> hidden = null)
        {
            ElementArgs = new ElementArgs(null, (IElement)this);

            XFunc = x ?? (_ => 0);
            YFunc = y ?? (_ => 0);
            WidthFunc = width ?? (args => args.Parent.GetWidth());
            HeightFunc = height ?? (args => args.Parent.GetHeight());
            HiddenFunc = hidden ?? (_ => false);
        }

        [DetectLoopAspect]
        public float GetX() => XFunc(ElementArgs);
        [DetectLoopAspect]
        public float GetY() => YFunc(ElementArgs);
        [DetectLoopAspect]
        public float GetWidth() => WidthFunc(ElementArgs);
        [DetectLoopAspect]
        public float GetHeight() => HeightFunc(ElementArgs);
        [DetectLoopAspect]
        public bool GetHidden() => HiddenFunc(ElementArgs);

        public virtual bool IsInside(Vector2 localPoint) => false;
        public virtual List<Model> GetModels() => new List<Model>();//Draw.Rectangle(new Vector2(), new Vector2(GetWidth(), GetHeight()), new Color4(0f, 0f, 0f, 0.3f)).GetModels();
    }
}
