using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    [DataContract]
    public class SceneState : IDeepClone<SceneState>
    {
        [DataMember]
        public SceneInstant CurrentInstant = new SceneInstant();
        [DataMember]
        public Timeline<Player> PlayerTimeline = new Timeline<Player>();
        public Player CurrentPlayer => (Player)PlayerTimeline.Path.Last();
        [DataMember]
        public List<Timeline<Block>> BlockTimelines = new List<Timeline<Block>>();
        public IEnumerable<ITimeline> Timelines => BlockTimelines
            .OfType<ITimeline>()
            .Concat(new[] { PlayerTimeline });
        public int StartTime => Timelines.Min(item => item.Path.Min(entity => entity.StartTime));

        public void SetTimeToStart()
        {
            CurrentInstant = new SceneInstant();
            CurrentInstant.Time = StartTime;
        }

        public SceneState DeepClone()
        {
            var clone = (SceneState)MemberwiseClone();

            clone.CurrentInstant = CurrentInstant.DeepClone();
            clone.PlayerTimeline = PlayerTimeline.DeepClone();
            clone.BlockTimelines = BlockTimelines.Select(item => item.DeepClone()).ToList();
            return clone;
        }
    }
}
