using FarseerPhysics.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class WorldExt
    {
        /// <summary>
        /// Returns a list of all fixtures in the physics world.
        /// </summary>
        public static List<Fixture> GetFixtures(World world)
        {
            List<Fixture> fixtures = new List<Fixture>();
            foreach (Body body in world.BodyList)
            {
                fixtures.AddRange(body.FixtureList);
            }
            return fixtures;
        }
    }
}
