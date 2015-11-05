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
using OpenTK;
using FarseerPhysics.Common;

namespace Game
{
    public class PhysContactListener
    {
        public Scene Scene { get; private set; }
        Entity DebugEntity;
        bool DebugMode = true;

        public PhysContactListener(Scene scene)
        {
            Scene = scene;
            Scene.PhysWorld.ContactManager.PreSolve = PreSolveListener;
        }

        public void StepBegin()
        {
            foreach (Body body in Scene.PhysWorld.BodyList)
            {
                foreach (Fixture f in body.FixtureList)
                {
                    FixtureExt.GetUserData(f).PortalCollisions.Clear();
                }
                BodyExt.GetUserData(body).PreviousPosition = body.Position;
            }
            foreach (Portal p in Scene.PortalList)
            {
                if (p.GetType() == typeof(FixturePortal))
                {
                    FixturePortal portal = (FixturePortal)p;
                    if (portal.Position != null)
                    {
                        Xna.Vector2[] verts = Vector2Ext.ConvertToXna(p.GetWorldVerts());
                        Scene.PhysWorld.RayCast(
                            delegate(Fixture fixture, Xna.Vector2 point, Xna.Vector2 normal, float fraction)
                            {
                                //ignore any fixtures that are attached to the same body as the portal fixture
                                if (fixture.Body != portal.FixtureParent.Body)
                                {
                                    FixtureExt.GetUserData(fixture).PortalCollisions.Add(portal);
                                }
                                return -1;
                            },
                            verts[0],
                            verts[1]);
                    }
                }
            }

            foreach (Body b in Scene.PhysWorld.BodyList)
            {
                b.ApplyForce(new Xna.Vector2(0, -9.8f/2) * b.Mass);
            }

            Scene.RemoveEntity(DebugEntity);
            DebugEntity = Scene.CreateEntity();
        }

        public void StepEnd()
        {
            foreach (Body body in Scene.PhysWorld.BodyList)
            {
                Portal portalNearest = null;
                double portalTDistance = 1f;

                Line position = new Line(body.Position, BodyExt.GetUserData(body).PreviousPosition);
                foreach (Portal p in Scene.PortalList)
                {
                    Line line = new Line(p.GetWorldVerts());
                    IntersectPoint intersect = position.Intersects(line, true);
                    if (intersect.T <= portalTDistance && intersect.Exists)
                    {
                        portalNearest = p;
                        portalTDistance = intersect.T;
                    }
                }
                if (portalNearest != null)
                {
                    Transform2D transform = new Transform2D(body.Position, body.Rotation);
                    Transform2D velocity = new Transform2D(body.LinearVelocity, body.AngularVelocity);
                    portalNearest.Enter(transform, velocity);
                    body.Position = Vector2Ext.ConvertToXna(transform.Position);
                    body.Rotation = transform.Rotation;
                    body.LinearVelocity = Vector2Ext.ConvertToXna(velocity.Position);
                    body.AngularVelocity = velocity.Rotation;
                }
            }
        }

        private void PreSolveListener(Contact contact, ref FarseerPhysics.Collision.Manifold oldManifold)
        {
            if (contact.Manifold.PointCount > 0)
            {
                if (!IsContactValid(contact))
                {
                    contact.Enabled = false;
                }
                #region debug
                if (DebugMode)
                {
                    FixtureUserData[] userData = new FixtureUserData[2];
                    userData[0] = FixtureExt.GetUserData(contact.FixtureA);
                    userData[1] = FixtureExt.GetUserData(contact.FixtureB);
                    Xna.Vector2 normal;
                    FarseerPhysics.Common.FixedArray2<Xna.Vector2> vList;
                    contact.GetWorldManifold(out normal, out vList);

                    if (userData[0].PortalCollisions.Count > 0 || userData[1].PortalCollisions.Count > 0)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            Model model = Model.CreateCube();
                            model.Transform.Scale = new Vector3(0.2f, 0.2f, 0.2f);
                            DebugEntity.Models.Add(model);
                            model.Transform.Position = new Vector3(userData[i].Fixture.Body.Position.X, userData[i].Fixture.Body.Position.Y, 0);

                            BodyUserData bodyUserData = BodyExt.GetUserData(userData[i].Fixture.Body);
                            model = Model.CreateCube();
                            model.Transform.Scale = new Vector3(0.15f, 0.15f, 0.15f);
                            model.Transform.Rotation = new Quaternion(0, 1, 1, (float)Math.PI/2);
                            DebugEntity.Models.Add(model);
                            model.Transform.Position = new Vector3(bodyUserData.PreviousPosition.X, bodyUserData.PreviousPosition.Y, 1);
                        
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            Model model = Model.CreateCube();
                            model.Transform.Scale = new Vector3(0.1f, 0.1f, 0.1f);
                            DebugEntity.Models.Add(model);
                            if (contact.Enabled)
                            {
                                model.TextureId = Renderer.Textures["grid.png"];
                            }
                            model.Transform.Position = new Vector3(vList[i].X, vList[i].Y, 0);
                        }
                    }
                    IsContactValid(contact);
                }
                #endregion
            }
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
            FarseerPhysics.Common.FixedArray2<Xna.Vector2> vList;
            contact.GetWorldManifold(out normal, out vList);

            foreach (Portal p in Scene.PortalList)
            {
                if (p.GetType() == typeof(FixturePortal))
                {
                    FixturePortal portal = (FixturePortal)p;
                    //don't consider fixtures that are a part of this portal
                    if (userData[0].Portal == portal || userData[1].Portal == portal)
                    {
                        continue;
                    }
                    
                    Line line = new Line(portal.GetWorldVerts());
                    float[] vDist = new float[2];
                    vDist[0] = line.PointDistance(vList[0], true);
                    vDist[1] = line.PointDistance(vList[1], true);
                    //only consider contacts that are between the fixture this portal is parented too and some other fixture
                    if (contact.FixtureA == portal.FixtureParent || contact.FixtureB == portal.FixtureParent)
                    {
                        //Debug.Assert(!(contact.FixtureA == portal.FixtureParent && contact.FixtureB == portal.FixtureParent));
                        if (contact.Manifold.PointCount == 1)
                        {
                            if (vDist[0] < FixturePortal.CollisionMargin)
                            {
                                return false;
                            }
                        }
                        else if (vDist[0] < FixturePortal.CollisionMargin || vDist[1] < FixturePortal.CollisionMargin)
                        {
                            return false;
                        }
                    }
                }
            }

            for (int i0 = 0; i0 < userData.Length; i0++)
            {
                int i1 = (i0 + 1) % userData.Length;
                foreach (FixturePortal portal in userData[i0].PortalCollisions)
                {
                    if (userData[i0].Portal == portal || userData[i1].Portal == portal)
                    {
                        continue;
                    }
                    Line line = new Line(portal.GetWorldVerts());
                    //Xna.Vector2 pos = userData[i0].Fixture.Body.Position;
                    Xna.Vector2 pos = BodyExt.GetUserData(userData[i0].Fixture.Body).PreviousPosition;
                    bool sideOf = line.GetSideOf(vList[0]) != line.GetSideOf(pos);
                    if (contact.Manifold.PointCount == 1)
                    {
                        if (sideOf)
                        {
                            return false;
                        }
                    }
                    else if (sideOf || line.GetSideOf(vList[1]) != line.GetSideOf(pos))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
