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
    public class EditorPortal : EditorObject
    {
        [DataMember]
        public FloatPortal Portal { get; private set; }
        [DataMember]
        public Entity PortalEntity { get; private set; }

        public EditorPortal(EditorScene editorScene)
            : base(editorScene)
        {
            PortalEntity = new Entity(Scene.Scene);
            PortalEntity.SetParent(Marker);
            Model arrow0, arrow1;
            arrow0 = ModelFactory.CreateArrow(new Vector3(0, -0.5f, 0), new Vector2(0, 1), 0.05f, 0.2f, 0.1f);
            arrow0.SetColor(new Vector3(0.1f, 0.1f, 0.5f));
            PortalEntity.AddModel(arrow0);
            arrow1 = ModelFactory.CreateArrow(new Vector3(), new Vector2(0.2f, 0), 0.05f, 0.2f, 0.1f);
            arrow1.SetColor(new Vector3(0.1f, 0.1f, 0.5f));
            PortalEntity.AddModel(arrow1);
            Portal = new FloatPortal(Scene.Scene);
            Portal.SetParent(Marker);
        }

        public override IDeepClone ShallowClone()
        {
            EditorPortal clone = new EditorPortal(Scene);
            ShallowClone(clone);
            return clone;
        }

        protected override void ShallowClone(EditorObject destination)
        {
            base.ShallowClone(destination);
            EditorPortal destinationCast = (EditorPortal)destination;
            destinationCast.Portal = Portal;
            destinationCast.PortalEntity = PortalEntity;
        }

        public override List<IDeepClone> GetCloneableRefs()
        {
            List<IDeepClone> list = base.GetCloneableRefs();
            list.Add((IDeepClone)Portal);
            list.Add((IDeepClone)PortalEntity);
            return list;
        }

        public override void UpdateRefs(IReadOnlyDictionary<IDeepClone, IDeepClone> cloneMap)
        {
            Portal = (FloatPortal)cloneMap[(IDeepClone)Portal];
            PortalEntity = (Entity)cloneMap[(IDeepClone)PortalEntity];
        }
    }
}
