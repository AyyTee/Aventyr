using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                if (!Elements.Exists(item => item.Key.Equals(element.Key)))
                {
                    Elements.Add(element);
                }
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

        public StyleElement(Type elementType, string funcName, ElementFunc<object> elementFunc)
        {
            Key = (elementType, funcName);
            ElementFunc = elementFunc;
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
