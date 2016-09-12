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
            /*if (onlyNullTransforms)
            {
                set.RemoveWhere(item => item.WorldTransform != null && item.WorldVelocity != null);
            }
            foreach (IPortalCommon p in set)
            {
                if (!onlyVelocity)
                {
                    p.WorldTransform = GetWorldTransformPortal(p);
                }
                p.WorldVelocity = GetWorldVelocityPortal(p);
            }*/

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
            foreach (IPortalCommon p in set)
            {
                /*if (onlyNullTransforms && p.WorldTransform != null && p.WorldVelocity != null)
                {
                    continue;
                }
                if (p.WorldTransform == null)
                {
                    p.WorldTransform = new Transform2();
                }
                if (p.WorldVelocity == null)
                {
                    p.WorldVelocity = Transform2.CreateVelocity();
                }*/
                if (!onlyVelocity)
                {
                    p.WorldTransform = GetWorldTransformPortal(p);
                }
                p.WorldVelocity = GetWorldVelocityPortal(p);
            }
        }

        private static void _resetWorldTransform(IPortalCommon instance)
        {
            instance.Path.Portals.Clear();

            instance.WorldTransform = GetWorldTransformPortal(instance);
            for (int i = 0; i < instance.Children.Count; i++)
            {
                _resetWorldTransform(instance.Children[i]);
            }
        }

        public static bool IsRoot(IPortalCommon instance)
        {
            return instance.Parent == null;
        }

        /*private static Transform2 GetWorldTransform(IPortalCommon instance, bool ignorePortals = false)
        {
            List<IPortal> portals = instance.Scene.GetPortalList();
            if (!ignorePortals)
            {
                IPortal portalCast = instance as IPortal;
                if (portalCast != null)
                {
                    return portalCast.WorldTransformPrevious;
                }
            }

            Transform2 local = instance.GetTransform();
            if (local == null || IsRoot(instance) || IsRoot(instance.Parent))
            {
                return local;
            }

            Transform2 parent = GetWorldTransform(instance.Parent, ignorePortals);
            Transform2 t = local.Transform(parent);

            if (!ignorePortals)
            {
                Ray.Settings settings = new Ray.Settings();
                IPortalable portalable = new Portalable(new Transform2(parent.Position, t.Size, t.Rotation, t.MirrorX), Transform2.CreateVelocity(t.Position - parent.Position));
                Ray.RayCast(portalable, portals, settings);
                return portalable.GetTransform();
            }
            return t;
        }*/

        public static Transform2 GetWorldTransformPortal(IPortalCommon instance, bool ignorePortals = false)
        {
            List<IPortal> portals = instance.Scene.GetPortalList();
            Transform2 local = instance.GetTransform();
            if (local == null || IsRoot(instance) || IsRoot(instance.Parent))
            {
                return local;
            }

            Transform2 parent = instance.Parent.WorldTransform;//GetWorldTransform(instance.Parent, ignorePortals);
            Transform2 t = local.Transform(parent);

            if (!ignorePortals)
            {
                Ray.Settings settings = new Ray.Settings();
                IPortalable portalable = new Portalable(null, new Transform2(parent.Position, t.Size, t.Rotation, t.MirrorX), Transform2.CreateVelocity(t.Position - parent.Position));

                Ray.RayCast(portalable, GetPortalsForPortal(instance, portals), settings);
                return portalable.GetTransform();
            }
            return t;
        }

        /// <summary>
        /// Returns the instantaneous velocity in world coordinates for this SceneNode.  
        /// This takes into account portal teleporation.
        /// </summary>
        /*private static Transform2 GetWorldVelocity(IPortalCommon instance, bool ignorePortals = false)
        {
            List<IPortal> portals = instance.Scene.GetPortalList();
            if (!ignorePortals)
            {
                IPortal portalCast = instance as IPortal;
                if (portalCast != null)
                {
                    return portalCast.WorldVelocityPrevious;
                }
            }

            Transform2 local = instance.GetTransform();
            Transform2 velocity = instance.GetVelocity();
            if (local == null || velocity == null || IsRoot(instance))
            {
                return velocity;
            }

            Transform2 parent = GetWorldTransform(instance.Parent, ignorePortals);
            Transform2 worldTransform = local.Transform(parent);
            Transform2 parentVelocity = GetWorldVelocity(instance.Parent, ignorePortals);

            Vector2 positionDelta = (worldTransform.Position - parent.Position);

            Transform2 worldVelocity = Transform2.CreateVelocity();


            worldVelocity.Size = local.Size * parentVelocity.Size + velocity.Size * parent.Size;

            worldVelocity.Rotation = (parent.MirrorX ? -velocity.Rotation : velocity.Rotation) + parentVelocity.Rotation;

            Vector2 v = MathExt.AngularVelocity(positionDelta, parentVelocity.Rotation);
            Vector2 scaleSpeed = positionDelta * parentVelocity.Size / parent.Size;

            Matrix2 mat = Matrix2.CreateScale(parent.Scale) * Matrix2.CreateRotation(parent.Rotation);

            worldVelocity.Position = Vector2Ext.Transform(velocity.Position, mat) + parentVelocity.Position + v + scaleSpeed;

            IPortalable portalable = new Portalable(new Transform2(parent.Position, worldTransform.Size, worldTransform.Rotation, worldTransform.MirrorX), Transform2.CreateVelocity(positionDelta));
            Ray.RayCast(portalable, portals, new Ray.Settings(), (EnterCallbackData data, double movementT) => {
                worldVelocity = TransformVelocity(data.Instance, data.EntrancePortal, worldVelocity, movementT);
            });

            return worldVelocity;
        }*/

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

        public static Transform2 GetWorldVelocityPortal(IPortalCommon instance, bool ignorePortals = false)
        {
            List<IPortal> portals = instance.Scene.GetPortalList();
            Transform2 local = instance.GetTransform();
            Transform2 velocity = instance.GetVelocity();
            if (local == null || velocity == null || IsRoot(instance) || IsRoot(instance.Parent))
            {
                return velocity;
            }

            Transform2 parent = instance.Parent.WorldTransform;//GetWorldTransform(instance.Parent, ignorePortals);
            Transform2 worldTransform = local.Transform(parent);
            Transform2 parentVelocity = instance.Parent.WorldVelocity;//GetWorldVelocity(instance.Parent, ignorePortals);

            Vector2 positionDelta = (worldTransform.Position - parent.Position);

            Transform2 worldVelocity = Transform2.CreateVelocity();


            worldVelocity.Size = local.Size * parentVelocity.Size + velocity.Size * parent.Size;

            worldVelocity.Rotation = (parent.MirrorX ? -velocity.Rotation : velocity.Rotation) + parentVelocity.Rotation;

            Vector2 v = MathExt.AngularVelocity(positionDelta, parentVelocity.Rotation);
            Vector2 scaleSpeed = positionDelta * parentVelocity.Size / parent.Size;

            Matrix2 mat = Matrix2.CreateScale(parent.Scale) * Matrix2.CreateRotation(parent.Rotation);

            worldVelocity.Position = Vector2Ext.Transform(velocity.Position, mat) + parentVelocity.Position + v + scaleSpeed;

            IPortalable portalable = new Portalable(null, new Transform2(parent.Position, worldTransform.Size, worldTransform.Rotation, worldTransform.MirrorX), Transform2.CreateVelocity(positionDelta));
            Ray.RayCast(portalable, GetPortalsForPortal(instance, portals), new Ray.Settings(), (EnterCallbackData data, double movementT) => {
                worldVelocity = TransformVelocity(data.Instance, data.EntrancePortal, worldVelocity, movementT);
            });

            return worldVelocity;
        }

        private static HashSet<IPortal> GetPortalsForPortal(IPortalCommon instance, IList<IPortal> portals)
        {
            HashSet<IPortal> portalSet = new HashSet<IPortal>(portals);
            portalSet.ExceptWith(Tree<IPortalCommon>.GetDescendents(instance).OfType<IPortal>());
            portalSet.RemoveWhere(item => ReferenceEquals(item, instance.Parent));
            /*portalSet.RemoveWhere(
                item => item.WorldTransform == null || 
                item.Linked?.WorldTransform == null);*/
            return portalSet;
        }
    }
}
