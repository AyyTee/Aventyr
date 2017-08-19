using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;

namespace Ui
{
    public class DataTemplate<T> : IDataTemplate
    {
        public Func<OrderedSet<T>> DataFunc { get; }
        public Func<T, IElement> TemplateFunc { get; }

        Dictionary<T, IElement> _cachedElements = new Dictionary<T, IElement>();

        public DataTemplate(Func<OrderedSet<T>> data, Func<T, IElement> template)
        {
            DataFunc = data;
            TemplateFunc = template;
        }

        public IEnumerable<IElement> GetElements()
        {
            var newCache = new Dictionary<T, IElement>();
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
        IEnumerable<IElement> GetElements();
    }
}
