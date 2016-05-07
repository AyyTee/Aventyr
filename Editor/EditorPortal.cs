using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool IsFixed { get { return _polygonTransform != null; } }
        public override bool IgnoreScale { get { return true; } }
        [DataMember]
        IPolygonCoord _polygonTransform;
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
            if (IsFixed)
            {
                _portalModel.SetColor(new Vector3(0, 0.8f, 0.5f));
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

        public override Transform2 GetTransform()
        {
            return _polygonTransform == null ? 
                base.GetTransform() : PolygonExt.GetTransform(((IWall)Parent).Vertices, _polygonTransform);
        }

        public override void SetTransform(Transform2 transform)
        {
            base.SetTransform(transform);
            _polygonTransform = null;
        }

        /// <summary>
        /// Set transform as FixtureEdgeCoord.  This EditorPortal's parent will become the EditorObject 
        /// associated with the FixtureEdgeCoord's fixture.
        /// </summary>
        /// <param name="transform"></param>
        public void SetTransform(IWall wall, IPolygonCoord transform)
        {
            _polygonTransform = transform;
            SetParent((EditorObject)wall);
            //SetParent((EditorObject)FixtureExt.GetUserData(transform.Fixture).Entity);
        }

        public IPolygonCoord GetPolygonCoord()
        {
            return _polygonTransform.ShallowClone();
        }
    }
}
