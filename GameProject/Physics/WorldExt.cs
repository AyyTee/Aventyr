using System.Collections.Generic;
using FarseerPhysics.Dynamics;

namespace Game.Physics
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
