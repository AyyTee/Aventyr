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
        public int Time { get; set; }

        public SceneInstant()
        {
        }

        public SceneInstant DeepClone()
        {
            var clone = new SceneInstant();
            clone.Time = Time;
            foreach (var entity in Entities.Keys)
            {
                // We don't clone the entity itself. Just the entity instant.
                clone.Entities.Add(entity, Entities[entity].DeepClone());
            }
            return clone;
        }
    }
}
