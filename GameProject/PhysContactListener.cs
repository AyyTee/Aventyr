using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Xna = Microsoft.Xna.Framework;
using System.Diagnostics;
using FarseerPhysics.Collision.Shapes;

namespace Game
{
    public class PhysContactListener
    {
        public Scene Scene { get; private set; }

        public PhysContactListener(Scene scene)
        {
            Scene = scene;
            /*Scene.PhysWorld.ContactManager.BeginContact += BeginContactListener;
            Scene.PhysWorld.ContactManager.EndContact += EndContactListener;*/
            Scene.PhysWorld.ContactManager.PreSolve = PreSolveListener;
        }

        private void PreSolveListener(Contact contact, ref FarseerPhysics.Collision.Manifold oldManifold)
        {
            FixtureUserData userDataA = FixtureExt.GetUserData(contact.FixtureA);
            FixtureUserData userDataB = FixtureExt.GetUserData(contact.FixtureB);

            Xna.Vector2 normal;// = new Xna.Vector2();
            var vList = new FarseerPhysics.Common.FixedArray2<Xna.Vector2>();
            contact.GetWorldManifold(out normal, out vList);

            Fixture[] fixtures = GetContactFixtures(contact);
            for (int i = 0; i < fixtures.Length; i++)
            {

                FixtureUserData userData = FixtureExt.GetUserData(fixtures[i]);
                foreach (Fixture portalFixture in userData.PortalSensorCollisions)
                {
                    if (FixtureExt.GetUserData(portalFixture).Portal == userDataA.Portal)
                    {
                        break;
                    }
                    else if (FixtureExt.GetUserData(portalFixture).Portal == userDataB.Portal)
                    {
                        break;
                    }
                    EdgeShape edge = (EdgeShape)portalFixture.Shape;
                    Line line = new Line(edge.Vertex1, edge.Vertex2);
                    //if (line.GetSideOf(vList[0]) != line.GetSideOf(f.Body.Position))
                    {
                        contact.Enabled = false;
                        break;
                    }
                }
            }
            /*if (vList[0].X == 0 && vList[0].Y == 0)
            {
                contact.Enabled = false;
            }
            else
            {
                Entity entity = Scene.CreateEntity();
                entity.Transform.Scale = new OpenTK.Vector2(0.1f, 0.1f);
                var v2 = vList[0];//(vList[0] + vList[1]) / 2;
                entity.Transform.Position = new OpenTK.Vector2(v2.X, v2.Y);
                entity.Models.Add(Model.CreateCube());
                
            }*/
        }

        private bool BeginContactListener(Contact contact)
        {
            FixtureUserData userDataA = FixtureExt.GetUserData(contact.FixtureA);
            FixtureUserData userDataB = FixtureExt.GetUserData(contact.FixtureB);

            if (userDataA.IsPortalSensor)
            {
                userDataB.PortalSensorCollisions.Add(contact.FixtureA);
            }
            else if (userDataB.IsPortalSensor)
            {
                userDataA.PortalSensorCollisions.Add(contact.FixtureB);
            }
            return true;
        }

        private void EndContactListener(Contact contact)
        {
            FixtureUserData userDataA = FixtureExt.GetUserData(contact.FixtureA);
            FixtureUserData userDataB = FixtureExt.GetUserData(contact.FixtureB);
            if (userDataB.IsPortalSensor)
            {
                Debug.Assert(userDataA.PortalSensorCollisions.Remove(contact.FixtureB));
            }
            if (userDataA.IsPortalSensor)
            {
                Debug.Assert(userDataB.PortalSensorCollisions.Remove(contact.FixtureA));
            }
        }

        private Fixture[] GetContactFixtures(Contact contact)
        {
            Fixture[] fixtures = new Fixture[2];
            fixtures[0] = contact.FixtureA;
            fixtures[1] = contact.FixtureB;
            return fixtures;
        }
    }
}
