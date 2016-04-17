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
    public class EditorPortal : EditorObject, IPortal
    {
        [DataMember]
        public IPortal Linked { get; set; }
        public bool OneSided { get { return false; } }
        [DataMember]
        public bool IsMirrored { get; set; }
        List<Model> _models = new List<Model>();

        public EditorPortal(EditorScene editorScene, bool initialize = true)
            : base(editorScene)
        {
            //DrawOverPortals = true;
            IsPortalable = false;
            if (initialize)
            {
                Model arrow0, arrow1;
                arrow0 = ModelFactory.CreateArrow(new Vector3(0, -0.5f, 0), new Vector2(0, 1), 0.05f, 0.2f, 0.1f);
                arrow0.SetColor(new Vector3(0.1f, 0.1f, 0.5f));
                arrow1 = ModelFactory.CreateArrow(new Vector3(), new Vector2(0.2f, 0), 0.05f, 0.2f, 0.1f);
                arrow1.SetColor(new Vector3(0.1f, 0.1f, 0.5f));

                _models.Add(arrow0);
                _models.Add(arrow1);
            }
        }

        public override IDeepClone ShallowClone()
        {
            EditorPortal clone = new EditorPortal(Scene, false);
            ShallowClone(clone);
            return clone;
        }

        public Transform2 GetWorldVelocity()
        {
            return new Transform2();
        }

        public override List<Model> GetModels()
        {
            List<Model> models = base.GetModels();
            models.AddRange(_models);
            return models;
        }

        public override void Remove()
        {
            base.Remove();
            if (Linked != null && Linked.Linked == this)
            {
                ((EditorPortal)Linked).Linked = null;
            }
        }
    }
}
