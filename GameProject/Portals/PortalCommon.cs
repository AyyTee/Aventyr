using OpenTK;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Game.Common;
using Game.Rendering;

namespace Game.Portals
{
    public static class PortalCommon
    {
        public static void UpdateWorldTransform(IScene scene, bool onlyNullTransforms = false, bool onlyVelocity = false)
        {
            HashSet<IPortalCommon> set = new HashSet<IPortalCommon>();
            set.UnionWith(scene.GetPortalableList());
            set.UnionWith(scene.GetPortalList());
            UpdateWorldTransform(set, onlyNullTransforms, onlyVelocity);
        }

        public static void UpdateWorldTransform(IEnumerable<IPortalCommon> set, bool onlyNullTransforms = false, bool onlyVelocity = false)
        {
            Debug.Assert(set.FirstOrDefault(item => item.WorldTransform == null && item.WorldVelocity != null) == null, 
                "IPortalCommon instance cannot have a world transform but no world velocity.");

            var newSet = onlyNullTransforms ? set.Where(item => item.WorldTransform == null) : set;

            /*Sort the nodes by parent depth in order to make sure we have the WorldTransforms 
             * for parents before child nodes use those transforms.*/
            foreach (IPortalCommon p in newSet.OrderBy(Tree<IPortalCommon>.Depth))
            {
                if (!onlyVelocity)
                {
                    p.WorldTransform = GetWorldTransform(p);
                }
                p.WorldVelocity = GetWorldVelocity(p);
            }
        }

        static void _resetWorldTransform(IPortalCommon instance)
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
        /// Is true if the IPortalCommon is located somewhere (as oppsed to having a null position).
        /// </summary>
        static bool IsPlaced(IPortalCommon instance)
        {
            return Tree<IPortalCommon>.GetAncestors(instance, true)
                .Any(item => item.GetTransform() == null);
        }

        /// <summary>
        /// Calculates the world transform for a given instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static Transform2 GetWorldTransform(IPortalCommon instance)
        {
            if (IsPlaced(instance))
            {
                return null;
            }

            List<IPortal> portals = instance.Scene.GetPortalList();
            Transform2 local = instance.GetTransform();
            if (IsRoot(instance))
            {
                return local;
            }

            Transform2 parent = instance.Parent.WorldTransform;
            Transform2 t = local.Transform(parent);

            var result = Ray.RayCast(
                new Transform2(parent.Position, t.Rotation, t.Size, t.MirrorX), 
                Transform2.CreateVelocity(t.Position - parent.Position), 
                GetPortalsForPortal(instance, portals), 
                new Ray.Settings());
            return result.WorldTransform;
        }

        public static Transform2 GetWorldTransformUsingPath(IPortalCommon instance)
        {
            if (IsPlaced(instance))
            {
                return null;
            }

            Transform2 local = instance.GetTransform();
            if (IsRoot(instance))
            {
                return local;
            }

            Transform2 parent = instance.Parent.WorldTransform;
            Transform2 t = local.Transform(parent);

            return t.Transform(instance.Path.GetPortalTransform());
        }

        public static Transform2 TransformVelocity(Transform2 portalableTransform, Transform2 portalableVelocity, IPortalRenderable portal, Transform2 velocity, double movementT)
        {
            var newVelocity = Portal.EnterVelocity(portal, 0.5f, velocity);
            Vector2 endPosition = portalableTransform.Position + portalableVelocity.Position * (float)(1 - movementT);
            float angularVelocity = portal.Linked.WorldVelocity.Rotation;
            if (portal.WorldTransform.MirrorX != portal.Linked.WorldTransform.MirrorX)
            {
                angularVelocity -= portal.WorldVelocity.Rotation;
            }
            else
            {
                angularVelocity += portal.WorldVelocity.Rotation;
            }
            return newVelocity.SetPosition(newVelocity.Position + MathEx.AngularVelocity(endPosition, portal.Linked.WorldTransform.Position, angularVelocity));
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
            if (IsRoot(instance))
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

            var rotation = parentTransform.MirrorX ? 
                -velocity.Rotation : 
                velocity.Rotation;
            rotation += parentVelocity.Rotation;

            Transform2 worldVelocity = Transform2.CreateVelocity()
                .SetSize(local.Size * parentVelocity.Size + velocity.Size * parentTransform.Size)
                .SetRotation(rotation);

            Matrix2 mat = Matrix2.CreateScale(parentTransform.Scale) * Matrix2.CreateRotation(parentTransform.Rotation);
            Vector2 positionDelta = (worldTransform.Position - parentTransform.Position);

            worldVelocity = worldVelocity.SetPosition(
                Vector2Ex.Transform(velocity.Position, mat) + 
                parentVelocity.Position + 
                MathEx.AngularVelocity(positionDelta, parentVelocity.Rotation) + 
                positionDelta * parentVelocity.Size / parentTransform.Size);

            HashSet<IPortal> portals = GetPortalsForPortal(instance, instance.Scene.GetPortalList());
            var result = Ray.RayCast(
                new Transform2(parentTransform.Position, worldTransform.Rotation, worldTransform.Size, worldTransform.MirrorX), 
                Transform2.CreateVelocity(positionDelta), 
                portals, 
                new Ray.Settings());

            foreach (var callback in result.PortalsEntered)
            {
                worldVelocity = TransformVelocity(
                    callback.EnterData.GetTransform(),
                    callback.EnterData.GetVelocity(),
                    callback.EnterData.EntrancePortal, 
                    worldVelocity, 
                    callback.MovementT);
            }

            return worldVelocity;
        }

        static HashSet<IPortal> GetPortalsForPortal(IPortalCommon instance, IEnumerable<IPortal> portals)
        {
            var portalSet = new HashSet<IPortal>(portals);
            portalSet.ExceptWith(Tree<IPortalCommon>.GetDescendents(instance).OfType<IPortal>());
            portalSet.RemoveWhere(item => ReferenceEquals(item, instance.Parent));
            return portalSet;
        }

        public static void SetLocalTransform(IPortalable portalable, Transform2 transform)
        {
            portalable.Transform = transform;
            ClearWorld(portalable);
        }

        public static void SetLocalTransform(FixturePortal fixturePortal, WallCoord coord)
        {
            fixturePortal.SetPosition(coord);
            ClearWorld(fixturePortal);
        }

        public static void SetLocalVelocity(IPortalable portalable, Transform2 velocity)
        {
            portalable.Velocity = velocity;
            ClearWorld(portalable, true);
        }

        static void ClearWorld(IPortalCommon portalCommon, bool onlyVelocity = false)
        {
            foreach (var child in Tree<IPortalCommon>.GetDescendents(portalCommon))
            {
                child.WorldVelocity = null;
                if (!onlyVelocity)
                {
                    child.WorldTransform = null;
                    child.Path.Portals.Clear();
                }
            }
        }
    }
}
