
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

        public Transform2 GetWorldTransform(bool ignorePortals = false)
        {
            Transform2 local = GetTransform();
            if (local == null || IsRoot)
            {
                return local;
            }

            Transform2 parent = Parent.GetWorldTransform(ignorePortals);
            Transform2 t = local.Transform(parent);

            if (!ignorePortals)
            {
                IPortal portalCast = this as IPortal;
                if (portalCast != null)
                {
                    //return portalCast.WorldTransformPrevious;
                    return t.Transform(portalCast.Path.GetPortalTransform());
                }
                else
                {
                    Ray.Settings settings = new Ray.Settings();
                    IPortalable portalable = new Portalable(new Transform2(parent.Position, t.Size, t.Rotation, t.MirrorX), Transform2.CreateVelocity(t.Position - parent.Position));
                    Ray.RayCast(portalable, Scene.GetPortalList(), settings);
                    return portalable.GetTransform();
                }
            }
            return t;
        }

        public virtual Transform2 GetVelocity()
        {
            return Transform2.CreateVelocity();
        }

        /// <summary>
        /// Returns the instantaneous velocity in world coordinates for this SceneNode.  
        /// This takes into account portal teleporation.
        /// </summary>
        public Transform2 GetWorldVelocity(bool ignorePortals = false)
        {
            Transform2 local = GetTransform();
            Transform2 velocity = GetVelocity();
            if (local == null || velocity == null || IsRoot)
            {
                return velocity;
            }

            Transform2 parent = Parent.GetWorldTransform(ignorePortals);
            Transform2 worldTransform = local.Transform(parent);
            Transform2 parentVelocity = Parent.GetWorldVelocity(ignorePortals);

            Vector2 positionDelta = (worldTransform.Position - parent.Position);

            Transform2 worldVelocity = Transform2.CreateVelocity();


            worldVelocity.Size = local.Size * parentVelocity.Size + velocity.Size * parent.Size;

            worldVelocity.Rotation = (parent.MirrorX ? -velocity.Rotation : velocity.Rotation) + parentVelocity.Rotation;

            //Vector2 v = positionDelta.PerpendicularLeft * parentVelocity.Rotation;
            Vector2 v = MathExt.AngularVelocity(positionDelta, parentVelocity.Rotation);
            Vector2 scaleSpeed = positionDelta * parentVelocity.Size / parent.Size;

            Matrix2 mat = Matrix2.CreateScale(parent.Scale) * Matrix2.CreateRotation(parent.Rotation);

            worldVelocity.Position = Vector2Ext.Transform(velocity.Position, mat) + parentVelocity.Position + v + scaleSpeed;


            /*IPortal portalCast = this as IPortal;
            if (portalCast != null)
            {

                for (int i = 0; i < portalCast.Path.Portals.Count; i++)
                {
                    IPortal p = portalCast.Path.Portals[i];
                    worldVelocity = TransformVelocity(, p, worldVelocity, );
                }
                return worldVelocity.Transform(portalCast.Path.GetPortalTransform());
            }
            else*/
            Queue<IPortal> path = null;
            IPortal portalCast = this as IPortal;
            if (portalCast != null)
            {
                path = new Queue<IPortal>(portalCast.Path.Portals);
            }
            /*Skip raycast if this SceneNode is exactly on top of the parent or there is no parent.  
                * This is not just an optimisation step but also necessary to prevent the assert inside 
                * the raycast callback from failing.*/
            if (local.Position == Vector2.Zero || Parent.IsRoot)
            {
                return worldVelocity;
            }
            IPortalable portalable = new Portalable(new Transform2(parent.Position, worldTransform.Size, worldTransform.Rotation, worldTransform.MirrorX), Transform2.CreateVelocity(positionDelta));
            Ray.RayCast(portalable, Scene.GetPortalList(), new Ray.Settings(), (EnterCallbackData data, double movementT) => {
                Debug.Assert(path == null || path.Dequeue() == data.EntrancePortal);
                worldVelocity = TransformVelocity(data.Instance, data.EntrancePortal, worldVelocity, movementT);
            });
            Debug.Assert(path == null || path.Count == 0);

            return worldVelocity;
        }

        private Transform2 TransformVelocity(IPortalable portalable, IPortal portal, Transform2 velocity, double movementT)
        {
            velocity = velocity.ShallowClone();
            velocity = Portal.EnterVelocity(portal, 0.5f, velocity);
            Vector2 endPosition = portalable.GetTransform().Position + portalable.GetVelocity().Position * (float)(1 - movementT);
            float angularVelocity = portal.Linked.GetWorldVelocity().Rotation;
            if (portal.GetWorldTransform().MirrorX != portal.Linked.GetWorldTransform().MirrorX)
            {
                angularVelocity -= portal.GetWorldVelocity().Rotation;
            }
            else
            {
                angularVelocity += portal.GetWorldVelocity().Rotation;
            }
            velocity.Position += MathExt.AngularVelocity(endPosition, portal.Linked.GetWorldTransform().Position, angularVelocity);
            return velocity;
        }

        public SceneNode FindByName(string name)
        {
            return Tree<SceneNode>.FindByType<SceneNode>(this).Find(item => (item.Name == name));
        }
    }
}
