
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
    public class SceneNode : ITreeNode<SceneNode>, IDeepClone
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        List<SceneNode> _children = new List<SceneNode>();
        public List<SceneNode> Children { get { return new List<SceneNode>(_children); } }
        [DataMember]
        public SceneNode Parent { get; private set; }

        public Scene Scene { get; private set; }

        #region Constructors
        public SceneNode(Scene scene)
        {
            Name = "";
            Debug.Assert(scene != null);
            Scene = scene;
            if (Scene != null)
            {
                SetParent(Scene.Root);
            }
        }
        #endregion

        public virtual IDeepClone ShallowClone()
        {
            SceneNode clone = new SceneNode(Scene);
            ShallowClone(clone);
            return clone;
        }

        protected void ShallowClone(SceneNode destination)
        {
            destination.Parent = Parent;
            destination._children = Children;
            destination.Name = Name + " Clone";
        }

        public virtual List<IDeepClone> GetCloneableRefs()
        {
            return new List<IDeepClone>(Children);
        }

        public virtual void UpdateRefs(IReadOnlyDictionary<IDeepClone, IDeepClone> cloneMap)
        {
            if (Parent != null)
            {
                if (cloneMap.ContainsKey(Parent))
                {
                    Parent = (SceneNode)cloneMap[Parent];
                }
                else
                {
                    Parent = Scene.Root;
                }
            }
            
            List<SceneNode> children = Children;
            _children.Clear();
            foreach (SceneNode e in children)
            {
                _children.Add((SceneNode)cloneMap[e]);
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
            Debug.Assert(!Tree<SceneNode>.ParentLoopExists(this), "Cannot have cycles in Parent tree.");
        }

        public virtual void SetScene(Scene scene)
        {
            if (Parent == Scene.Root)
            {
                Scene = scene;
                SetParent(scene.Root);
            }
            else
            {
                Scene = scene;
            }
        }

        public void RemoveChildren()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].SetParent(Scene.Root);
            }
        }

        /// <summary>Remove from scene.</summary>
        public virtual void Remove()
        {
            Debug.Assert(Parent != null);
            SetParent(null);
            //RemoveChildren();
        }

        public virtual void StepBegin()
        {
        }

        public virtual void StepEnd()
        {
        }

        public virtual Transform2 GetTransform()
        {
            return new Transform2();
        }

        public virtual Transform2 GetWorldTransform()
        {
            if (Parent != null)
            {
                return GetTransform().Transform(Parent.GetWorldTransform());
            }
            return GetTransform();
        }

        public virtual Transform2 GetVelocity()
        {
            return new Transform2();
        }

        public virtual Transform2 GetWorldVelocity()
        {
            if (Parent != null)
            {
                return GetVelocity().Transform(Parent.GetWorldVelocity());
            }
            return GetVelocity();
        }

        public SceneNode FindByName(string name)
        {
            return Tree<SceneNode>.FindByType<SceneNode>(this).Find(item => (item.Name == name));
        }
    }
}
