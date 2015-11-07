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
    public static class FixtureExt
    {
        public static void SetUserData(Fixture fixture, FixtureUserData userData)
        {
            //Ugly solution to storing Game classes in a way that still works when deserializing the data.
            //This list is intended to only store one element.
            var a = new List<FixtureUserData>();
            fixture.UserData = a;
            a.Add(userData);
        }

        public static FixtureUserData GetUserData(Fixture fixture)
        {
            Debug.Assert(fixture.UserData != null);
            FixtureUserData userData = ((List<FixtureUserData>)fixture.UserData)[0];
            Debug.Assert(userData != null);
            return userData;
        }

        public static Fixture CreateFixture(Body body, Shape shape)
        {
            Fixture fixture = new Fixture(body, shape);
            SetUserData(fixture, new FixtureUserData(fixture));
            return fixture;
        }

        /*public static Fixture CreatePortalFixture(Body body, Shape shape, FixturePortal portal)
        {
            Fixture fixture = CreateFixture(body, shape);
            GetUserData(fixture).PortalParents = portal;
            return fixture;
        }*/
    }
}
