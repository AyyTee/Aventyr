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

        public Fixture Fixture
        {
            get { return _fixture; }
        }

        public FixtureUserData(Fixture fixture)
        {
            _fixture = fixture;
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
