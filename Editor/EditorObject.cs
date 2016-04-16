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
    public class EditorObject : ITreeNode<EditorObject>, ITransform2, IDeepClone, IRenderable
    {
        [DataMember]
        EditorScene _scene;
        public EditorScene Scene 
        { 
            get { return _scene == null ? Parent.Scene : _scene; }
            private set { _scene = value; }
        }
        /*[DataMember]
        public Entity Marker { get; private set; }*/
        Model _marker;
        [DataMember]
        public bool Visible { get; private set; }
        public bool DrawOverPortals { get; set; }
        public bool IsPortalable { get { return true; } }
        [DataMember]
        public bool IsSelected { get; private set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        List<EditorObject> _children = new List<EditorObject>();
        public List<EditorObject> Children { get { return new List<EditorObject>(_children); } }
        [DataMember]
        public EditorObject Parent { get; private set; }
        [DataMember]
        Transform2 _transform = new Transform2();
        bool _initialized = false;

        public EditorObject(EditorScene editorScene)
        {
            Name = "";
            Debug.Assert(editorScene != null);
            SetParent(editorScene);
            Visible = true;
            Initialize();
            //SetMarker();
        }

        public EditorObject(EditorObject entity)
        {
            Name = "";
            Debug.Assert(entity != null);
            SetParent(entity);
            Visible = true;
            Initialize();
            //SetMarker();
        }

        /// <summary>
        /// Initializes instance state. It is expected to be called in the constructor or during deserialization. 
        /// Should only be called once during the lifetime of this instance.
        /// </summary>
        public virtual void Initialize()
        {
            Debug.Assert(_initialized == false);
            _initialized = true;
            _marker = ModelFactory.CreateCircle(new Vector3(), 0.05f, 10);
            _marker.Transform.Position = new Vector3(0, 0, DrawDepth.EntityMarker);
            _marker.SetColor(new Vector3(1f, 0.5f, 0f));
        }

        public virtual List<Model> GetModels()
        {
            List<Model> models = new List<Model>();
            models.Add(_marker);
            return models;
        }

        /*public void SetMarker()
        {
            if (Marker != null)
            {
                Marker.Remove();
            }
            Marker = new Entity(Scene.Scene);
            Marker.Name = "Marker";
            Marker.DrawOverPortals = true;
            Model circle = ModelFactory.CreateCircle(new Vector3(), 0.05f, 10);
            circle.Transform.Position = new Vector3(0, 0, DrawDepth.EntityMarker);
            circle.SetColor(new Vector3(1f, 0.5f, 0f));
            Marker.AddModel(circle);
            Marker.SetTransform(GetTransform());
        }*/

        public virtual void SetScene(EditorScene destination)
        {
            Scene._children.Remove(this);
            Scene = destination;
            Scene._children.Add(this);
            //Marker.SetParent(Scene.Scene.Root);
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
            //Marker = (Entity)cloneMap[Marker];
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
            _transform = transform.ShallowClone();
            //Marker.SetTransform(_transform);
        }

        public Transform2 GetWorldTransform()
        {
            return GetTransform();
        }

        public virtual Transform2 GetTransform()
        {
            return _transform.ShallowClone();
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
            //Marker.Remove();
        }

        public virtual void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
            if (IsSelected)
            {
                _marker.SetColor(new Vector3(1f, 1f, 0f));
                _marker.Transform.Scale = new Vector3(1.5f, 1.5f, 1.5f);
            }
            else
            {
                _marker.SetColor(new Vector3(1f, 0.5f, 0f));
                _marker.Transform.Scale = new Vector3(1f, 1f, 1f);
            }
        }

        public virtual void SetVisible(bool isVisible)
        {
            Visible = isVisible;
        }
    }
}
