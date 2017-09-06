using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Ui.Elements;

namespace Ui
{
    public class DataTemplate<T> : IDataTemplate
    {
        public Func<OrderedSet<T>> DataFunc { get; }
        public Func<T, Element> TemplateFunc { get; }

        Dictionary<T, Element> _cachedElements = new Dictionary<T, Element>();

        public DataTemplate(Func<OrderedSet<T>> data, Func<T, Element> template)
        {
            DataFunc = data;
            TemplateFunc = template;
        }

        public IEnumerable<Element> GetElements()
        {
            var newCache = new Dictionary<T, Element>();
            var data = DataFunc();
            var elements = data.Select(item =>
                {
                    var element = _cachedElements.GetOrDefault(item) ?? TemplateFunc(item);
                    newCache.Add(item, element);
                    return element;
                }).ToList();

            _cachedElements = newCache;
            return elements;
        }
    }

    public interface IDataTemplate : IBaseElement
    {
        IEnumerable<Element> GetElements();
    }
}
