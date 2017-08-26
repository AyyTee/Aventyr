using Game.Common;
using Game.Models;
using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Ui
{
    /// <summary>
    /// Base implementation of IElement.
    /// </summary>
    public abstract class Element : IElement
    {
        public ElementArgs ElementArgs { get; set; }

        internal ElementFunc<float> XFunc { get; set; }
        internal ElementFunc<float> YFunc { get; set; }
        internal ElementFunc<float> WidthFunc { get; set; }
        internal ElementFunc<float> HeightFunc { get; set; }
        internal ElementFunc<bool> HiddenFunc { get; set; }

        [DetectLoop]
        public float X => InvokeFunc(XFunc, nameof(X));
        [DetectLoop]
        public float Y => InvokeFunc(YFunc, nameof(Y));
        [DetectLoop]
        public float Width => InvokeFunc(WidthFunc, nameof(Width));
        [DetectLoop]
        public float Height => InvokeFunc(HeightFunc, nameof(Height));
        [DetectLoop]
        public bool Hidden => InvokeFunc(HiddenFunc, nameof(Hidden));

        public virtual ImmutableDictionary<(Type ElementType, string FuncName), ElementFunc<object>> Style { get; internal set; } = new Dictionary<(Type, string), ElementFunc<object>>().ToImmutableDictionary();

        public Element(
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<float> width = null,
            ElementFunc<float> height = null,
            ElementFunc<bool> hidden = null)
        {
            ElementArgs = new ElementArgs(null, this);
            XFunc = x;
            YFunc = y;
            WidthFunc = width;
            HeightFunc = height;
            HiddenFunc = hidden;
        }

        public T InvokeFunc<T>(ElementFunc<T> func, string funcName)
        {
            // If a func is already set then we just invoke that.
            if (func != null)
            {
                return func(ElementArgs);
            }
            // Otherwise we recursively check parent elements for the specified func.
            var parent = (Element)ElementArgs.Parent;
            DebugEx.Assert(parent != null, $"No func found for {GetType().Name}.{funcName}");
            return parent._invokeFunc<T>(GetType(), funcName);
        }

        T _invokeFunc<T>(Type elementType, string funcName)
        {
            foreach (var type in GetType().InheritanceHierarchy())
            {
                var key = (type, funcName);
                if (Style.Keys.Contains(key))
                {
                    return (T)Style[key](ElementArgs);
                }
            }
            var parent = (Element)ElementArgs.Parent;
            DebugEx.Assert(parent != null, $"No func found for {elementType.Name}.{funcName}");
            return parent._invokeFunc<T>(elementType, funcName);
        }

        public static ImmutableDictionary<(Type, string), ElementFunc<object>> DefaultStyle()
        {
            return new Dictionary<(Type, string), ElementFunc<object>>
            {
                { (typeof(Element), nameof(XFunc)), _ => 0 },
                { (typeof(Element), nameof(YFunc)), _ => 0 },
                { (typeof(Element), nameof(WidthFunc)), args => args.Parent.Width },
                { (typeof(Element), nameof(HeightFunc)), args => args.Parent.Height },
                { (typeof(Element), nameof(HiddenFunc)), args => false }
            }.ToImmutableDictionary();
        }

        public virtual bool IsInside(Vector2 localPoint) => false;
        public virtual List<Model> GetModels(ModelArgs args) => new List<Model>();//Draw.Rectangle(new Vector2(), new Vector2(Width, Height), new Color4(0f, 0f, 0f, 0.3f)).GetModels();

        public virtual IEnumerator<IElement> GetEnumerator() => new List<IElement>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
