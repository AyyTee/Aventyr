using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using System.Collections.Generic;
using System.Diagnostics;

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

        //Portal this fixture belongs to.
        private FixturePortal _portal;
        public FixturePortal Portal 
        { 
            get
            {
                return _portal;
            }
            set
            {
                Debug.Assert(_childPortals.Count == 0, "This fixture cannot be assigned to a portal.");
                _portal = value;
            }
        }
        /// <summary>
        /// A list of FixturePortals that are parented to this fixture.
        /// </summary>
        private List<FixturePortal> _childPortals = new List<FixturePortal>();
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

        public FixtureUserData(Fixture fixture, FixturePortal portal)
        {
            Portal = portal;
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

        public FixtureUserData(Fixture fixture)
            :this(fixture, null)
        {

        }

        public void AddChildPortal(FixturePortal portal)
        {
            Debug.Assert(Portal == null, "Portals cannot be parented to this Fixture.");
            _childPortals.Add(portal);
        }

        public void Update()
        {

        }
    }
}
