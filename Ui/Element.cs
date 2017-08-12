using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui
{
    public class Element
    {
        protected static NullReferenceException NullArgException => new NullReferenceException();

        public ElementArgs ElementArgs { get; set; }

        public Func<ElementArgs, Transform2> TransformFunc { get; protected set; }
        public Func<ElementArgs, float> WidthFunc { get; protected set; }
        public Func<ElementArgs, float> HeightFunc { get; protected set; }
        public Func<ElementArgs, bool> HiddenFunc { get; protected set; }

        public Element(
            Func<ElementArgs, Transform2> transform = null,
            Func<ElementArgs, float> width = null,
            Func<ElementArgs, float> height = null,
            Func<ElementArgs, bool> hidden = null)
        {
            TransformFunc = transform ?? (_ => new Transform2());
            WidthFunc = width ?? (args => args.Parent.GetWidth());
            HeightFunc = height ?? (args => args.Parent.GetHeight());
            HiddenFunc = hidden ?? (_ => false);
        }

        public Transform2 GetTransform()
        {
            return TransformFunc(ElementArgs ?? throw NullArgException);
        }

        public float GetWidth()
        {
            return WidthFunc(ElementArgs ?? throw NullArgException);
        }

        public float GetHeight()
        {
            return HeightFunc(ElementArgs ?? throw NullArgException);
        }

        public bool GetHidden()
        {
            return HiddenFunc(ElementArgs ?? throw NullArgException);
        }
    }
}
