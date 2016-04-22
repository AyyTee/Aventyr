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
    public sealed class EditorPortal : EditorObject, IPortal
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
            IsPortalable = false;
            if (initialize)
            {
                _models.AddRange(ModelFactory.CreatePortal());
            }
        }

        public override IDeepClone ShallowClone()
        {
            EditorPortal clone = new EditorPortal(Scene, false);
            ShallowClone(clone);
            return clone;
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
