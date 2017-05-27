using Game.Portals;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Game.Common;
using Game.Serialization;

namespace Game
{
    /// <summary>
    /// Scene graph node.  All derived classes MUST override ShallowClone() and return an instance of the derived class.
    /// </summary>
    [DataContract, DebuggerDisplay(nameof(SceneNode) + " {" + nameof(Name) + "}")]
    public class SceneNode : ITreeNode<SceneNode>, IDeepClone, ISceneObject, IPortalCommon
    {
        [DataMember]
        public PortalPath Path { get; set; } = new PortalPath();
        [DataMember]
        public Transform2 WorldTransform { get; set; }
        [DataMember]
        public Transform2 WorldVelocity { get; set; }
        IPortalCommon ITreeNode<IPortalCommon>.Parent => Parent;
        List<IPortalCommon> ITreeNode<IPortalCommon>.Children => Children.ToList<IPortalCommon>();

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        HashSet<SceneNode> _children = new HashSet<SceneNode>();
        public List<SceneNode> Children => new List<SceneNode>(_children);

        [DataMember]
        public SceneNode Parent { get; private set; }
        [DataMember]
        public Scene Scene { get; private set; }
        IScene IPortalCommon.Scene => Scene;
        public virtual bool IsPortalable { get; set; } = true;

        public SceneNode(Scene scene)
        {
            Scene = scene;
            Scene.Add(this);
            Name = GetType().Name;
        }

        public virtual IDeepClone ShallowClone()
        {
            SceneNode clone = new SceneNode(Scene);
            ShallowClone(clone);
            return clone;
        }

        protected void ShallowClone(SceneNode destination)
        {
            //Remove the child pointer from the root node since the cloned instance is automatically parented to it.
            //Scene.Root._children.Remove(destination);
            destination.Parent = Parent;
            destination._children = new HashSet<SceneNode>(Children);
            destination.Name = Name + " Clone";
            destination.IsPortalable = IsPortalable;
        }

        public virtual HashSet<IDeepClone> GetCloneableRefs() => new HashSet<IDeepClone>(Children);

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
                    SetParent(Parent);
                }
            }

            List<SceneNode> children = Children;
            _children.Clear();
            foreach (SceneNode e in children)
            {
                _children.Add((SceneNode)cloneMap[e]);
            }
        }

        void RemoveFromParent()
        {
            Parent?._children.Remove(this);
            Parent = null;
        }

        public virtual void SetParent(SceneNode parent)
        {
            //Debug.Assert(Scene == parent.Scene);
            /*if (parent != null && parent.Scene != Scene)
            {
                Scene.SceneNodes.Remove(this);
                Scene = parent.Scene;
                Scene.SceneNodes.Add(this);
            }*/

            RemoveFromParent();
            Parent = parent;

            parent?._children.Add(this);

            Debug.Assert(Scene.SceneObjects.FindAll(item => item == this).Count <= 1);
            Debug.Assert(!Tree<SceneNode>.ParentLoopExists(this), "Cannot have cycles in Parent tree.");
        }

        public void RemoveChildren()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].SetParent(null);
            }
        }

        /// <summary>Remove from scene.</summary>
        public virtual void Remove()
        {
            Debug.Assert(!Scene.InStep, "Cannot be removed during Scene step.");
            RemoveFromParent();
            foreach (SceneNode child in Children.ToList())
            {
                child.Remove();
            }
            Scene.SceneObjects.Remove(this);
            Scene = null;
        }

        public virtual Transform2 GetTransform() => new Transform2();

        /// <summary>
        /// Set transform and update children.  This method is expected to only be called by classes extending SceneNode.
        /// </summary>
        /// <param name="transform"></param>
        public virtual void SetTransform(Transform2 transform)
        {
        }

        public virtual void SetVelocity(Transform2 transform)
        {
        }

        public virtual Transform2 GetVelocity() => Transform2.CreateVelocity();
    }
}
