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
        /// Ids of all portal sensor fixtures that this fixture is colliding with.
        /// </summary>
        public List<FixturePortal> PortalCollisions = new List<FixturePortal>();

        public FixturePortal Portal { get; set; }
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
    }
}
