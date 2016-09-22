using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Portals
{
    public static class PortalCommon
    {
        public static void ResetWorldTransform(IPortalCommon instance)
        {
            /*foreach (IPortalCommon p in Tree<IPortalCommon>.GetDescendents(instance))
            {
                p.WorldTransformPrevious = null;
            }
            _resetWorldTransform(instance);*/
        }

        public static void UpdateWorldTransform(IScene scene, bool onlyNullTransforms = false, bool onlyVelocity = false)
        {
            HashSet<IPortalCommon> set = new HashSet<IPortalCommon>();
            set.UnionWith(scene.GetPortalableList());
            set.UnionWith(scene.GetPortalList());
            UpdateWorldTransform(set, onlyNullTransforms, onlyVelocity);
        }

        public static void UpdateWorldTransform(IEnumerable<IPortalCommon> set, bool onlyNullTransforms = false, bool onlyVelocity = false)
        {
            var nullTransforms = set.Where(item => item.WorldTransform == null);
            foreach (IPortalCommon p in nullTransforms)
            {
                Debug.Assert(p.WorldVelocity == null);
                /*p.WorldTransform = new Transform2();
                p.WorldVelocity = Transform2.CreateVelocity();*/
            }
            if (onlyNullTransforms)
            {
                set = nullTransforms;
            }
            /*Sort the nodes by parent depth in order to make sure we have the WorldTransforms 
             * for parents before child nodes use those transforms.*/
            set = set.OrderBy(item => Tree<IPortalCommon>.Depth(item));

            foreach (IPortalCommon p in set)
            {
                if (!onlyVelocity)
                {
                    p.WorldTransform = GetWorldTransform(p);
                }
                p.WorldVelocity = GetWorldVelocity(p);
            }
        }

        private static void _resetWorldTransform(IPortalCommon instance)
        {
            instance.Path.Portals.Clear();

            instance.WorldTransform = GetWorldTransform(instance);
            for (int i = 0; i < instance.Children.Count; i++)
            {
                _resetWorldTransform(instance.Children[i]);
            }
        }

        public static bool IsRoot(IPortalCommon instance)
        {
            return instance.Parent == null;
        }

        /// <summary>
        /// Calculates the world transform for a given instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static Transform2 GetWorldTransform(IPortalCommon instance)
        {
            List<IPortal> portals = instance.Scene.GetPortalList();
            Transform2 local = instance.GetTransform();
            if (local == null || IsRoot(instance))
            {
                return local;
            }

            Transform2 parent = instance.Parent.WorldTransform;
            Transform2 t = local.Transform(parent);

            Ray.Settings settings = new Ray.Settings();
            IPortalable portalable = new Portalable(null, new Transform2(parent.Position, t.Size, t.Rotation, t.MirrorX), Transform2.CreateVelocity(t.Position - parent.Position));

            Ray.RayCast(portalable, GetPortalsForPortal(instance, portals), settings);
            return portalable.GetTransform();
        }

        public static Transform2 TransformVelocity(IGetTransformVelocity portalable, IPortal portal, Transform2 velocity, double movementT)
        {
            velocity = velocity.ShallowClone();
            velocity = Portal.EnterVelocity(portal, 0.5f, velocity);
            Vector2 endPosition = portalable.GetTransform().Position + portalable.GetVelocity().Position * (float)(1 - movementT);
            float angularVelocity = portal.Linked.WorldVelocity.Rotation;
            if (portal.WorldTransform.MirrorX != portal.Linked.WorldTransform.MirrorX)
            {
                angularVelocity -= portal.WorldVelocity.Rotation;
            }
            else
            {
                angularVelocity += portal.WorldVelocity.Rotation;
            }
            velocity.Position += MathExt.AngularVelocity(endPosition, portal.Linked.WorldTransform.Position, angularVelocity);
            return velocity;
        }

        /// <summary>
        /// Calculates the world velocity for a given instance.
        /// </summary>
        public static Transform2 GetWorldVelocity(IPortalCommon instance)
        {
            
            Transform2 local = instance.GetTransform();
            Transform2 velocity = instance.GetVelocity();
            if (local == null || velocity == null)
            {
                return null;
            }
            else if (IsRoot(instance))
            {
                return velocity;
            }

            Transform2 parentTransform = instance.Parent.WorldTransform;
            Transform2 parentVelocity = instance.Parent.WorldVelocity;
            if (parentTransform == null || parentVelocity == null)
            {
                return null;
            }

            Transform2 worldTransform = local.Transform(parentTransform);

            Transform2 worldVelocity = Transform2.CreateVelocity();
            worldVelocity.Size = local.Size * parentVelocity.Size + velocity.Size * parentTransform.Size;
            worldVelocity.Rotation = (parentTransform.MirrorX ? -velocity.Rotation : velocity.Rotation) + parentVelocity.Rotation;

            Matrix2 mat = Matrix2.CreateScale(parentTransform.Scale) * Matrix2.CreateRotation(parentTransform.Rotation);
            Vector2 positionDelta = (worldTransform.Position - parentTransform.Position);

            worldVelocity.Position = 
                Vector2Ext.Transform(velocity.Position, mat) + 
                parentVelocity.Position + 
                MathExt.AngularVelocity(positionDelta, parentVelocity.Rotation) + 
                positionDelta * parentVelocity.Size / parentTransform.Size;

            IPortalable portalable = new Portalable(null, new Transform2(parentTransform.Position, worldTransform.Size, worldTransform.Rotation, worldTransform.MirrorX), Transform2.CreateVelocity(positionDelta));
            HashSet<IPortal> portals = GetPortalsForPortal(instance, instance.Scene.GetPortalList());
            Ray.RayCast(portalable, portals, new Ray.Settings(), (EnterCallbackData data, double movementT) => {
                worldVelocity = TransformVelocity(data.Instance, data.EntrancePortal, worldVelocity, movementT);
            });

            return worldVelocity;
        }

        private static HashSet<IPortal> GetPortalsForPortal(IPortalCommon instance, IList<IPortal> portals)
        {
            HashSet<IPortal> portalSet = new HashSet<IPortal>(portals);
            portalSet.ExceptWith(Tree<IPortalCommon>.GetDescendents(instance).OfType<IPortal>());
            portalSet.RemoveWhere(item => ReferenceEquals(item, instance.Parent));
            return portalSet;
        }
    }
}
