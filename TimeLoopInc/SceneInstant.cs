using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Game.Common;

namespace TimeLoopInc
{
    [DataContract]
    public class SceneInstant : IDeepClone<SceneInstant>
    {
        [DataMember]
        public Dictionary<IGridEntity, IGridEntityInstant> Entities { get; private set; } = new Dictionary<IGridEntity, IGridEntityInstant>();

        public IGridEntityInstant this[IGridEntity entity] => Entities[entity];

        [DataMember]
        public int Time { get; private set; }

        public SceneInstant(int time)
        {
            Time = time;
        }

        public SceneInstant DeepClone()
        {
            var clone = new SceneInstant(Time);
            foreach (var entity in Entities.Keys)
            {
                // We don't clone the entity itself. Just the entity instant.
                clone.Entities.Add(entity, Entities[entity].DeepClone());
            }
            return clone;
        }

        public SceneInstant WithTime(int time)
        {
            var clone = DeepClone();
            clone.Time = time;
            return clone;
        }
    }
}
