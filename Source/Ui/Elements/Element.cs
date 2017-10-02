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
        internal Dictionary<string, ElementFunc<object>> _funcCache = new Dictionary<string, ElementFunc<object>>();

        protected Element(
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<float> width = null,
            ElementFunc<float> height = null,
            ElementFunc<bool> hidden = null,
            Style style = null)
        {
            ElementArgs = new ElementArgs(null, this, null);
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _hidden = hidden;

            Style = style?.ToImmutable() ?? ImmutableDictionary<(Type, string), ElementFunc<object>>.Empty;
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
            if (element.Style.Count > 0)
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

            var parent = element.ElementArgs.Parent;

            // If we didn't find the func then we try adding in the default style and then repeating this process.
            if (parent == null)
            {
                var defaultStyles = RootStyle().ToImmutable();
                var previousAddedStyles = element.Style;

                var invalidKeys = previousAddedStyles.Keys
                    .Intersect(defaultStyles.Keys)
                    // This is a really hacky way to make sure the two funcs are equal.
                    .Where(key => ActionComparer(defaultStyles[key], previousAddedStyles[key]));
                DebugEx.Assert(!invalidKeys.Any(), $"The following root style elements are not consistent { string.Join(", ", invalidKeys)}");

                var newStyle = defaultStyles
                    .Except(previousAddedStyles, new StyleKeyValueComparer())
                    .Union(previousAddedStyles)
                    .ToImmutableDictionary();

                // If the unioned style isn't larger then we know there isn't a point in searching again.
                if (newStyle.Count == previousAddedStyles.Count)
                {
                    DebugEx.Fail($"No func found for {GetType().Name}.{funcName}");
                }
                DebugEx.Assert(newStyle.Count > previousAddedStyles.Count, "Somehow the unioned style got smaller.");

                element.Style = newStyle;

                return _invokeFunc<T>(this, funcName);
            }
            
            return _invokeFunc<T>(parent, funcName);
        }

        /// <summary>
        /// Returns a style intended to be used only by the root element. Classes derivded from Element should override this in the following manner:
        /// public override Style RootStyle()
        /// {
        ///     var baseStyle = base.RootStyle();
        ///     var style = new Style()
        ///     {
        ///         new StyleElement<DERIVED_CLASS, int>(),
        ///         new StyleElement<DERIVED_CLASS, float>(),
        ///         ...
        ///     }
        ///     return baseStyle.With(style);
        /// }
        /// 
        /// Note that derived classes should not use base types in place of DERIVED_CLASS.
        /// </summary>
        /// <returns></returns>
        public virtual Style RootStyle()
        {
            return new Style
            {
                new StyleElement<Element, float>(nameof(X), _ => 0f),
                new StyleElement<Element, float>(nameof(Y), _ => 0f),
                new StyleElement<Element, float>(nameof(Width), args => args.Parent.Width),
                new StyleElement<Element, float>(nameof(Height), args => args.Parent.Height),
                new StyleElement<Element, bool>(nameof(Hidden), args => false)
            };
        }

        class StyleKeyValueComparer : IEqualityComparer<KeyValuePair<(Type, string), ElementFunc<object>>>
        {
            public bool Equals(KeyValuePair<(Type, string), ElementFunc<object>> x, KeyValuePair<(Type, string), ElementFunc<object>> y)
            {
                return x.Key.Equals(y.Key);
            }

            public int GetHashCode(KeyValuePair<(Type, string), ElementFunc<object>> obj)
            {
                return obj.Key.GetHashCode();
            }
        }

        bool ActionComparer<T>(ElementFunc<T> firstAction, ElementFunc<T> secondAction)
        {
            if (firstAction.Target != secondAction.Target)
                return false;

            var firstMethodBody = firstAction.Method.GetMethodBody().GetILAsByteArray();
            var secondMethodBody = secondAction.Method.GetMethodBody().GetILAsByteArray();

            if (firstMethodBody.Length != secondMethodBody.Length)
                return false;

            for (var i = 0; i < firstMethodBody.Length; i++)
            {
                if (firstMethodBody[i] != secondMethodBody[i])
                    return false;
            }
            return true;
        }

        public virtual bool IsInside(Vector2 localPoint) => false;
        public virtual List<Model> GetModels(ModelArgs args) => new List<Model>();//Draw.Rectangle(new Vector2(), new Vector2(Width, Height), new Color4(0f, 0f, 0f, 0.3f)).GetModels();

        public virtual IEnumerator<Element> GetEnumerator() => new List<Element>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
