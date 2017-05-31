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
        public Dictionary<IGridEntity, IGridEntityInstant> Entities { get; private set; } = new Dictionary<IGridEntity, IGridEntityInstant>();
        [DataMember]
        public int Time;
        [DataMember]
        public Timeline<Player> PlayerTimeline = new Timeline<Player>();
        [DataMember]
        public List<Timeline<Block>> BlockTimelines = new List<Timeline<Block>>();
        public IEnumerable<ITimeline> Timelines => BlockTimelines
            .OfType<ITimeline>()
            .Concat(new[] { PlayerTimeline });
        [DataMember]
        public Player CurrentPlayer;
        public int StartTime => Timelines.Min(item => item.Path.Min(entity => entity.StartTime));

        public void SetTimeToStart()
        {
            Time = StartTime;
            Entities.Clear();
        }

        public SceneState DeepClone()
        {
            var clone = (SceneState)MemberwiseClone();

            clone.Entities = new Dictionary<IGridEntity, IGridEntityInstant>();
            foreach (var entity in Entities.Keys)
            {
                var cloneEntity = entity.DeepClone();
                clone.Entities.Add(cloneEntity, Entities[entity].DeepClone());
                if (entity == CurrentPlayer)
                {
                    clone.CurrentPlayer = (Player)cloneEntity;
                }
            }
            clone.PlayerTimeline = PlayerTimeline.DeepClone();
            clone.BlockTimelines = BlockTimelines.Select(item => item.DeepClone()).ToList();
            return clone;
        }
    }
}
