using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EditorLogic
{
    [DataContract, Affine, AffineMember]
    public sealed class EditorPortal : EditorObject, IPortal
    {
        [DataMember]
        public IPortal Linked { get; set; }
        public bool OneSided { get { return false; } }
        public override bool IgnoreScale { get { return true; } }
        Model _portalModel;

        public EditorPortal(EditorScene editorScene)
            : base(editorScene)
        {
            IsPortalable = false;
            Initialize();
        }

        public override IDeepClone ShallowClone()
        {
            EditorPortal clone = new EditorPortal(Scene);
            ShallowClone(clone);
            return clone;
        }

        public override void Initialize()
        {
            base.Initialize();
            _portalModel = ModelFactory.CreatePortal();
            _portalModel.Transform.Position += new Vector3(0, 0, 2);
        }

        public override List<Model> GetModels()
        {
            List<Model> models = base.GetModels();
            if (OnEdge)
            {
                _portalModel.SetColor(new Vector3(0, 0.8f, 0.5f));
            }
            else
            {
                _portalModel.SetColor(ModelFactory.ColorPortalDefault);
            }
            models.Add(_portalModel);
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
