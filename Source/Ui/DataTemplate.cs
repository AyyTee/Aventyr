using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Ui.Elements;

namespace Ui
{
    public class DataTemplate<TData, TTemplate> : IDataTemplate<TTemplate> where TTemplate : class
    {
        public Func<OrderedSet<TData>> Data { get; }
        public Func<TData, TTemplate> Template { get; }

        Dictionary<TData, TTemplate> _cachedElements = new Dictionary<TData, TTemplate>();

        public DataTemplate(Func<OrderedSet<TData>> data, Func<TData, TTemplate> template)
        {
            Data = data;
            Template = template;
        }

        public IEnumerable<TTemplate> GetElements()
        {
            var newCache = new Dictionary<TData, TTemplate>();
            var data = Data();
            var elements = data.Select(item =>
                {
                    var element = _cachedElements.GetOrDefault(item) ?? Template(item);
                    newCache.Add(item, element);
                    return element;
                }).ToList();

            _cachedElements = newCache;
            return elements;
        }
    }

    public interface IDataTemplate<out TTemplate> : IBaseElement
    {
        IEnumerable<TTemplate> GetElements();
    }
}
