using FarseerPhysics.Dynamics;
using Game.Portals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xna = Microsoft.Xna.Framework;

namespace Game
{
    public class BodyUserData : ITreeNode<BodyUserData>
    {
        public int BodyId;
        public readonly IActor Actor;
        public readonly Body Body;
        public Xna.Vector2 PreviousPosition { get; set; }
        public List<ChildBody> BodyChildren = new List<ChildBody>();
        public ChildBody BodyParent { get; private set; } = new ChildBody(null, null);
        public bool IsChild { get { return BodyParent.Body != null; } }

        public BodyUserData Parent {
            get
            {
                return BodyParent.Body == null ? null : BodyExt.GetUserData(BodyParent.Body);
            }
        }
        public List<BodyUserData> Children {
            get
            {
                return BodyChildren.Select(
                    item => item.Body == null ? null : BodyExt.GetUserData(item.Body)).ToList();
            }
        }

        public class ChildBody
        {
            public readonly Body Body;
            public readonly IPortal Portal;
            public ChildBody(Body body, IPortal portal)
            {
                Body = body;
                Portal = portal;
            }
        }

        #region Constructors
        public BodyUserData()
        {
        }

        public BodyUserData(IActor actor, Body body)
        {
            Debug.Assert(body != null);
            Debug.Assert(actor != null);
            Actor = actor;
            BodyId = body.BodyId;
            Body = body;
        }
        #endregion

        public void UpdatePortalCollisions()
        {
            foreach (ChildBody child in BodyChildren)
            {
                Debug.Assert(child.Body != Body);
                child.Body.LinearVelocity = Body.LinearVelocity;
                child.Body.AngularVelocity = Body.AngularVelocity;
                child.Body.Position = Body.Position;
                child.Body.Rotation = Body.Rotation;
                Portal.Enter(child.Portal, child.Body);
            }

            foreach (IPortal portal in PortalCollisionsNew())
            {
                if (BodyParent.Portal?.Linked == portal)
                {
                    continue;
                }

                Body bodyClone = Body.DeepClone();
                BodyUserData userData = BodyExt.SetUserData(bodyClone, Actor);
                userData.BodyParent = new ChildBody(Body, portal);
                foreach (Fixture f in bodyClone.FixtureList)
                {
                    FixtureExt.SetUserData(f);
                }

                Portal.Enter(portal, bodyClone);

                BodyChildren.Add(new ChildBody(bodyClone, portal));

                Physics.Factory.CreatePortalJoint(((Scene)Actor.Scene).World, Body, bodyClone, portal);

                userData.UpdatePortalCollisions();
            }

            /*foreach (FixturePortal portal in PortalCollisionsRemoved())
            {
                if (BodyParent != null && portal == BodyParent.Portal)
                {
                    continue;
                }
                ChildBody childBody = BodyChildren.Find(item => item.Portal == portal);
                Debug.Assert(childBody != null);
                Debug.Assert(childBody.Body != Body);

                RemoveChildBody(childBody, ref bodiesToRemove);
                BodyChildren.Remove(childBody);
            }*/
        }

        public HashSet<IPortal> PortalCollisions()
        {
            HashSet<IPortal> collisions = new HashSet<IPortal>();
            foreach (Fixture f in Body.FixtureList)
            {
                collisions.UnionWith(FixtureExt.GetUserData(f).PortalCollisions);
            }
            Debug.Assert(!collisions.Contains(null));
            return collisions;
        }

        private HashSet<IPortal> PortalCollisionsPrevious()
        {
            HashSet<IPortal> collisionsPrevious = new HashSet<IPortal>();
            foreach (Fixture f in Body.FixtureList)
            {
                collisionsPrevious.UnionWith(FixtureExt.GetUserData(f).PortalCollisionsPrevious);
            }
            Debug.Assert(!collisionsPrevious.Contains(null));
            return collisionsPrevious;
        }

        private HashSet<IPortal> PortalCollisionsNew()
        {
            return new HashSet<IPortal>(PortalCollisions().Except(PortalCollisionsPrevious()));
        }

        private HashSet<IPortal> PortalCollisionsRemoved()
        {
            return new HashSet<IPortal>(PortalCollisionsPrevious().Except(PortalCollisions()));
        }

        private void RemoveChildBody(ChildBody child, ref List<Body> bodiesToRemove)
        {
            BodyUserData userData = BodyExt.GetUserData(child.Body);
            foreach (ChildBody subchild in userData.BodyChildren)
            {
                Debug.Assert(subchild != child);
                Debug.Assert(subchild.Body != child.Body);
                userData.RemoveChildBody(subchild, ref bodiesToRemove);
            }
            bodiesToRemove.Add(child.Body);
        }
    }
}
