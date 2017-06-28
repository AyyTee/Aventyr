using Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class Timeline<T> : ITimeline, IDeepClone<Timeline<T>> where T : IGridEntity, IDeepClone<IGridEntity>
    {
        public T this[int index] => (T)Path[index];

        public List<IGridEntity> Path { get; private set; } = new List<IGridEntity>();

        public void Add(T entity) => Path.Add(entity);

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

    public static class ITimelineEx
    {
        public static int MinTime(this ITimeline timeline)
        {
            return timeline.Path.MinOrNull(item => item.StartTime) ?? 0;
        }

        public static int MaxTime(this ITimeline timeline)
        {
            var start = timeline.MinTime();
            return timeline.Path
                .MaxOrNull(item => item.EndTime == int.MaxValue ? start : item.EndTime) ?? start;
        }
    }
}
