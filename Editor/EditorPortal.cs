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
        public bool IsFixed { get { return _fixtureTransform != null; } }
        public override bool IgnoreScale { get { return false; } }
        [DataMember]
        FixtureEdgeCoord _fixtureTransform;
        List<Model> _models = new List<Model>();

        public EditorPortal(EditorScene editorScene)
            : base(editorScene)
        {
            IsPortalable = false;
        }

        public override IDeepClone ShallowClone()
        {
            EditorPortal clone = new EditorPortal(Scene);
            ShallowClone(clone);
            return clone;
        }

        public override List<Model> GetModels()
        {
            List<Model> models = base.GetModels();
            Model[] portalModel = ModelFactory.CreatePortal();
            
            foreach (Model m in portalModel)
            {
                if (IsFixed)
                {
                    m.SetColor(new Vector3(0, 0.8f, 0.5f));
                }
                m.Transform.Position += new Vector3(0,0,2);
            }

            models.AddRange(portalModel);
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
            return _fixtureTransform == null ? base.GetTransform() : _fixtureTransform.GetTransform();
        }

        public override void SetTransform(Transform2 transform)
        {
            base.SetTransform(transform);
            _fixtureTransform = null;
        }

        /// <summary>
        /// Set transform as FixtureEdgeCoord.  This EditorPortal's parent will become the EditorObject 
        /// associated with the FixtureEdgeCoord's fixture.
        /// </summary>
        /// <param name="transform"></param>
        public void SetTransform(FixtureEdgeCoord transform)
        {
            _fixtureTransform = transform;
            SetParent((EditorObject)FixtureExt.GetUserData(transform.Fixture).Entity);
        }

        public FixtureEdgeCoord GetFixtureEdgeCoord()
        {
            return _fixtureTransform.ShallowClone();
        }
    }
}
