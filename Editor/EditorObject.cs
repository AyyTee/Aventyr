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
    public class EditorObject : ITreeNode<EditorObject>, ITransform2, IDeepClone
    {
        EditorScene _scene;
        public EditorScene Scene 
        { 
            get
            {
                if (_scene == null)
                {
                    return Parent.Scene;
                }
                return _scene;
            }
            private set { _scene = value; }
        }

        public Entity Marker { get; private set; }
        public bool IsSelected { get; private set; }
        public string Name { get; set; }

        List<EditorObject> _children = new List<EditorObject>();
        public List<EditorObject> Children { get { return new List<EditorObject>(_children); } }
        public EditorObject Parent { get; private set; }

        public EditorObject(EditorScene editorScene)
        {
            Name = "";
            Debug.Assert(editorScene != null);
            SetParent(editorScene);
            SetMarker();
        }

        public EditorObject(EditorObject entity)
        {
            Name = "";
            Debug.Assert(entity != null);
            SetParent(entity);
            SetMarker();
        }

        private void SetMarker()
        {
            Marker = new Entity(Scene.Scene);
            Marker.Name = "Marker";
            //Marker.SetParent(this);
            Marker.DrawOverPortals = true;
            Model circle = ModelFactory.CreateCircle(new Vector3(), 0.05f, 10);
            circle.Transform.Position = new Vector3(0, 0, DrawDepth.EntityMarker);
            circle.SetColor(new Vector3(1f, 0.5f, 0f));
            Marker.AddModel(circle);
        }

        public virtual void SetScene(EditorScene scene)
        {
            Scene.Children.Remove(this);
            Scene = scene;
            Scene.Children.Add(this);
            Marker.SetScene(scene.Scene);
        }

        public virtual List<IDeepClone> GetCloneableRefs()
        {
            List<IDeepClone> list = new List<IDeepClone>();
            list.AddRange(Children);
            //list.Add(Marker);
            return list;
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
            Scene.Children.Add(this);
            Parent = null;
        }

        public virtual void SetParent(EditorObject parent)
        {
            Debug.Assert(parent != null);
            RemoveSelf();
            /*if (Parent != null)
            {
                Parent._children.Remove(this);
            }*/
            Parent = parent;
            parent._children.Add(this);
            Scene = null;
            Debug.Assert(!Tree<EditorObject>.ParentLoopExists(this), "Cannot have cycles in Parent tree.");
        }

        public virtual void SetTransform(Transform2 transform)
        {
            Marker.SetTransform(transform);
        }

        public Transform2 GetWorldTransform()
        {
            return GetTransform();
        }

        public virtual Transform2 GetTransform()
        {
            return Marker.GetTransform();
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
                _scene.Children.Remove(this);
                Scene = null;
            }
        }

        public virtual void Remove()
        {
            RemoveSelf();
            Marker.Remove();
        }

        public virtual void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
            if (IsSelected)
            {
                Marker.ModelList[0].SetColor(new Vector3(1f, 1f, 0f));
                Marker.ModelList[0].Transform.Scale = new Vector3(1.5f, 1.5f, 1.5f);
            }
            else
            {
                Marker.ModelList[0].SetColor(new Vector3(1f, 0.5f, 0f));
                Marker.ModelList[0].Transform.Scale = new Vector3(1f, 1f, 1f);
            }
        }

        public virtual void SetVisible(bool isVisible)
        {
            Marker.Visible = isVisible;
        }
    }
}
