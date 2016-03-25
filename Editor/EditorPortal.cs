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

        public EditorPortal(EditorScene editorScene, bool initialize = true)
            : base(editorScene)
        {
            if (initialize)
            {
                PortalEntity = new Entity(Scene.Scene);
                Model arrow0, arrow1;
                arrow0 = ModelFactory.CreateArrow(new Vector3(0, -0.5f, 0), new Vector2(0, 1), 0.05f, 0.2f, 0.1f);
                arrow0.SetColor(new Vector3(0.1f, 0.1f, 0.5f));
                PortalEntity.AddModel(arrow0);
                arrow1 = ModelFactory.CreateArrow(new Vector3(), new Vector2(0.2f, 0), 0.05f, 0.2f, 0.1f);
                arrow1.SetColor(new Vector3(0.1f, 0.1f, 0.5f));
                PortalEntity.AddModel(arrow1);
                Portal = new FloatPortal(Scene.Scene);
                Portal.SetParent(PortalEntity);
            }
        }

        public override IDeepClone ShallowClone()
        {
            EditorPortal clone = new EditorPortal(Scene, false);
            ShallowClone(clone);
            return clone;
        }

        protected void ShallowClone(EditorPortal destination)
        {
            base.ShallowClone(destination);
            destination.Portal = Portal;
            destination.PortalEntity = PortalEntity;
        }

        public override HashSet<IDeepClone> GetCloneableRefs()
        {
            HashSet<IDeepClone> list = base.GetCloneableRefs();
            list.Add(Portal);
            list.Add(PortalEntity);
            return list;
        }

        public override void UpdateRefs(IReadOnlyDictionary<IDeepClone, IDeepClone> cloneMap)
        {
            base.UpdateRefs(cloneMap);
            Portal = (FloatPortal)cloneMap[Portal];
            PortalEntity = (Entity)cloneMap[PortalEntity];
        }

        public override void SetTransform(Transform2 transform)
        {
            base.SetTransform(transform);
            if (PortalEntity != null)
            {
                PortalEntity.SetTransform(transform);
            }
        }

        public override void Remove()
        {
            base.Remove();
            Portal.SetLinked(null);
            PortalEntity.Remove();
        }

        public override void SetScene(EditorScene destination)
        {
            base.SetScene(destination);
            PortalEntity.SetParent(destination.Scene.Root);
        }
    }
}
