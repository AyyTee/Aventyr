using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class SceneState// : IDeepClone<SceneState>
    {
        public Dictionary<IGridEntity, IGridEntityInstant> Entities { get; private set; } = new Dictionary<IGridEntity, IGridEntityInstant>();
        public int Time;
        public Timeline<Player> PlayerTimeline = new Timeline<Player>();
        public List<Timeline<Block>> BlockTimelines = new List<Timeline<Block>>();
        public IEnumerable<ITimeline> Timelines => BlockTimelines
            .OfType<ITimeline>()
            .Concat(new[] { PlayerTimeline });
        public Player CurrentPlayer;
        public int StartTime => Timelines.Min(item => item.Path.Min(entity => entity.StartTime));

        public void SetTimeToStart()
        {
            Time = StartTime;
            Entities.Clear();
        }

        //public SceneState DeepClone()
        //{
        //    var clone = (SceneState)MemberwiseClone();

        //    clone.Entities = new Dictionary<IGridEntity, IGridEntityInstant>();
        //    foreach (var entity in Entities.Keys)
        //    {
        //        clone.Entities.Add(entity.DeepClone(), Entities[entity].DeepClone());
        //    }
        //    return clone;
        //}
    }
}
