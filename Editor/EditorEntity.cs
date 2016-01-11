using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    [DataContract]
    public class EditorEntity : EditorObject
    {
        [DataMember]
        public Entity Entity { get; private set; }

        public EditorEntity(Scene scene)
            : base(scene)
        {
            Entity = new Entity(Scene);
            Entity.SetParent(this);
        }

        protected override void DeepCloneFinalize(Dictionary<SceneNode, SceneNode> cloneMap)
        {
            base.DeepCloneFinalize(cloneMap);
            EditorEntity clone = (EditorEntity)cloneMap[this];
            clone.Entity = (Entity)cloneMap[Entity];
        }

        public override SceneNode Clone(Scene scene)
        {
            EditorEntity clone = new EditorEntity(scene);
            Clone(clone);
            return clone;
        }
    }
}
