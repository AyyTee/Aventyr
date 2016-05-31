using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using OpenTK;
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
        [XmlIgnore]
        public List<FixturePortal> PortalCollisions = new List<FixturePortal>();
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
        //Portal this fixture belongs to.
        private FixturePortal[] _portalParents = new FixturePortal[2];
        [XmlIgnore]
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
        
        private List<Fixture> _fixtureChildList = new List<Fixture>();
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

        public bool IsPortalChild(FixturePortal portal)
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
            int a = Fixture.Body.FixtureList.Count;
            foreach (Fixture f in _fixtureChildList)
            {
                Fixture.Body.DestroyFixture(f);
            }
            int b = Fixture.Body.FixtureList.Count;
            //FixtureExt.GetUserData(Fixture).Entity.Scene.World.ProcessChanges();
            _fixtureChildList.Clear();
            var sortedPortals = GetChildPortals().ToArray().OrderBy(item => PolygonExt.EdgeIndexT(item.Position)).ToList();
            sortedPortals.RemoveAll(item => !Portal.IsValid(item));
            for (int i = 0; i < sortedPortals.Count(); i++)
            {
                if (i == 0 || (i > 0 && sortedPortals[i].Position.EdgeIndex != sortedPortals[i - 1].Position.EdgeIndex))
                {
                    Fixture fixture = FixtureExt.CreateFixture(Fixture.Body, CreatePortalShape(sortedPortals[i], true));
                    _fixtureChildList.Add(fixture);
                    FixtureExt.GetUserData(fixture).PortalParents = new FixturePortal[] {
                        sortedPortals[i],
                        null
                    };
                }
                if (i < sortedPortals.Count() - 1 && sortedPortals[i].Position.EdgeIndex == sortedPortals[i + 1].Position.EdgeIndex)
                {
                    Fixture fixture = FixtureExt.CreateFixture(Fixture.Body, CreatePortalShape(sortedPortals[i], sortedPortals[i + 1]));
                    _fixtureChildList.Add(fixture);
                    FixtureExt.GetUserData(fixture).PortalParents = new FixturePortal[] {
                        sortedPortals[i],
                        sortedPortals[i+1]
                    };
                }
                else
                {
                    Fixture fixture = FixtureExt.CreateFixture(Fixture.Body, CreatePortalShape(sortedPortals[i], false));
                    _fixtureChildList.Add(fixture);
                    FixtureExt.GetUserData(fixture).PortalParents = new FixturePortal[] {
                        sortedPortals[i],
                        null
                    };
                }
            }
        }

        private PolygonShape CreatePortalShape(FixturePortal portal, FixturePortal portalNext)
        {
            Vector2[] verts = new Vector2[4];
            Line edge = PolygonExt.GetEdge(((IWall)portal.Parent).Vertices, portal.Position);
            //PolygonShape shape = (PolygonShape)portal.Position.Fixture.Shape;

            int i;
            i = 1;
            if (!portal.IsMirrored)
            {
                i = 0;
            }

            verts[0] = Vector2Ext.Transform(Portal.GetVerts(portal)[i], portal.GetTransform().GetMatrix());
            verts[1] = Vector2Ext.Transform(Portal.GetVerts(portal)[i] + new Vector2(-FixturePortal.EdgeMargin, 0), portal.GetTransform().GetMatrix());

            i = 0;
            if (!portalNext.IsMirrored)
            {
                i = 1;
            }

            verts[2] = Vector2Ext.Transform(Portal.GetVerts(portalNext)[i] + new Vector2(-FixturePortal.EdgeMargin, 0), portalNext.GetTransform().GetMatrix());
            verts[3] = Vector2Ext.Transform(Portal.GetVerts(portalNext)[i], portalNext.GetTransform().GetMatrix());
            
            MathExt.SetWinding(verts, false);

            return new PolygonShape(new FarseerPhysics.Common.Vertices(Vector2Ext.ConvertToXna(verts)), 0);
        }

        private PolygonShape CreatePortalShape(FixturePortal portal, bool previousVertex)
        {
            Vector2[] verts = new Vector2[3];
            var tempVerts = Vector2Ext.Transform(Portal.GetVerts(portal), portal.GetTransform().GetMatrix());
            Line edge = PolygonExt.GetEdge(((IWall)portal.Parent).Vertices, portal.Position);
            PolygonShape shape = (PolygonShape)FixtureExt.GetFixturePortalParent(portal).Shape;
            int i = 1;
            if (previousVertex)
            {
                i = 0;
            }

            int iNext = i;
            if (!portal.IsMirrored)
            {
                iNext = (i + 1) % 2;
            }
                
            int index = (portal.Position.EdgeIndex + i) % shape.Vertices.Count;
            verts[0] = tempVerts[iNext];
            verts[1] = Vector2Ext.ConvertTo(shape.Vertices[index]);
            verts[2] = Vector2Ext.Transform(Portal.GetVerts(portal)[iNext] + new Vector2(-FixturePortal.EdgeMargin, 0), portal.GetTransform().GetMatrix());
            MathExt.SetWinding(verts, false);

            /*Entity debugEntity = Entity.Scene.CreateEntity();
            debugEntity.Models.Add(Model.CreatePolygon(verts));*/
            
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
