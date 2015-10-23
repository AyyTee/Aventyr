using FarseerPhysics.Dynamics;
using System;
using System.Collections.Generic;
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
            return ((List<FixtureUserData>)fixture.UserData)[0];
        }
    }
}
