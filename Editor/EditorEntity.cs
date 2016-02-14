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

        public EditorEntity(EditorScene editorScene)
            : base(editorScene)
        {
            Entity = new Entity(Scene.Scene);
            Entity.SetParent(Marker);
        }

        public override IDeepClone ShallowClone()
        {
            EditorEntity clone = new EditorEntity(Scene);
            ShallowClone(clone);
            return clone;
        }

        protected override void ShallowClone(EditorObject destination)
        {
            base.ShallowClone(destination);
            EditorEntity destinationCast = (EditorEntity)destination;
            destinationCast.Entity = Entity;
        }

        public override List<IDeepClone> GetCloneableRefs()
        {
            List<IDeepClone> list = base.GetCloneableRefs();
            list.Add((IDeepClone)Entity);
            return list;
        }
    }
}
