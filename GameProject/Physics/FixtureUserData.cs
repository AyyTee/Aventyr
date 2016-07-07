using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

namespace Game
{
    public class FixtureUserData
    {
        public readonly Fixture Fixture;
        /// <summary>
        /// All FixturePortals that this fixture is colliding with.
        /// </summary>
        public List<IPortal> PortalCollisions = new List<IPortal>();
        /*public List<FixturePortal> PortalCollisions {
            get
            {
                List<IPortal> portals = ((SceneNode)Entity).Scene.GetPortalList();
                List<FixturePortal> collisions = new List<FixturePortal>();
                foreach (FixturePortal p in portals.OfType<FixturePortal>())
                {
                    Line portalEdge = new Line(Portal.GetWorldVerts(p));
                    if (MathExt.LinePolygonIntersect(portalEdge, Entity.GetWorldVertices()).Count == 0)
                    {
                        collisions.Add(p);
                    }
                }
                return collisions;
            }
        }*/
        private FixturePortal[] _portalParents = new FixturePortal[2];
        /// <summary>
        /// The portals this fixture is a collision edge for (a maximum of 2). 
        /// Both array indices are null if this fixture does not belong to a portal.
        /// </summary>
        public FixturePortal[] PortalParents 
        { 
            get
            {
                return _portalParents;
            }
            private set
            {
                Debug.Assert(GetChildPortals().Count == 0, "This fixture cannot be assigned to a portal.");
                _portalParents = value;
            }
        }
        
        private List<Fixture> _fixtureChildren = new List<Fixture>();
        public List<Fixture> FixtureChildren { get { return new List<Fixture>(_fixtureChildren); } }
        public IActor Actor
        {
            get
            {
                Debug.Assert(Fixture.Body.UserData != null, "Body UserData does not exist.");
                BodyUserData userData = (BodyUserData)BodyExt.GetUserData(Fixture.Body);
                return userData.Actor;
            }
        }

        #region Constructors
        public FixtureUserData()
        {
        }

        public FixtureUserData(Fixture fixture)
        {
            Fixture = fixture;
        }
        #endregion

        public bool PartOfPortal(FixturePortal portal)
        {
            return PortalParents[0] == portal || PortalParents[1] == portal;
        }

        public bool IsPortalParentless()
        {
            return PortalParents[0] == null && PortalParents[1] == null;
        }

        /// <summary>
        /// Updates the Fixtures used for FixturePortal collisions.
        /// </summary>
        public void ProcessChanges()
        {
            foreach (Fixture f in _fixtureChildren)
            {
                Fixture.Body.DestroyFixture(f);
            }
            //FixtureExt.GetUserData(Fixture).Entity.Scene.World.ProcessChanges();
            _fixtureChildren.Clear();
            var sortedPortals = GetChildPortals().ToArray().OrderBy(item => PolygonExt.EdgeIndexT(item.Position)).ToList();
            sortedPortals.RemoveAll(item => !Portal.IsValid(item));
            for (int i = 0; i < sortedPortals.Count(); i++)
            {
                if (i == 0 || (i > 0 && sortedPortals[i].Position.EdgeIndex != sortedPortals[i - 1].Position.EdgeIndex))
                {
                    Fixture fixture = FixtureExt.CreateFixture(Fixture.Body, CreatePortalShape(sortedPortals[i], true));
                    _fixtureChildren.Add(fixture);
                    FixtureExt.GetUserData(fixture).PortalParents = new FixturePortal[] {
                        sortedPortals[i],
                        null
                    };
                }
                if (i < sortedPortals.Count() - 1 && sortedPortals[i].Position.EdgeIndex == sortedPortals[i + 1].Position.EdgeIndex)
                {
                    Fixture fixture = FixtureExt.CreateFixture(Fixture.Body, CreatePortalShape(sortedPortals[i], sortedPortals[i + 1]));
                    _fixtureChildren.Add(fixture);
                    FixtureExt.GetUserData(fixture).PortalParents = new FixturePortal[] {
                        sortedPortals[i],
                        sortedPortals[i+1]
                    };
                }
                else
                {
                    Fixture fixture = FixtureExt.CreateFixture(Fixture.Body, CreatePortalShape(sortedPortals[i], false));
                    _fixtureChildren.Add(fixture);
                    FixtureExt.GetUserData(fixture).PortalParents = new FixturePortal[] {
                        sortedPortals[i],
                        null
                    };
                }
            }
        }

        private PolygonShape CreatePortalShape(FixturePortal portal, FixturePortal portalNext)
        {
            Debug.Assert(portal.Position.EdgeIndex == portalNext.Position.EdgeIndex);
            Debug.Assert(portal.Position.EdgeT < portalNext.Position.EdgeT);
            Vector2[] verts = new Vector2[4];

            {
                Transform2 t0 = portal.GetTransform();
                t0.MirrorX = false;
                t0.Size = Math.Abs(t0.Size);
                verts[0] = Vector2Ext.Transform(Portal.GetVerts(portal)[0], t0.GetMatrix());
                verts[1] = Vector2Ext.Transform(Portal.GetVerts(portal)[0] + new Vector2(-FixturePortal.EdgeMargin, 0), t0.GetMatrix());
            }

            {
                Transform2 t1 = portalNext.GetTransform();
                t1.MirrorX = false;
                t1.Size = Math.Abs(t1.Size);
                verts[2] = Vector2Ext.Transform(Portal.GetVerts(portalNext)[1] + new Vector2(-FixturePortal.EdgeMargin, 0), t1.GetMatrix());
                verts[3] = Vector2Ext.Transform(Portal.GetVerts(portalNext)[1], t1.GetMatrix());
            }
            verts = (Vector2[])MathExt.SetWinding(verts, false);

            return new PolygonShape(new FarseerPhysics.Common.Vertices(Vector2Ext.ConvertToXna(verts)), 0);
        }

        private PolygonShape CreatePortalShape(FixturePortal portal, bool previousVertex)
        {
            Vector2[] verts = new Vector2[3];
            
            PolygonShape shape = (PolygonShape)FixtureExt.GetFixturePortalParent(portal).Shape;
            int i = 1;
            if (previousVertex)
            {
                i = 0;
            }
            int iNext = (i + 1) % 2;

            Transform2 t = portal.GetTransform();
            t.MirrorX = false;
            t.Size = Math.Abs(t.Size);

            int index = (portal.Position.EdgeIndex + i) % Actor.Vertices.Count;
            verts[0] = Vector2Ext.Transform(Portal.GetVerts(portal)[iNext], t.GetMatrix());
            verts[1] = ActorExt.GetFixtureContour(Actor)[index];
            verts[2] = Vector2Ext.Transform(Portal.GetVerts(portal)[iNext] + new Vector2(-FixturePortal.EdgeMargin, 0), t.GetMatrix());
            verts = (Vector2[])MathExt.SetWinding(verts, false);

            return new PolygonShape(new FarseerPhysics.Common.Vertices(Vector2Ext.ConvertToXna(verts)), 0);
        }

        /// <summary>
        /// A list of FixturePortals that are parented to this fixture.
        /// </summary>
        private List<FixturePortal> GetChildPortals()
        {
            List<FixturePortal> portals = ((SceneNode)Actor).Children.OfType<FixturePortal>().ToList();
            return portals.FindAll(item => FixtureExt.GetFixturePortalParent(item) == Fixture);
        }
    }
}
