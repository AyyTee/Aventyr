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
        public static bool IsRoot(IPortalCommon instance)
        {
            return instance.Parent == null;
        }

        private static Transform2 GetWorldTransform(IPortalCommon instance, IList<IPortal> portals, bool ignorePortals = false)
        {
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

            Transform2 parent = GetWorldTransform(instance.Parent, portals, ignorePortals);
            Transform2 t = local.Transform(parent);

            if (!ignorePortals)
            {
                Ray.Settings settings = new Ray.Settings();
                IPortalable portalable = new Portalable(new Transform2(parent.Position, t.Size, t.Rotation, t.MirrorX), Transform2.CreateVelocity(t.Position - parent.Position));
                Ray.RayCast(portalable, portals, settings);
                return portalable.GetTransform();
            }
            return t;
        }

        public static Transform2 GetWorldTransformPortal(IPortalCommon instance, IList<IPortal> portals, bool ignorePortals = false)
        {
            Debug.Assert(instance is IPortal);
            Transform2 local = instance.GetTransform();
            if (local == null || IsRoot(instance) || IsRoot(instance.Parent))
            {
                return local;
            }

            Transform2 parent = instance.Parent.GetWorldTransform(ignorePortals);
            Transform2 t = local.Transform(parent);

            if (!ignorePortals)
            {
                Ray.Settings settings = new Ray.Settings();
                IPortalable portalable = new Portalable(new Transform2(parent.Position, t.Size, t.Rotation, t.MirrorX), Transform2.CreateVelocity(t.Position - parent.Position));

                Ray.RayCast(portalable, GetPortalsForPortal(instance, portals), settings);
                return portalable.GetTransform();
            }
            return t;
        }

        /// <summary>
        /// Returns the instantaneous velocity in world coordinates for this SceneNode.  
        /// This takes into account portal teleporation.
        /// </summary>
        private static Transform2 GetWorldVelocity(IPortalCommon instance, IList<IPortal> portals, bool ignorePortals = false)
        {
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

            Transform2 parent = instance.Parent.GetWorldTransform(ignorePortals);
            Transform2 worldTransform = local.Transform(parent);
            Transform2 parentVelocity = instance.Parent.GetWorldVelocity(ignorePortals);

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
        }

        public static Transform2 TransformVelocity(IGetTransformVelocity portalable, IPortal portal, Transform2 velocity, double movementT)
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

        public static Transform2 GetWorldVelocityPortal(IPortalCommon instance, IList<IPortal> portals, bool ignorePortals = false)
        {
            Transform2 local = instance.GetTransform();
            Transform2 velocity = instance.GetVelocity();
            if (local == null || velocity == null || IsRoot(instance))
            {
                return velocity;
            }

            Transform2 parent = GetWorldTransform(instance.Parent, portals, ignorePortals);
            Transform2 worldTransform = local.Transform(parent);
            Transform2 parentVelocity = GetWorldVelocity(instance.Parent, portals, ignorePortals);

            Vector2 positionDelta = (worldTransform.Position - parent.Position);

            Transform2 worldVelocity = Transform2.CreateVelocity();


            worldVelocity.Size = local.Size * parentVelocity.Size + velocity.Size * parent.Size;

            worldVelocity.Rotation = (parent.MirrorX ? -velocity.Rotation : velocity.Rotation) + parentVelocity.Rotation;

            Vector2 v = MathExt.AngularVelocity(positionDelta, parentVelocity.Rotation);
            Vector2 scaleSpeed = positionDelta * parentVelocity.Size / parent.Size;

            Matrix2 mat = Matrix2.CreateScale(parent.Scale) * Matrix2.CreateRotation(parent.Rotation);

            worldVelocity.Position = Vector2Ext.Transform(velocity.Position, mat) + parentVelocity.Position + v + scaleSpeed;

            IPortalable portalable = new Portalable(new Transform2(parent.Position, worldTransform.Size, worldTransform.Rotation, worldTransform.MirrorX), Transform2.CreateVelocity(positionDelta));
            Ray.RayCast(portalable, GetPortalsForPortal(instance, portals), new Ray.Settings(), (EnterCallbackData data, double movementT) => {
                worldVelocity = TransformVelocity(data.Instance, data.EntrancePortal, worldVelocity, movementT);
            });

            return worldVelocity;
        }

        private static HashSet<IPortal> GetPortalsForPortal(IPortalCommon instance, IList<IPortal> portals)
        {
            HashSet<IPortal> portalList = new HashSet<IPortal>(portals);
            portalList.ExceptWith(Tree<IPortalCommon>.GetDescendents(instance).OfType<IPortal>());
            portalList.RemoveWhere(item => ReferenceEquals(item, instance.Parent));
            return portalList;
        }
    }
}
