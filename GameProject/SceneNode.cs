
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
    /// Scene graph node.  All derived classes MUST override ShallowClone() and return an instance of the derived class.
    /// </summary>
    [DataContract]
    public class SceneNode : ITreeNode<SceneNode>, IDeepClone
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        HashSet<SceneNode> _children = new HashSet<SceneNode>();
        public List<SceneNode> Children { get { return new List<SceneNode>(_children); } }
        [DataMember]
        public SceneNode Parent { get; private set; }

        /*[DataMember]
        public Scene Scene { get; private set; }*/
        [DataMember]
        readonly Scene _scene;
        public Scene Scene
        {
            get { return _scene == null ? Parent.Scene : _scene; }
        }

        #region Constructors
        public SceneNode(Scene scene)
        {
            //Debug.Assert(scene != null);
            //Scene = scene;
            //if (Scene != null && Scene.Root != null)
            if (scene.Root != null)
            {
                Name = "";
                SetParent(scene.Root);
            }
            else
            {
                Name = "Root";
                _scene = scene;
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
            //Remove the child pointer from the root node since the cloned instance is automatically parented to it.
            Scene.Root._children.Remove(destination);
            destination.Parent = Parent;
            destination._children = new HashSet<SceneNode>(Children);
            destination.Name = Name + " Clone";
        }

        public virtual HashSet<IDeepClone> GetCloneableRefs()
        {
            return new HashSet<IDeepClone>(Children);
        }

        public virtual void UpdateRefs(IReadOnlyDictionary<IDeepClone, IDeepClone> cloneMap)
        {
            if (Parent != null)
            {
                if (cloneMap.ContainsKey(Parent))
                {
                    Parent = (SceneNode)cloneMap[Parent];
                    //SetParent((SceneNode)cloneMap[Parent]);
                }
                else
                {
                    //Parent = Scene.Root;
                    SetParent(Parent);
                }
            }
            /*if (Parent != null && cloneMap.ContainsKey(Parent))
            {
                Parent = (SceneNode)cloneMap[Parent];
            }
            else
            {
                Parent = null;
            }*/
            
            List<SceneNode> children = Children;
            _children.Clear();
            foreach (SceneNode e in children)
            {
                //((SceneNode)cloneMap[e]).SetParent(this);
                _children.Add((SceneNode)cloneMap[e]);
            }
        }

        private void RemoveParent()
        {
            if (Parent != null)
            {
                Parent._children.Remove(this);
            }
            Parent = null;
        }

        public virtual void SetParent(SceneNode parent)
        {
            Debug.Assert(parent != null);
            Debug.Assert(_scene == null);
            RemoveParent();
            Parent = parent;
            //if (Parent != null)
            {
                //Scene = null;
                //Debug.Assert(parent.Scene == Scene, "Parent cannot be in a different scene.");
                parent._children.Add(this);
            }
            Debug.Assert(Scene.SceneNodeList.FindAll(item => item == this).Count <= 1);
            Debug.Assert(!Tree<SceneNode>.ParentLoopExists(this), "Cannot have cycles in Parent tree.");
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
            //SetParent(null);
            RemoveParent();
            //Scene = null;
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
