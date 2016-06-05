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
    public class EditorObject : ITreeNode<EditorObject>, IPortalable, IDeepClone, IRenderable, ISceneObject
    {
        [DataMember]
        EditorScene _scene;
        public EditorScene Scene
        {
            get { return _scene == null ? Parent.Scene : _scene; }
            private set { _scene = value; }
        }
        [DataMember]
        public bool Visible { get; set; }
        [DataMember]
        public bool DrawOverPortals { get; set; }
        [DataMember]
        public bool IsPortalable { get; set; }
        [DataMember]
        public bool IsSelected { get; private set; }
        public bool OnEdge { get { return PolygonTransform != null; } }
        [DataMember]
        public string Name { get; set; }
        public virtual bool IgnoreScale { get { return false; } }
        [DataMember]
        List<EditorObject> _children = new List<EditorObject>();
        public List<EditorObject> Children { get { return new List<EditorObject>(_children); } }
        [DataMember]
        public EditorObject Parent { get; private set; }
        [DataMember]
        public Transform2 _transform = new Transform2();
        [DataMember]
        public IPolygonCoord PolygonTransform;
        public bool IsModified { get; set; }

        public EditorObject(EditorScene editorScene)
        {
            Name = "";
            Debug.Assert(editorScene != null);
            SetParent(editorScene);
            Visible = true;
            IsPortalable = true;
        }

        public virtual void Initialize()
        {
        }

        public virtual List<Model> GetModels()
        {
            List<Model> models = new List<Model>();
            Model marker = Game.ModelFactory.CreateCircle(new Vector3(), 0.05f, 10);
            marker.Transform.Position = new Vector3(0, 0, DrawDepth.EntityMarker);

            if (IsSelected)
            {
                marker.SetColor(new Vector3(1f, 1f, 0f));
                marker.Transform.Scale = new Vector3(1.5f, 1.5f, 1.5f);
            }
            else
            {
                marker.SetColor(new Vector3(1f, 0.5f, 0f));
            }

            models.Add(marker);
            return models;
        }

        public virtual void SetScene(EditorScene destination)
        {
            Scene._children.Remove(this);
            Scene = destination;
            Scene._children.Add(this);
        }

        public virtual HashSet<IDeepClone> GetCloneableRefs()
        {
            return new HashSet<IDeepClone>(Children);
        }

        public virtual void UpdateRefs(IReadOnlyDictionary<IDeepClone, IDeepClone> cloneMap)
        {
            if (Parent != null && cloneMap.ContainsKey(Parent))
            {
                Parent = (EditorObject)cloneMap[Parent];
            }
            else
            {
                Parent = null;
            }
            List<EditorObject> children = Children;
            _children.Clear();
            foreach (EditorObject e in children)
            {
                _children.Add((EditorObject)cloneMap[e]);
            }
        }

        public virtual IDeepClone ShallowClone()
        {
            EditorObject clone = new EditorObject(Scene);
            ShallowClone(clone);
            return clone;
        }

        protected void ShallowClone(EditorObject destination)
        {
            destination._children = Children;
            destination.IsSelected = IsSelected;
            destination.SetTransform(GetTransform());
            destination.Name = Name + " Clone";
        }

        public virtual void SetParent(EditorScene scene)
        {
            Debug.Assert(scene != null);
            RemoveSelf();
            Scene = scene;
            Scene._children.Add(this);
            Parent = null;
        }

        public virtual void SetParent(EditorObject parent)
        {
            Debug.Assert(parent != null);
            RemoveSelf();
            Parent = parent;
            parent._children.Add(this);
            Scene = null;
            Debug.Assert(!Tree<EditorObject>.ParentLoopExists(this), "Cannot have cycles in Parent tree.");
        }

        public virtual void SetTransform(Transform2 transform)
        {
            /*if (PolygonTransform != null)
            {
                PolygonTransform = null;
                SetParent(Scene);
            }*/
            _transform = transform.ShallowClone();
        }

        public virtual Transform2 GetTransform()
        {
            /*if (_transform == null)
            {
                return PolygonExt.GetTransform(((IWall)Parent).Vertices, PolygonTransform);
            }
            return _transform.ShallowClone();*/
            /*if (PolygonTransform != null)
            {
                return _transform.Transform(PolygonExt.GetTransform(((IWall)Parent).Vertices, PolygonTransform));
            }*/
            return _transform.ShallowClone();
        }

        public Transform2 GetTransformWithPolygon()
        {
            if (PolygonTransform != null)
            {
                return _transform.Transform(PolygonExt.GetTransform(((IWall)Parent).Vertices, PolygonTransform));
            }
            return _transform.ShallowClone();
        }

        public virtual Transform2 GetWorldTransform()
        {
            Transform2 transform;
            if (PolygonTransform != null)
            {
                transform = _transform.Transform(PolygonExt.GetTransform(((IWall)Parent).Vertices, PolygonTransform));
            }
            else
            {
                transform = GetTransform();
            }
            if (Parent != null)
            {
                transform = transform.Transform(Parent.GetWorldTransform());
                
                if (IgnoreScale)
                {
                    transform.SetScale(GetTransform().Scale);
                }
                return transform;
            }
            return transform;
        }

        public Transform2 GetVelocity()
        {
            return new Transform2();
        }

        public void SetVelocity(Transform2 velocity)
        {
        }

        public Transform2 GetWorldVelocity()
        {
            return new Transform2();
        }

        private void RemoveSelf()
        {
            if (Parent != null)
            {
                Parent._children.Remove(this);
                Parent = null;
            }
            if (_scene != null)
            {
                _scene._children.Remove(this);
                Scene = null;
            }
        }

        public virtual void Remove()
        {
            RemoveSelf();
        }

        public virtual void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
        }

        /// <summary>
        /// Set transform as FixtureEdgeCoord.  This EditorPortal's parent will become the EditorObject 
        /// associated with the FixtureEdgeCoord's fixture.
        /// </summary>
        /// <param name="transform"></param>
        public void SetTransform(IWall wall, IPolygonCoord transform)
        {
            _transform = new Transform2();
            PolygonTransform = transform;
            SetParent((EditorObject)wall);
        }

        public IPolygonCoord GetPolygonCoord()
        {
            return OnEdge ? PolygonTransform.ShallowClone() : null;
        }

        /// <summary>
        /// Set name of this EditorObject and flag it as modified.
        /// </summary>
        public void SetName(string name)
        {
            if (name != Name)
            {
                Name = name;
                IsModified = true;
            }
        }
    }
}
