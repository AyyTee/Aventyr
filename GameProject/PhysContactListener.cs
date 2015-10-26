using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;

namespace Game
{
    public class PhysContactListener
    {
        public Scene Scene { get; private set; }

        public PhysContactListener(Scene scene)
        {
            Scene = scene;
            Scene.PhysWorld.ContactManager.BeginContact += CollisionListener;
        }

        public bool CollisionListener(Contact target)
        {
            /*FixtureUserData userDataA = (FixtureUserData)target.FixtureA.UserData;
            FixtureUserData userDataB = (FixtureUserData)target.FixtureB.UserData;
            userDataA*/
            foreach (Portal p in Scene.PortalList)
            {
                
            }
            return true;
        }
    }
}
