using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using OpenTK;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game
{
    public class FixtureUserData
    {
        public bool[] EdgeIsExterior;
        private Fixture _fixture;
        /// <summary>
        /// All FixturePortals that this fixture is colliding with.
        /// </summary>
        public List<FixturePortal> PortalCollisions = new List<FixturePortal>();
        public bool Update { get; private set; }
        //Portal this fixture belongs to.
        private FixturePortal[] _portalParents = new FixturePortal[2];
        public FixturePortal[] PortalParents 
        { 
            get
            {
                return _portalParents;
            }
            private set
            {
                Debug.Assert(_childPortals.Count == 0, "This fixture cannot be assigned to a portal.");
                _portalParents = value;
            }
        }
        /// <summary>
        /// A list of FixturePortals that are parented to this fixture.
        /// </summary>
        private List<FixturePortal> _childPortals = new List<FixturePortal>();
        private List<Fixture> _fixtureChildList = new List<Fixture>();
        public Entity Entity
        {
            get
            {
                Debug.Assert(Fixture.Body.UserData != null, "Body UserData does not exist.");
                BodyUserData userData = (BodyUserData)BodyExt.GetUserData(Fixture.Body);
                return userData.LinkedEntity;
            }
        }

        public Fixture Fixture
        {
            get { return _fixture; }
        }

        private int _fixtureId;

        public int FixtureId
        {
            get { return _fixtureId; }
            set { _fixtureId = value; }
        }

        public FixtureUserData()
        {
        }

        public FixtureUserData(Fixture fixture)
        {
            _fixture = fixture;
            _fixtureId = fixture.FixtureId;
            Debug.Assert(Fixture.UserData == null, "UserData has already been assigned for this fixture.");
            Fixture.UserData = this;
            switch (Fixture.ShapeType)
            {
                case ShapeType.Polygon:
                    {
                        PolygonShape shape = (PolygonShape) Fixture.Shape;
                        EdgeIsExterior = new bool[shape.Vertices.Count];
                        for (int i = 0; i < EdgeIsExterior.Length; i++)
                        {
                            EdgeIsExterior[i] = true;
                        }
                        break;
                    }
            }
        }

        public bool IsPortalChild(FixturePortal portal)
        {
            return PortalParents[0] == portal || PortalParents[1] == portal;
        }

        public bool IsPortalParentless()
        {
            return PortalParents[0] == null && PortalParents[1] == null;
        }

        public void AddPortal(FixturePortal portal)
        {
            Debug.Assert(IsPortalParentless(), "Portals cannot be parented to this Fixture.");
            Debug.Assert(!_childPortals.Exists(item => item == portal), "Portal has already been added to this fixture.");
            _childPortals.Add(portal);
            Update = true;
        }

        public void RemovePortal(FixturePortal portal)
        {
            _childPortals.Remove(portal);
            Update = true;
        }

        public void ProcessChanges()
        {
            if (Update == false)
            {
                return;
            }
            Update = false;
            foreach (Fixture f in _fixtureChildList)
            {
                Fixture.Body.DestroyFixture(f);
            }
            _fixtureChildList.Clear();
            var sortedPortals = _childPortals.ToArray().OrderBy(item => item.Position.EdgeIndexT).ToArray();

            for (int i = 0; i < sortedPortals.Count(); i++)
            {
                if (i > 0)
                {
                    if (sortedPortals[i].Position.EdgeIndex != sortedPortals[i - 1].Position.EdgeIndex)
                    {
                        Fixture fixture = FixtureExt.CreateFixture(Fixture.Body, CreatePortalShape(sortedPortals[i], true));
                        _fixtureChildList.Add(fixture);
                        FixtureExt.GetUserData(fixture).PortalParents = new FixturePortal[] {
                            sortedPortals[i],
                            null
                        };
                        //sortedPortals[i].CollisionFixturePrevious = fixture;
                    }
                }
                else
                {
                    Fixture fixture = FixtureExt.CreateFixture(Fixture.Body, CreatePortalShape(sortedPortals[i], true));
                    _fixtureChildList.Add(fixture);
                    FixtureExt.GetUserData(fixture).PortalParents = new FixturePortal[] {
                            sortedPortals[i],
                            null
                        };
                    //sortedPortals[i].CollisionFixturePrevious = fixture;
                }
                if (i < sortedPortals.Count() - 1)
                {
                    if (sortedPortals[i].Position.EdgeIndex != sortedPortals[i + 1].Position.EdgeIndex)
                    {
                        Fixture fixture = FixtureExt.CreateFixture(Fixture.Body, CreatePortalShape(sortedPortals[i], false));
                        _fixtureChildList.Add(fixture);
                        FixtureExt.GetUserData(fixture).PortalParents = new FixturePortal[] {
                            sortedPortals[i],
                            null
                        };
                        //sortedPortals[i].CollisionFixtureNext = fixture;
                    }
                    else
                    {
                        Fixture fixture = FixtureExt.CreateFixture(Fixture.Body, CreatePortalShape(sortedPortals[i], sortedPortals[i + 1]));
                        _fixtureChildList.Add(fixture);
                        FixtureExt.GetUserData(fixture).PortalParents = new FixturePortal[] {
                            sortedPortals[i],
                            sortedPortals[i+1]
                        };
                        //sortedPortals[i].CollisionFixtureNext = fixture;
                        //sortedPortals[i+1].CollisionFixturePrevious = fixture;
                    }
                }
                else
                {
                    Fixture fixture = FixtureExt.CreateFixture(Fixture.Body, CreatePortalShape(sortedPortals[i], false));
                    _fixtureChildList.Add(fixture);
                    FixtureExt.GetUserData(fixture).PortalParents = new FixturePortal[] {
                        sortedPortals[i],
                        null
                    };
                    //sortedPortals[i].CollisionFixtureNext = fixture;
                }
            }
        }

        private PolygonShape CreatePortalShape(FixturePortal portal, FixturePortal portalNext)
        {
            Vector2[] verts = new Vector2[4];
            Line edge = portal.Position.GetEdge();
            PolygonShape shape = (PolygonShape)portal.Position.Fixture.Shape;

            int i;
            i = 1;
            if (!portal.IsMirrored)
            {
                i = 0;
            }

            verts[0] = Vector2Ext.Transform(portal.GetVerts()[i], portal.GetTransform().GetMatrix());
            verts[1] = Vector2Ext.Transform(portal.GetVerts()[i] + new Vector2(-FixturePortal.EdgeMargin, 0), portal.GetTransform().GetMatrix());

            i = 0;
            if (!portalNext.IsMirrored)
            {
                i = 1;
            }

            verts[2] = Vector2Ext.Transform(portalNext.GetVerts()[i] + new Vector2(-FixturePortal.EdgeMargin, 0), portalNext.GetTransform().GetMatrix());
            verts[3] = Vector2Ext.Transform(portalNext.GetVerts()[i], portalNext.GetTransform().GetMatrix());
            
            verts = MathExt.SetHandedness(verts, false);

            /*Entity debugEntity = Entity.Scene.CreateEntity();
            debugEntity.Models.Add(Model.CreatePolygon(verts));*/

            return new PolygonShape(new FarseerPhysics.Common.Vertices(Vector2Ext.ConvertToXna(verts)), 0);
        }

        private PolygonShape CreatePortalShape(FixturePortal portal, bool previousVertex)
        {
            Vector2[] verts = new Vector2[3];
            var tempVerts = Vector2Ext.Transform(portal.GetVerts(), portal.GetTransform().GetMatrix());
            Line edge = portal.Position.GetEdge();
            PolygonShape shape = (PolygonShape)portal.Position.Fixture.Shape;
            int i = 1;
            if (previousVertex)
            {
                i = 0;
            }

            int i0 = i;
            if (!portal.IsMirrored)
            {
                i0 = (i + 1) % 2;
            }
                
            int index = (portal.Position.EdgeIndex + i) % shape.Vertices.Count;
            verts[0] = tempVerts[i0];
            verts[1] = Vector2Ext.ConvertTo(shape.Vertices[index]);
            verts[2] = Vector2Ext.Transform(portal.GetVerts()[i0] + new Vector2(-FixturePortal.EdgeMargin, 0), portal.GetTransform().GetMatrix());
            verts = MathExt.SetHandedness(verts, false);

            /*Entity debugEntity = Entity.Scene.CreateEntity();
            debugEntity.Models.Add(Model.CreatePolygon(verts));*/

            return new PolygonShape(new FarseerPhysics.Common.Vertices(Vector2Ext.ConvertToXna(verts)), 0);
        }
    }
}
