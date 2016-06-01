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
    public class PhyicsListener
    {
        public Scene Scene { get; private set; }
        Entity DebugEntity;
        bool DebugMode = true;

        public PhyicsListener(Scene scene)
        {
            Scene = scene;
            Scene.World.ContactManager.PreSolve = PreSolveListener;
        }

        public void StepBegin()
        {
            foreach (Body body in Scene.World.BodyList)
            {
                //The number of fixtures is going to change so a copy of FixtureList is made.
                List<Fixture> fixtures = new List<Fixture>(body.FixtureList);
                //Don't include fixtures that are used for FixturePortal collisions.
                fixtures.RemoveAll(item => !FixtureExt.GetUserData(item).IsPortalParentless());
                foreach (Fixture f in fixtures)
                {
                    FixtureExt.GetUserData(f).ProcessChanges();
                    FixtureExt.GetUserData(f).PortalCollisions.Clear();
                }
                BodyExt.GetUserData(body).PreviousPosition = body.Position;
            }
            foreach (IPortal portal in Scene.GetPortalList().OfType<IPortal>())
            {
                if (portal.GetWorldTransform() != null)
                {
                    Xna.Vector2[] verts = Vector2Ext.ConvertToXna(Portal.GetWorldVerts(portal));
                    Scene.World.RayCast(
                        delegate(Fixture fixture, Xna.Vector2 point, Xna.Vector2 normal, float fraction)
                        {
                            FixturePortal fixturePortal = portal as FixturePortal;
                            if (fixturePortal != null)
                            {
                                //Ignore any fixtures that are attached to the same body as the portal fixture.
                                if (fixture.Body != FixtureExt.GetFixturePortalParent(fixturePortal).Body)
                                {
                                    FixtureExt.GetUserData(fixture).PortalCollisions.Add(portal);
                                }
                            }
                            else
                            {
                                //Other portal types don't have fixtures so there is no need to check.
                                FixtureExt.GetUserData(fixture).PortalCollisions.Add(portal);
                            }
                            return -1;
                        },
                        verts[0],
                        verts[1]);
                }
            }
            //List<Body> bodiesToRemove = new List<Body>();

            foreach (Body b in Scene.World.BodyList)
            {
                //BodyExt.GetUserData(b).UpdatePortalCollisions(ref bodiesToRemove);
                b.ApplyForce(new Xna.Vector2(0, -9.8f/2) * b.Mass);
            }
            /*foreach (Body b in bodiesToRemove)
            {
                Scene.World.RemoveBody(b);
            }*/

            if (DebugEntity != null)
            {
                DebugEntity.Remove();
            }
            DebugEntity = new Entity(Scene);
        }

        public void StepEnd()
        {
            foreach (Body body in Scene.World.BodyList)
            {
                IPortal portalNearest = null;
                double portalTDistance = 1f;

                Line position = new Line(body.Position, BodyExt.GetUserData(body).PreviousPosition);
                foreach (IPortal p in Scene.GetPortalList())
                {
                    if (!Portal.IsValid(p))
                    {
                        continue;
                    }
                    Line line = new Line(Portal.GetWorldVerts(p));
                    IntersectCoord intersect = MathExt.LineLineIntersect(position, line, true);
                    if (intersect.TFirst <= portalTDistance && intersect.Exists)
                    {
                        portalNearest = p;
                        portalTDistance = intersect.TFirst;
                    }
                }
                if (portalNearest != null)
                {
                    IPortalable actor = BodyExt.GetUserData(body).Actor;
                    Portal.Enter(portalNearest, actor);
                }

                if (DebugMode)
                {
                    Model model = ModelFactory.CreateCube();
                    model.Transform.Scale = new Vector3(0.03f, 0.03f, 0.03f);
                    DebugEntity.AddModel(model);
                    model.Transform.Position = new Vector3(body.Position.X, body.Position.Y, 10);
                    model.SetColor(new Vector3(1, 0.5f, 0.5f));

                    BodyUserData bodyUserData = BodyExt.GetUserData(body);
                    Model positionPrev = ModelFactory.CreateCube();
                    positionPrev.Transform.Scale = new Vector3(0.03f, 0.03f, 0.03f);
                    positionPrev.Transform.Rotation = new Quaternion(0, 1, 1, (float)Math.PI / 2);
                    DebugEntity.AddModel(positionPrev);
                    positionPrev.Transform.Position = new Vector3(bodyUserData.PreviousPosition.X, bodyUserData.PreviousPosition.Y, 10);
                    positionPrev.SetColor(new Vector3(1, 0, 0));
                }
            }
        }

        private void PreSolveListener(Contact contact, ref Manifold oldManifold)
        {
            if (contact.IsTouching)
            {
                if (!IsContactValid(contact))
                {
                    contact.Enabled = false;
                }
                #region Debug
                if (DebugMode)
                {
                    FixtureUserData[] userData = new FixtureUserData[2];
                    userData[0] = FixtureExt.GetUserData(contact.FixtureA);
                    userData[1] = FixtureExt.GetUserData(contact.FixtureB);
                    Xna.Vector2 normal;
                    FixedArray2<Xna.Vector2> vList;
                    contact.GetWorldManifold(out normal, out vList);

                    if (userData[0].PortalCollisions.Count > 0 || userData[1].PortalCollisions.Count > 0)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            Model model = ModelFactory.CreateCube();
                            model.Transform.Scale = new Vector3(0.02f, 0.02f, 0.02f);
                            model.Transform.Position = new Vector3(0, 0, 10);
                            DebugEntity.AddModel(model);
                            if (contact.Enabled)
                            {
                                model.SetColor(new Vector3(1, 1, 0.2f));
                            }
                            else
                            {
                                model.SetColor(new Vector3(0.5f, 0.5f, 0));
                            }
                            model.Transform.Position = new Vector3(vList[i].X, vList[i].Y, 0);
                        }
                    }
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
            FixedArray2<Xna.Vector2> vList;
            contact.GetWorldManifold(out normal, out vList);

            //Contact is invalid if it is too close to a portal.
            foreach (IPortal p in Scene.GetPortalList())
            {
                if (!Portal.IsValid(p))
                {
                    continue;
                }
                FixturePortal portal = p as FixturePortal;
                if (portal != null)
                {
                    //Don't consider this portal if its fixtures are in the contact.
                    if (userData[0].PartOfPortal(portal) || userData[1].PartOfPortal(portal))
                    {
                        continue;
                    }

                    Line line = new Line(Portal.GetWorldVerts(portal));
                    double[] vDist = new double[] {
                        MathExt.PointLineDistance(vList[0], line, true),
                        MathExt.PointLineDistance(vList[1], line, true)
                    };
                    //Only consider contacts that are between the fixture this portal is parented too and some other fixture.
                    if (contact.FixtureA == FixtureExt.GetFixturePortalParent(portal) || contact.FixtureB == FixtureExt.GetFixturePortalParent(portal))
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
                        //else if ((vDist[0] + vDist[1])/2 < FixturePortal.CollisionMargin)
                        {
                            return false;
                        }
                    }
                }
            }

            //Contact is invalid if it is on the opposite side of the portal from its body origin.
            for (int i = 0; i < userData.Length; i++)
            {
                int iNext = (i + 1) % userData.Length;
                foreach (FixturePortal portal in userData[i].PortalCollisions)
                {
                    if (userData[i].PartOfPortal(portal) || userData[iNext].PartOfPortal(portal))
                    {
                        continue;
                    }
                    Line line = new Line(Portal.GetWorldVerts(portal));
                    //Xna.Vector2 pos = userData[i0].Fixture.Body.Position;
                    Xna.Vector2 pos = BodyExt.GetUserData(userData[i].Fixture.Body).PreviousPosition;
                    bool sideOf = line.GetSideOf(vList[0]) != line.GetSideOf(pos);
                    Debug.Assert(contact.Manifold.PointCount > 0);
                    if (contact.Manifold.PointCount == 1)
                    {
                        if (sideOf)
                        {
                            return false;
                        }
                    }
                    //else if (sideOf || line.GetSideOf(vList[1]) != line.GetSideOf(pos))
                    else if (line.GetSideOf((vList[0] + vList[1])/2) != line.GetSideOf(pos))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
