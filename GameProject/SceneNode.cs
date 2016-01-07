
using FarseerPhysics.Dynamics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Game
{
    /// <summary>
    /// Scene graph node.  All derived classes MUST override Clone(Scene) and return an instance of the derived class.
    /// </summary>
    public class SceneNode
    {
        /// <summary>Unique identifier within the scene.</summary>
        public readonly int Id;
        public string Name { get; set; }
        List<SceneNode> _children = new List<SceneNode>();
        public List<SceneNode> ChildList { get { return new List<SceneNode>(_children); } }
        public SceneNode Parent { get; private set; }

        public readonly Scene Scene;

        #region constructors
        public SceneNode(Scene scene)
        {
            Debug.Assert(scene != null, "Must be assigned to a scene.");
            Scene = scene;
            Id = Scene.GetId();
            SetParent(Scene.Root);
        }
        #endregion

        /// <summary>
        /// Clones a SceneNode and recursively clones all of it's children.
        /// </summary>
        public SceneNode DeepClone()
        {
            return DeepClone(Scene);
        }

        /// <summary>
        /// Clones a SceneNode and recursively clones all of it's children.
        /// </summary>
        /// <param name="scene">Scene to clone into.</param>
        /// <param name="mask"></param>
        public SceneNode DeepClone(Scene scene, HashSet<SceneNode> mask = null)
        {
            Dictionary<SceneNode, SceneNode> cloneMap = new Dictionary<SceneNode, SceneNode>();
            SceneNode clone = Clone(scene);
            cloneMap.Add(this, clone);
            List<SceneNode> cloneList = new List<SceneNode>();
            CloneChildren(cloneMap, cloneList, mask);
            foreach (SceneNode s in cloneList)
            {
                DeepCloneFinalize(cloneMap);
            }
            return clone;
        }

        /// <summary>
        /// This method is called by DeepClone after all SceneNodes have been cloned and parented. Useful for updating references.
        /// </summary>
        /// <param name="cloneMap">Map of source SceneNodes to destination SceneNodes.</param>
        protected virtual void DeepCloneFinalize(Dictionary<SceneNode, SceneNode>  cloneMap)
        {
        }

        private void CloneChildren(Dictionary<SceneNode, SceneNode> cloneMap, List<SceneNode> cloneList, HashSet<SceneNode> mask)
        {
            foreach (SceneNode p in ChildList)
            {
                if (mask == null || mask.Contains(p))
                {
                    SceneNode clone = p.Clone();
                    cloneMap.Add(p, clone);
                    clone.SetParent(cloneMap[p.Parent]);
                    cloneList.Add(p);
                }
                p.CloneChildren(cloneMap, cloneList, mask);
            }
        }

        public SceneNode Clone()
        {
            return Clone(Scene);
        }

        public virtual SceneNode Clone(Scene scene)
        {
            SceneNode clone = new SceneNode(scene);
            Clone(clone);
            return clone;
        }

        protected virtual void Clone(SceneNode destination)
        {
            destination.Name = Name;
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
                ChildList[i].SetParent(Scene.Root);
            }
        }

        /// <summary>
        /// Remove from scene graph.
        /// </summary>
        public virtual void Remove()
        {
            SetParent(null);
            //RemoveChildren();
        }

        public virtual Transform2D GetTransform()
        {
            return new Transform2D();
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
