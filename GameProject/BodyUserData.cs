using FarseerPhysics.Dynamics;
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
    
    public class BodyUserData
    {
        public long EntityID;
        [XmlIgnore]
        public Entity LinkedEntity { get; private set; }
        public Body Body { get; private set; }
        public Xna.Vector2 PreviousPosition { get; set; }
        public HashSet<FixturePortal> PortalCollisions = new HashSet<FixturePortal>();
        public List<ChildBody> BodyChildren = new List<ChildBody>();
        public ChildBody BodyParent = new ChildBody(null, null);

        public class ChildBody
        {
            public Body Body;
            public Portal Portal;
            public ChildBody(Body body, Portal portal)
            {
                Body = body;
                Portal = portal;
            }
        }

        #region Constructors
        public BodyUserData()
        {
        }

        public BodyUserData(Entity linked, Body body)
        {
            LinkedEntity = linked;
            if (LinkedEntity != null)
            {
                EntityID = linked.Id;
            }
            Body = body;
        }

        public BodyUserData(Body body)
        {
            Body = body;
            LinkedEntity = null;
        }
        #endregion

        public void UpdatePortalCollisions(ref List<Body> bodiesToRemove)
        {
            foreach (ChildBody child in BodyChildren)
            {
                Debug.Assert(child.Body != Body);
            }
            HashSet<FixturePortal> collisionsNew = new HashSet<FixturePortal>();
            foreach (Fixture fixture in Body.FixtureList)
            {
                FixtureUserData userData = FixtureExt.GetUserData(fixture);
                collisionsNew.UnionWith(userData.PortalCollisions);
            }
            var collisionsRemoved = PortalCollisions.Except(collisionsNew).ToList();
            var collisionsAdded = collisionsNew.Except(PortalCollisions).ToList();
            PortalCollisions = collisionsNew;

            foreach (FixturePortal portal in collisionsAdded)
            {
                AddChildBody(portal);   
            }

            foreach (FixturePortal portal in collisionsRemoved)
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
            }
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

        private void AddChildBody(FixturePortal portal)
        {
            BodyUserData userData = BodyExt.GetUserData(Body);
            if (userData.LinkedEntity == null)
            {
                return;
            }
            if (userData.BodyParent.Portal != portal)
            {
                Debug.Assert(!userData.BodyChildren.Exists(item => item.Portal == portal));
                Body bodyClone = Body.DeepClone();
                BodyUserData userDataClone = BodyExt.SetUserData(bodyClone, null);
                userDataClone.BodyParent = new ChildBody(Body, portal.Linked);
                portal.Enter(bodyClone);

                ChildBody childBody = new ChildBody(bodyClone, portal);
                userData.BodyChildren.Add(childBody);

                Debug.Assert(childBody.Body != Body);
            }
        }
    }
}
