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
    public class EditorObject : ITreeNode<EditorObject>, ITransform2
    {
        public readonly EditorScene EditorScene;

        public Entity Marker { get; private set; }
        public bool IsSelected { get; private set; }

        List<EditorObject> _children = new List<EditorObject>();
        public List<EditorObject> Children { get { return new List<EditorObject>(_children); } }
        public EditorObject Parent { get; private set; }

        public EditorObject(EditorScene editorScene)
        {
            Debug.Assert(editorScene != null);
            EditorScene = editorScene;
            SetParent(EditorScene.Root);
            SetMarker();
        }

        private void SetMarker()
        {
            Marker = new Entity(EditorScene.Scene);
            //Marker.SetParent(this);
            Marker.DrawOverPortals = true;
            Model circle = ModelFactory.CreateCircle(new Vector3(), 0.05f, 10);
            circle.Transform.Position = new Vector3(0, 0, DrawDepth.EntityMarker);
            circle.SetColor(new Vector3(1f, 0.5f, 0f));
            Marker.AddModel(circle);
        }

        public virtual EditorObject Clone(EditorScene scene)
        {
            EditorObject clone = new EditorObject(scene);
            Clone(clone);
            return clone;
        }

        protected virtual void Clone(EditorObject destination)
        {
        }

        [OnDeserialized]
        private void Deserialize(StreamingContext stream)
        {
            SetMarker();
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
                Debug.Assert(parent.EditorScene == EditorScene, "Parent cannot be in a different scene.");
                parent._children.Add(this);
            }
            Debug.Assert(!Tree<EditorObject>.ParentLoopExists(this), "Cannot have cycles in Parent tree.");
        }

        /*protected override void DeepCloneFinalize(Dictionary<SceneNode, SceneNode> cloneMap)
        {
            base.DeepCloneFinalize(cloneMap);
            EditorObject clone = (EditorObject)cloneMap[this];
            clone.Marker = (Entity)cloneMap[Marker];
        }*/

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
