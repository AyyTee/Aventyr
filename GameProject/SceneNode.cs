
using FarseerPhysics.Dynamics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml;

namespace Game
{
    /// <summary>
    /// Scene graph node.  All derived classes MUST override Clone(Scene) and return an instance of the derived class.
    /// </summary>
    [DataContract]
    public class SceneNode
    {
        /// <summary>Unique identifier within the scene.</summary>
        [DataMember]
        public long Id { get; private set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        List<SceneNode> _children = new List<SceneNode>();
        public List<SceneNode> ChildList { get { return new List<SceneNode>(_children); } }
        [DataMember]
        public SceneNode Parent { get; private set; }

        public Scene Scene { get; private set; }

        #region Constructors
        public SceneNode(Scene scene)
        {
            Debug.Assert(scene != null, "Must be assigned to a scene.");
            Scene = scene;
            if (Scene != null)
            {
                Id = Scene.GetId();
                SetParent(Scene.Root);
            }
        }
        #endregion

        /// <summary>
        /// Assigns SceneNode and all it's descendents to a Scene.  
        /// Cannot be done if SceneNode is already part of a Scene.  Method must be called by the root node.
        /// </summary>
        public void SetScene(Scene scene)
        {
            Debug.Assert(Parent == null, "Must not have a parent.");
            _setScene(scene);
            SetParent(Scene.Root);
        }

        private void _setScene(Scene scene)
        {
            Debug.Assert(Scene == null, "SceneNode is already a part of a Scene.");
            Scene = scene;
            foreach (SceneNode s in ChildList)
            {
                s._setScene(Scene);
            }
        }

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
            List<SceneNode> cloneList = new List<SceneNode>();
            SceneNode clone = Clone(scene);
            clone.SetParent(null);
            cloneMap.Add(this, clone);
            cloneList.Add(this);
            CloneChildren(scene, cloneMap, cloneList, mask);
            foreach (SceneNode s in cloneList)
            {
                s.DeepCloneFinalize(cloneMap);
            }
            clone.SetParent(scene.Root);
            return clone;
        }

        /// <summary>
        /// This method is called by DeepClone after all SceneNodes have been cloned and parented. Useful for updating references.
        /// </summary>
        /// <param name="cloneMap">Map of source SceneNodes to destination SceneNodes.</param>
        protected virtual void DeepCloneFinalize(Dictionary<SceneNode, SceneNode> cloneMap)
        {
        }

        private void CloneChildren(Scene scene, Dictionary<SceneNode, SceneNode> cloneMap, List<SceneNode> cloneList, HashSet<SceneNode> mask)
        {
            foreach (SceneNode p in ChildList)
            {
                if (mask == null || mask.Contains(p))
                {
                    SceneNode clone = p.Clone(scene);
                    cloneMap.Add(p, clone);
                    clone.SetParent(cloneMap[p.Parent]);
                    cloneList.Add(p);
                }
                p.CloneChildren(scene, cloneMap, cloneList, mask);
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
            HashSet<SceneNode> map = new HashSet<SceneNode>();
            SceneNode child = this;
            while (child.Parent != null)
            {
                child = child.Parent;
                if (!map.Add(child))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
