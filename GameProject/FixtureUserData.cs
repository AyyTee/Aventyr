using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class FixtureUserData
    {
        public bool[] EdgeIsExterior;
        private Fixture _fixture;

        public Entity Entity
        {
            get
            {
                Debug.Assert(Fixture.Body.UserData != null, "Body UserData does not exist.");
                BodyUserData userData = (BodyUserData) Fixture.Body.UserData;
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

        private FixtureUserData()
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
                        break;
                    }
            }
        }
    }
}
