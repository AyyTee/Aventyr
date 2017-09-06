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
using System.Runtime.CompilerServices;

namespace Ui.Elements
{
    /// <summary>
    /// Base implementation of IElement.
    /// </summary>
    public abstract class Element : IEnumerable<Element>, IElement
    {
        public ElementArgs ElementArgs { get; set; }

        internal ElementFunc<float> _x;
        internal ElementFunc<float> _y;
        internal ElementFunc<float> _width;
        internal ElementFunc<float> _height;
        internal ElementFunc<bool> _hidden;

        public float X => GetValue(_x);
        public float Y => GetValue(_y);
        public float Width => GetValue(_width);
        public float Height => GetValue(_height);
        public bool Hidden => GetValue(_hidden);

        internal ImmutableDictionary<(Type ElementType, string FuncName), ElementFunc<object>> Style { get; set; }
        Dictionary<string, ElementFunc<object>> _funcCache = new Dictionary<string, ElementFunc<object>>();

        protected Element(
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<float> width = null,
            ElementFunc<float> height = null,
            ElementFunc<bool> hidden = null,
            Style style = null)
        {
            ElementArgs = new ElementArgs(null, this);
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _hidden = hidden;

            Style = style?.ToImmutable();
        }

        public T GetValue<T>(ElementFunc<T> func, [CallerMemberName]string funcName = null)
        {
            DebugEx.Assert(funcName != null);

            using (var stackMethod = new DetectLoop.StackEntry(this, funcName))
            {
                if (!DetectLoop.OnEntry(stackMethod))
                {
                    return default(T);
                }

                // If the result has been cached, invoke that.
                if (_funcCache.ContainsKey(funcName))
                {
                    return (T)_funcCache[funcName](ElementArgs);
                }
                // If a func is already set then we just invoke that.
                if (func != null)
                {
                    _funcCache.Add(funcName, args => func(args));
                    return func(ElementArgs);
                }
                // Otherwise we recursively check parent elements for the specified func.
                return _invokeFunc<T>(this, funcName);
            }
        }

        T _invokeFunc<T>(Element element, string funcName)
        {
            if (element.Style != null && element.Style.Count > 0)
            {
                foreach (var type in GetType().InheritanceHierarchy())
                {
                    var key = (type, funcName);
                    if (element.Style.ContainsKey(key))
                    {
                        var func = element.Style[key];
                        _funcCache.Add(funcName, func);
                        return (T)func(ElementArgs);
                    }
                }
            }

            var parent = (Element)element.ElementArgs.Parent;
            DebugEx.Assert(parent != null, $"No func found for {GetType().Name}.{funcName}");
            return _invokeFunc<T>(parent, funcName);
        }

        public static Style DefaultStyle(IUiController controller)
        {
            var type = typeof(Element);
            return new Style
            {
                new StyleElement(type, nameof(X), _ => 0f),
                new StyleElement(type, nameof(Y), _ => 0f),
                new StyleElement(type, nameof(Width), args => args.Parent.Width),
                new StyleElement(type, nameof(Height), args => args.Parent.Height),
                new StyleElement(type, nameof(Hidden), args => false)
            };
        }

        public virtual bool IsInside(Vector2 localPoint) => false;
        public virtual List<Model> GetModels(ModelArgs args) => new List<Model>();//Draw.Rectangle(new Vector2(), new Vector2(Width, Height), new Color4(0f, 0f, 0f, 0.3f)).GetModels();

        public virtual IEnumerator<Element> GetEnumerator() => new List<Element>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
