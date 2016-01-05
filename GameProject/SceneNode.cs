
using FarseerPhysics.Dynamics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Game
{
    public class SceneNode
    {
        public int Id { get; private set; }
        public string Name { get; set; }
        List<SceneNode> _children = new List<SceneNode>();
        public List<SceneNode> ChildList { get { return new List<SceneNode>(_children); } }
        public SceneNode Parent { get; private set; }
        Transform2D _transform = new Transform2D();

        public Scene Scene { get; private set; }

        #region constructors
        public SceneNode(Scene scene)
        {
            Debug.Assert(scene != null, "Must be assigned to a scene.");
            Scene = scene;
            SetParent(Scene.Root);
            Id = Scene.GetId();
        }
        #endregion

        public virtual SceneNode DeepClone()
        {
            return DeepClone(Scene);
        }

        public virtual SceneNode DeepClone(Scene scene)
        {
            SceneNode clone = new SceneNode(scene);
            DeepClone(this, clone);
            return clone;
        }

        protected static void DeepClone(SceneNode source, SceneNode destination)
        {
            destination.Name = source.Name; 
            destination.RemoveChildren();
            foreach (SceneNode p in source.ChildList)
            {
                p.DeepClone().SetParent(destination);
            }
        }

        public virtual void SetParent(SceneNode parent)
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
            Debug.Assert(!ParentLoopExists(), "Cannot have cycles in Parent tree.");
        }

        public void RemoveChildren()
        {
            for (int i = 0; i < ChildList.Count; i++)
            {
                ChildList[i].SetParent(null);
            }
        }

        /// <summary>
        /// Remove from scene.
        /// </summary>
        public virtual void Remove()
        {
            SetParent(null);
            Scene = null;
        }

        public virtual Transform2D GetTransform()
        {
            return _transform.Copy();
        }

        public virtual void SetTransform(Transform2D transform)
        {
            _transform = transform.Copy();
        }

        public virtual void SetPosition(Vector2 position)
        {
            Transform2D transform = GetTransform();
            transform.Position = position;
            SetTransform(transform);
        }

        public virtual void SetRotation(float rotation)
        {
            Transform2D transform = GetTransform();
            transform.Rotation = rotation;
            SetTransform(transform);
        }

        public virtual Transform2D GetWorldTransform()
        {
            if (Parent != null)
            {
                return GetTransform().Transform(Parent.GetWorldTransform());
            }
            return GetTransform();
        }

        /// <summary>
        /// Returns true if there is a loop in the Parent dependencies.
        /// </summary>
        /// <returns></returns>
        private bool ParentLoopExists()
        {
            const int DONT_CARE = 0;
            Dictionary<SceneNode, int> map = new Dictionary<SceneNode, int>();
            SceneNode child = this;
            while (child.Parent != null)
            {
                child = child.Parent;
                if (map.ContainsKey(child))
                {
                    return true;
                }
                map.Add(child, DONT_CARE);
            }
            return false;
        }
    }
}
