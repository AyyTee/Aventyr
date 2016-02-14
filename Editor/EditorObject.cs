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
        public EditorScene Scene { get; private set; }

        public Entity Marker { get; private set; }
        public bool IsSelected { get; private set; }

        List<EditorObject> _children = new List<EditorObject>();
        public List<EditorObject> Children { get { return new List<EditorObject>(_children); } }
        public EditorObject Parent { get; private set; }

        public EditorObject(EditorScene editorScene)
        {
            Debug.Assert(editorScene != null);
            Scene = editorScene;
            SetParent(Scene.Root);
            SetMarker();
        }

        private void SetMarker()
        {
            Marker = new Entity(Scene.Scene);
            //Marker.SetParent(this);
            Marker.DrawOverPortals = true;
            Model circle = ModelFactory.CreateCircle(new Vector3(), 0.05f, 10);
            circle.Transform.Position = new Vector3(0, 0, DrawDepth.EntityMarker);
            circle.SetColor(new Vector3(1f, 0.5f, 0f));
            Marker.AddModel(circle);
        }

        public virtual void SetScene(EditorScene scene)
        {
            Scene = scene;
        }

        public virtual List<IDeepClone> GetCloneableRefs()
        {
            return new List<IDeepClone>(Children);
        }

        public virtual void UpdateRefs(IReadOnlyDictionary<IDeepClone, IDeepClone> cloneMap)
        {
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

        protected virtual void ShallowClone(EditorObject destination)
        {
            destination._children = Children;
            destination.IsSelected = IsSelected;
            destination.SetTransform(GetTransform());
        }

        public virtual void SetParent(EditorObject parent)
        {
            if (Parent != null)
            {
                Parent._children.Remove(this);
            }
            Parent = parent;
            if (Parent != null)
            {
                Debug.Assert(parent.Scene == Scene, "Parent cannot be in a different scene.");
                parent._children.Add(this);
            }
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

        public void Remove()
        {
            SetParent(null);
            Marker.SetParent(null);
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
