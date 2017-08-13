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

        protected ElementFunc<float> XFunc { get; set; }
        protected ElementFunc<float> YFunc { get; set; }
        protected ElementFunc<float> WidthFunc { get; set; }
        protected ElementFunc<float> HeightFunc { get; set; }
        protected ElementFunc<bool> HiddenFunc { get; set; }

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

        public float GetX() => XFunc(ElementArgs);
        public float GetY() => YFunc(ElementArgs);
        public float GetWidth() => WidthFunc(ElementArgs);
        public float GetHeight() => HeightFunc(ElementArgs);
        public bool GetHidden() => HiddenFunc(ElementArgs);

        public virtual bool IsInside(Vector2 localPoint) => false;
        public virtual List<Model> GetModels() => new List<Model>();//Draw.Rectangle(new Vector2(), new Vector2(GetWidth(), GetHeight()), new Color4(0f, 0f, 0f, 0.3f)).GetModels();
    }
}
