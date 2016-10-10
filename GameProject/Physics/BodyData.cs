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
    public class BodyData : ITreeNode<BodyData>
    {
        public int BodyId;
        public readonly IActor Actor;
        public readonly Body Body;
        public Xna.Vector2 PreviousPosition { get; set; }
        public List<ChildBody> BodyChildren = new List<ChildBody>();
        public ChildBody BodyParent { get; private set; } = new ChildBody(null, null);
        public bool IsChild { get { return BodyParent.Body != null; } }

        public BodyData Parent {
            get
            {
                return BodyParent.Body == null ? null : BodyExt.GetData(BodyParent.Body);
            }
        }
        public List<BodyData> Children {
            get
            {
                return BodyChildren.Select(
                    item => item.Body == null ? null : BodyExt.GetData(item.Body)).ToList();
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
        public BodyData()
        {
        }

        public BodyData(IActor actor, Body body)
        {
            Debug.Assert(body != null);
            Debug.Assert(actor != null);
            Actor = actor;
            BodyId = body.BodyId;
            Body = body;
        }
        #endregion

        /// <summary>
        /// This should only be called by IActor.
        /// </summary>
        public void Update()
        {
            foreach (IPortal portal in PortalCollisionsNew())
            {
                if (BodyParent.Portal?.Linked == portal)
                {
                    continue;
                }

                Body bodyClone = Body.DeepClone();
                bodyClone.BodyType = Actor.BodyType == BodyType.Dynamic ? BodyType.Dynamic : BodyType.Kinematic;
                BodyData userData = BodyExt.SetData(bodyClone, Actor);
                userData.BodyParent = new ChildBody(Body, portal);

                Portal.Enter(portal, bodyClone);
                foreach (Fixture f in bodyClone.FixtureList)
                {
                    FixtureData fixtureData = FixtureExt.SetData(f);
                    fixtureData.PortalCollisions.UnionWith(FixtureExt.GetPortalCollisions(f, Actor.Scene.GetPortalList()));
                    fixtureData.ProcessChanges();
                }

                BodyChildren.Add(new ChildBody(bodyClone, portal));

                Physics.Factory.CreatePortalJoint(((Scene)Actor.Scene).World, Body, bodyClone, portal);
            }

            foreach (IPortal portal in PortalCollisionsRemoved())
            {
                ChildBody child = BodyChildren.Find(item => item.Portal == portal);
                if (child != null)
                {
                    BodyExt.Remove(child.Body);
                }
            }

            foreach (BodyData data in Children)
            {
                data.Update();
            }
        }

        public void SetMass(float mass)
        {
            var nodes = Tree<BodyData>.GetAll(this);
            foreach (BodyData data in nodes)
            {
                data.Body.Mass = Actor.Mass / nodes.Count;
            }
        }

        public HashSet<IPortal> PortalCollisions()
        {
            HashSet<IPortal> collisions = new HashSet<IPortal>();
            foreach (Fixture f in Body.FixtureList)
            {
                collisions.UnionWith(FixtureExt.GetData(f).PortalCollisions);
            }
            Debug.Assert(!collisions.Contains(null));
            return collisions;
        }

        private HashSet<IPortal> PortalCollisionsPrevious()
        {
            HashSet<IPortal> collisionsPrevious = new HashSet<IPortal>();
            foreach (Fixture f in Body.FixtureList)
            {
                collisionsPrevious.UnionWith(FixtureExt.GetData(f).PortalCollisionsPrevious);
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
    }
}
