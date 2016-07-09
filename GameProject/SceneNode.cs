
using FarseerPhysics.Dynamics;
using Game.Portals;
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
    [DataContract, DebuggerDisplay("SceneNode {Name}")]
    public class SceneNode : ITreeNode<SceneNode>, IDeepClone, ISceneObject
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        HashSet<SceneNode> _children = new HashSet<SceneNode>();
        public List<SceneNode> Children { get { return new List<SceneNode>(_children); } }
        [DataMember]
        public SceneNode Parent { get; private set; }
        public bool IsRoot { get { return _scene != null; } }
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

            parent._children.Add(this);
            Debug.Assert(Scene.SceneNodeList.FindAll(item => item == this).Count <= 1);
            Debug.Assert(!Tree<SceneNode>.ParentLoopExists(this), "Cannot have cycles in Parent tree.");
        }

        public static void SetScene(SceneNode sceneNode, Scene destination)
        {
            Debug.Assert(sceneNode != null);
            Debug.Assert(!sceneNode.IsRoot, "Root nodes cannot be moved.");
            sceneNode.SetParent(destination.Root);
        }

        public static void SetScene(ICollection<SceneNode> sceneNodes, Scene destination)
        {
            foreach (SceneNode s in sceneNodes)
            {
                Debug.Assert(s != null);
                Debug.Assert(!s.IsRoot, "Root nodes cannot be moved.");
                if (!sceneNodes.Contains(s.Parent))
                {
                    s.SetParent(destination.Root);
                }
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
            RemoveParent();
        }

        public virtual Transform2 GetTransform()
        {
            return new Transform2();
        }

        public Transform2 GetWorldTransform()
        {
            Transform2 local = GetTransform();
            if (local == null || Parent.IsRoot)
            {
                return local;
            }

            Transform2 parent = Parent.GetWorldTransform();
            Transform2 t = local.Transform(parent);

            IPortal portalCast = this as IPortal;
            if (portalCast != null)
            {
                t = t.Transform(portalCast.Path.GetPortalTransform());
            }
            else
            {
                Ray.Settings settings = new Ray.Settings();
                settings.IgnorePortalVelocity = true;
                IPortalable portalable = new Portalable(parent, Transform2.CreateVelocity(t.Position - parent.Position));
                Ray.RayCast(portalable, Scene.GetPortalList(), settings);
                t = portalable.GetTransform();
            }
            return t;
        }

        public virtual Transform2 GetVelocity()
        {
            return Transform2.CreateVelocity();
        }

        public Transform2 GetWorldVelocity()
        {
            if (Parent != null)
            {
                Transform2 worldTransform = GetWorldTransform();
                Transform2 parentVelocity = Parent.GetWorldTransform();
                Debug.Assert(parentVelocity.Scale == Vector2.Zero);




                float scalarVelocity = GetVelocity().Size + parentVelocity.Size;
                float angularVelocity = GetVelocity().Rotation + parentVelocity.Rotation;
                //float angularVelocity = Parent.GetVelocity().Rotation * GetTransform().Position.Length;

                Vector2 linearVelocity = parentVelocity.Position;
            }
            return GetVelocity();
        }

        public SceneNode FindByName(string name)
        {
            return Tree<SceneNode>.FindByType<SceneNode>(this).Find(item => (item.Name == name));
        }
    }
}
