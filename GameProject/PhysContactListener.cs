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
            
            FixtureUserData userDataA = FixtureExt.GetUserData(contact.FixtureA);
            FixtureUserData userDataB = FixtureExt.GetUserData(contact.FixtureB);

            if (!IsContactValid(contact))
            {
                contact.Enabled = false;
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

        private void PostSolveListener(Contact contact, ContactConstraint impulse)
        {
        }

        /// <summary>
        /// Returns true if a contact should not be disabled due to portal clipping.
        /// </summary>
        private bool IsContactValid(Contact contact)
        {
            FixtureUserData[] userData = new FixtureUserData[2];
            userData[0] = FixtureExt.GetUserData(contact.FixtureA);
            userData[1] = FixtureExt.GetUserData(contact.FixtureB);

            Xna.Vector2 normal;
            var vList = new FarseerPhysics.Common.FixedArray2<Xna.Vector2>();
            contact.GetWorldManifold(out normal, out vList);

            foreach (Portal portal in Scene.PortalList)
            {
                if (userData[0].Portal == portal || userData[1].Portal == portal)
                {
                    continue;
                }
                Line line = new Line(portal.GetWorldVerts());
                float[] vDist = new float[2];
                vDist[0] = line.PointDistance(vList[0], true);
                vDist[1] = line.PointDistance(vList[1], true);
                if (contact.FixtureA == portal.FixtureParent || contact.FixtureB == portal.FixtureParent)
                {
                    if (contact.Manifold.PointCount == 1)
                    {
                        if (vDist[0] < Portal.PortalMargin)
                        {
                            return false;
                        }
                    }
                    else if (vDist[0] < Portal.PortalMargin || vDist[1] < Portal.PortalMargin)
                    {
                        return false;
                    }
                }
            }

            for (int i0 = 0; i0 < userData.Length; i0++)
            {
                int i1 = (i0 + 1) % userData.Length;
                foreach (Portal portal in userData[i0].PortalCollisions)
                {
                    if (userData[i0].Portal == portal || userData[i1].Portal == portal)
                    {
                        continue;
                    }
                    EdgeShape edge = (EdgeShape)portal.SensorFixture.Shape;
                    Line line = new Line(portal.GetWorldVerts());
                    bool sideOf = line.GetSideOf(vList[0]) != line.GetSideOf(userData[i0].Fixture.Body.Position);
                    if (contact.Manifold.PointCount == 1)
                    {
                        if (sideOf)
                        {
                            return false;
                        }
                    }
                    else if (sideOf && line.GetSideOf(vList[1]) != line.GetSideOf(userData[i0].Fixture.Body.Position))
                    {
                        return false;
                    }
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
