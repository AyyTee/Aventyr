using Game.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Elements;

namespace Ui
{
    public class Style : IEnumerable<StyleElement>
    {
        public List<StyleElement> Elements { get; } = new List<StyleElement>();

        public void Add(StyleElement element)
        {
            Elements.Add(element);
        }

        public void Add(Style style)
        {
            foreach (var element in style.Elements)
            {
                DebugEx.Assert(!Elements.Exists(item => item.Key.Equals(element.Key)), "A style element key was found in both source and destination styles.");
                Elements.Add(element);
            }
        }

        public Style With(params Style[] styles)
        {
            var newStyle = new Style { this };
            foreach (var style in styles)
            {
                newStyle.Add(style);
            }
            return newStyle;
        }

        public IEnumerator<StyleElement> GetEnumerator() => Elements.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class StyleElement
    {
        public (Type, string) Key { get; }
        public ElementFunc<object> ElementFunc { get; }

        public StyleElement(Type type, string propertyName, ElementFunc<object> propertyFunc)
        {
            if (propertyFunc == null)
            {
                throw new NullReferenceException("Property function cannot be null.");
            }

            Key = (type, propertyName);
            ElementFunc = propertyFunc;
        }
    }

    public class StyleElement<TElement, TValue> : StyleElement where TElement : Element
    {
        public StyleElement(string propertyName, ElementFunc<TValue> propertyFunc)
            : base(typeof(TElement), propertyName, args => propertyFunc(args))
        {
            try
            {
                var property = typeof(TElement).GetProperty(propertyName);
                if (property.PropertyType != typeof(TValue))
                {
                    throw new InvalidCastException($"Property type \"{property.PropertyType}\" does match the function's return type \"{typeof(TValue)}\"");
                }
            }
            catch (ArgumentNullException e)
            {
                throw new InvalidCastException($"Property \"{propertyName}\" is not defined in class \"{nameof(TElement)}\".", e);
            }
        }
    }

    public static class StyleEx
    {
        public static ImmutableDictionary<(Type, string), ElementFunc<object>> ToImmutable(this IEnumerable<StyleElement> elements)
        {
            return elements.Select(item => new KeyValuePair<(Type, string), ElementFunc<object>>(item.Key, item.ElementFunc)).ToImmutableDictionary();
        }
    }
}
