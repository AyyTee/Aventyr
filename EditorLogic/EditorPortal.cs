using Game;
using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Game.Models;
using Game.Serialization;
using Game.Rendering;
using OpenTK.Graphics;

namespace EditorLogic
{
    [DataContract]
    public sealed class EditorPortal : EditorObject, IPortal
    {
        [DataMember]
        public IPortal Linked { get; set; }
        IPortalRenderable IPortalRenderable.Linked => Linked;
        public bool OneSided => false;

        public EditorPortal(EditorScene editorScene)
            : base(editorScene)
        {
            Path = new PortalPath();
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
        }

        public override List<Model> GetModels()
        {
            List<Model> models = base.GetModels();
            var portalColor = OnEdge ?
                new Color4(0, 0.8f, 0.5f, 1) :
                ModelFactory.ColorPortalDefault;
            var portal = ModelFactory.CreatePortal(portalColor);
            portal.Transform.Position += new Vector3(0, 0, 2);
            models.Add(portal);
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
