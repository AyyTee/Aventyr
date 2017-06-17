using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class Timeline<T> : ITimeline, IDeepClone<Timeline<T>> where T : IGridEntity, IDeepClone<IGridEntity>, new()
    {
        public T this[int index]
        {
            get { return (T)Path[index]; }
        }

        public List<IGridEntity> Path { get; private set; } = new List<IGridEntity>();

        public void Add(T entity) => Path.Add(entity);

        public T CreateInstant(IGridEntity entity)
        {
            Debug.Assert(Path.Contains(entity));
            return new T();
        }

        public Timeline<T> DeepClone()
        {
            return new Timeline<T>()
            {
                Path = Path.Select(item => item.DeepClone()).ToList()
            };
        }
    }

    public interface ITimeline
    {
        List<IGridEntity> Path { get; }
    }
}
