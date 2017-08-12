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

        public ElementFunc<Transform2> TransformFunc { get; protected set; }
        public ElementFunc<float> WidthFunc { get; protected set; }
        public ElementFunc<float> HeightFunc { get; protected set; }
        public ElementFunc<bool> HiddenFunc { get; protected set; }

        public Element(
            ElementFunc<Transform2> transform = null,
            ElementFunc<float> width = null,
            ElementFunc<float> height = null,
            ElementFunc<bool> hidden = null)
        {
            ElementArgs = new ElementArgs(null, (IElement)this);

            TransformFunc = transform ?? (_ => new Transform2());
            WidthFunc = width ?? (args => args.Parent.GetWidth());
            HeightFunc = height ?? (args => args.Parent.GetHeight());
            HiddenFunc = hidden ?? (_ => false);
        }

        public Transform2 GetTransform() => TransformFunc(ElementArgs);
        public float GetWidth() => WidthFunc(ElementArgs);
        public float GetHeight() => HeightFunc(ElementArgs);
        public bool GetHidden() => HiddenFunc(ElementArgs);

        public virtual bool IsInside(Vector2 localPoint) => false;
        public virtual List<Model> GetModels() => new List<Model>();//Draw.Rectangle(new Vector2(), new Vector2(GetWidth(), GetHeight()), new Color4(0f, 0f, 0f, 0.3f)).GetModels();
    }
}
