using Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace TimeLoopInc
{
    public class Timeline<T> : ITimeline, IDeepClone<Timeline<T>> where T : IGridEntity, IDeepClone<IGridEntity>
    {
        public T this[int index] => (T)Path[index];

        public IList<IGridEntity> Path { get; private set; } = new List<IGridEntity>();

        public string Name => typeof(T).Name + " Timeline";

        public void Add(T entity) => Path.Add(entity);

        public Timeline<T> DeepClone()
        {
            return new Timeline<T>
            {
                Path = Path.Select(item => item.DeepClone()).ToList()
            };
        }

        public IEnumerator<IGridEntity> GetEnumerator()
        {
            return Path.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Path.GetEnumerator();
        }
    }

    public interface ITimeline : IEnumerable<IGridEntity>
    {
        string Name { get; }
        IList<IGridEntity> Path { get; }
    }

    public static class ITimelineEx
    {
        public static int MinTime(this ITimeline timeline)
        {
            return timeline.MinOrNull(item => item.StartTime) ?? 0;
        }

        public static int MaxTime(this ITimeline timeline)
        {
            var start = timeline.MinTime();
            return timeline
                .MaxOrNull(item => item.EndTime == int.MaxValue ? start : item.EndTime) ?? start;
        }
    }
}
