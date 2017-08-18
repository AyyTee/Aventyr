using Game.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui
{
    /// <summary>
    /// A list of unique items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OrderedSet<T> : IList<T>
    {
        List<T> _list = new List<T>();

        public T this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Count => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();

        public OrderedSet()
        {
        }

        /// <summary>
        /// Add an item. If this item already exists then remove the previous one first.
        /// </summary>
        public void Add(T item)
        {
            _list.Remove(item);
            _list.Add(item);
        }

        /// <summary>
        /// Add several items. If this item already exists then remove the previous one first.
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public void Insert(int index, T item)
        {
            var oldIndex = _list.IndexOf(item);
            if (oldIndex != -1)
            {
                if (oldIndex < index)
                {
                    index--;
                }
                _list.Remove(item);
            }
            _list.Insert(index, item);
        }

        public void Clear() => _list.Clear();
        public bool Contains(T item) => _list.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        public int IndexOf(T item) => _list.IndexOf(item);
        public bool Remove(T item) => _list.Remove(item);
        public void RemoveAt(int index) => _list.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
