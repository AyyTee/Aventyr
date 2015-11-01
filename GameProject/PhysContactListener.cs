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
using FarseerPhysics.Collision;

namespace Game
{
    public class PhysContactListener
    {
        public Scene Scene { get; private set; }
        private bool _isFirstPreSolve = true;
        private List<Contact> _contactList = new List<Contact>();
        private List<Fixture> _fixtures = new List<Fixture>();

        public PhysContactListener(Scene scene)
        {
            Scene = scene;
            /*Scene.PhysWorld.ContactManager.BeginContact += BeginContactListener;
            Scene.PhysWorld.ContactManager.EndContact += EndContactListener;*/
            Scene.PhysWorld.ContactManager.PreSolve = PreSolveListener;
            Scene.PhysWorld.ContactManager.PostSolve = PostSolveListener;
        }

        public void Step()
        {
            _fixtures.Clear();
            foreach (Body body in Scene.PhysWorld.BodyList)
            {
                foreach (Fixture f in body.FixtureList)
                {
                    FixtureExt.GetUserData(f).PortalCollisions.Clear();
                }
            }
            foreach (Portal p in Scene.PortalList)
            {
                if (p.SensorFixture != null)
                {
                    //EdgeShape edge = (EdgeShape)p.SensorFixture.Shape;
                    Xna.Vector2[] verts = VectorExt2.ConvertToXna(p.GetWorldVerts());
                    Scene.PhysWorld.RayCast(
                        delegate(Fixture fixture, Xna.Vector2 point, Xna.Vector2 normal, float fraction)
                        {
                            //ignore any fixtures that are attached to the same body as the portal fixture
                            if (fixture.Body != p.SensorFixture.Body)
                            {
                                //_fixtures.Add(fixture);
                                FixtureExt.GetUserData(fixture).PortalCollisions.Add(p);
                            }
                            return -1;
                        },
                        verts[0],
                        verts[1]);
                }
            }
        }

        private void PreSolveListener(Contact contact, ref FarseerPhysics.Collision.Manifold oldManifold)
        {
            if (_isFirstPreSolve)
            {
                Console.Out.Write("Begin");
                Console.Out.WriteLine();
                _isFirstPreSolve = false;
            }
            
            FixtureUserData userDataA = FixtureExt.GetUserData(contact.FixtureA);
            FixtureUserData userDataB = FixtureExt.GetUserData(contact.FixtureB);

            if (!IsContactValid(contact))
            {
                contact.Enabled = false;
            }

            /*if (contact.Enabled)
            {
                _contactList.Add(contact);
                if (contact.FixtureA.IsSensor || contact.FixtureB.IsSensor)
                {

                }
                if (userDataA.IsPortalSensor || userDataB.IsPortalSensor)
                {
                    Debug.Assert(userDataA.IsPortalSensor != userDataB.IsPortalSensor);
                    if (userDataA.IsPortalSensor == false)
                    {
                        userDataB.PortalSensorCollisions.Add(contact.FixtureA);
                    }
                    else
                    {
                        userDataA.PortalSensorCollisions.Add(contact.FixtureB);
                    }
                }
            }*/
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

        private void PostSolveListener(Contact contact, ContactConstraint impulse)
        {
            Console.Out.Write("Post");
            Console.Out.WriteLine();
            _isFirstPreSolve = true;
        }

        /// <summary>
        /// Returns true if a contact should not be disabled due to portal clipping.
        /// </summary>
        private bool IsContactValid(Contact contact)
        {
            FixtureUserData userDataA = FixtureExt.GetUserData(contact.FixtureA);
            FixtureUserData userDataB = FixtureExt.GetUserData(contact.FixtureB);

            Xna.Vector2 normal;
            var vList = new FarseerPhysics.Common.FixedArray2<Xna.Vector2>();
            contact.GetWorldManifold(out normal, out vList);
            //contact.Manifold.Points[0] = contact.Manifold.Points[1];
            //contact.Manifold.PointCount = 1;
            //
            //contact.Manifold.
            //contact.Manifold.
            //contact.GetManifold();
            foreach (Portal portal in Scene.PortalList)
            {
                if (userDataA.Portal == portal || userDataB.Portal == portal)
                {
                    continue;
                }
                Line line = new Line(portal.GetWorldVerts());
                float[] vDist = new float[2];
                vDist[0] = line.PointDistance(vList[0], true);
                vDist[1] = line.PointDistance(vList[1], true);
                if (vDist[0] < Portal.PortalMargin * 2)
                {
                    return false;
                }
                else if (vDist[1] < Portal.PortalMargin * 2 && contact.Manifold.PointCount == 2)
                {
                    return false;
                }
            }

            if (vList[0] != new Xna.Vector2(0, 0) || vList[1] != new Xna.Vector2(0, 0))
            {
                Console.Out.Write("Pre");
                Console.Out.WriteLine();
            }
            foreach (Portal portal in userDataA.PortalCollisions)
            {
                if (userDataA.Portal == portal || userDataB.Portal == portal)
                {
                    continue;
                }
                EdgeShape edge = (EdgeShape)portal.SensorFixture.Shape;
                Line portalLine = new Line(portal.GetWorldVerts());
                if (portalLine.GetSideOf(vList[0]) != portalLine.GetSideOf(contact.FixtureA.Body.Position))
                {
                    return false;
                }
            }

            foreach (Portal portal in userDataB.PortalCollisions)
            {
                if (userDataA.Portal == portal || userDataB.Portal == portal)
                {
                    continue;
                }
                EdgeShape edge = (EdgeShape)portal.SensorFixture.Shape;
                Line portalLine = new Line(portal.GetWorldVerts());
                if (portalLine.GetSideOf(vList[0]) != portalLine.GetSideOf(contact.FixtureB.Body.Position))
                {
                    return false;
                }
            }
            return true;
        }

        private bool BeginContactListener(Contact contact)
        {
            FixtureUserData userDataA = FixtureExt.GetUserData(contact.FixtureA);
            FixtureUserData userDataB = FixtureExt.GetUserData(contact.FixtureB);

            if (userDataA.IsPortalSensor)
            {
                userDataB.PortalCollisions.Add(userDataA.Portal);
            }
            else if (userDataB.IsPortalSensor)
            {
                userDataB.PortalCollisions.Add(userDataB.Portal);
            }
            return true;
        }

        private void EndContactListener(Contact contact)
        {
            FixtureUserData userDataA = FixtureExt.GetUserData(contact.FixtureA);
            FixtureUserData userDataB = FixtureExt.GetUserData(contact.FixtureB);
            if (userDataB.IsPortalSensor)
            {
                Debug.Assert(userDataA.PortalCollisions.Remove(userDataB.Portal));
            }
            if (userDataA.IsPortalSensor)
            {
                Debug.Assert(userDataB.PortalCollisions.Remove(userDataA.Portal));
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
