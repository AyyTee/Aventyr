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
using ClipperLib;
using Game.Portals;

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
                    FixtureUserData userData = FixtureExt.GetUserData(f);
                    userData.ProcessChanges();
                    userData.PortalCollisionsPrevious = new HashSet<IPortal>(userData.PortalCollisions);
                    userData.PortalCollisions.Clear();
                    //FixtureExt.GetUserData(f).PortalCollisionsClear();
                }
                BodyExt.GetUserData(body).PreviousPosition = body.Position;
            }
            foreach (IPortal portal in Scene.GetPortalList())
            {
                if (Portal.IsValid(portal))
                {
                    Xna.Vector2[] verts = Vector2Ext.ConvertToXna(Portal.GetWorldVerts(portal));
                    Scene.World.RayCast(
                        delegate (Fixture fixture, Xna.Vector2 point, Xna.Vector2 normal, float fraction)
                        {
                            FixturePortal fixturePortal = portal as FixturePortal;
                            if (fixturePortal != null)
                            {
                                //Ignore any fixtures that are attached to the same body as the portal fixture.
                                if (fixture.Body != FixtureExt.GetFixtureAttached(fixturePortal).Body)
                                {
                                    FixtureExt.GetUserData(fixture).PortalCollisions.Add(portal);
                                    //FixtureExt.GetUserData(fixture).PortalCollisionAdd(portal);
                                }
                            }
                            else
                            {
                                //Other portal types don't have fixtures so there is no need to check.
                                FixtureExt.GetUserData(fixture).PortalCollisions.Add(portal);
                                //FixtureExt.GetUserData(fixture).PortalCollisionAdd(portal);
                            }
                            return -1;
                        },
                        verts[0],
                        verts[1]);
                }
            }

            foreach (Body b in Scene.World.BodyList)
            {
                //BodyExt.GetUserData(b).UpdatePortalCollisions(ref bodiesToRemove);
                b.ApplyForce(new Xna.Vector2(0, -9.8f / 2) * b.Mass);

                ActorExt.GetGravity(BodyExt.GetUserData(b).Actor, Scene.GetPortalList(), new Vector2(0, -9.8f / 2));

                BodyExt.GetUserData(b).UpdatePortalCollisions();
            }
            

            if (DebugEntity != null)
            {
                DebugEntity.Remove();
            }
            DebugEntity = new Entity(Scene);
            DebugEntity.IsPortalable = false;
        }

        public void StepEnd()
        {
            /*foreach (Body body in Scene.World.BodyList)
            {
                IPortal portalNearest = null;
                double portalTDistance = 1f;

                Vector2 prevPos = Vector2Ext.ConvertTo(BodyExt.GetUserData(body).PreviousPosition);
                Vector2 pos = Vector2Ext.ConvertTo(body.Position);
                Line position = new Line(prevPos, pos);
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
            }*/

            #region Debug
            if (DebugMode)
            {
                foreach (Body body in Scene.World.BodyList)
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
            #endregion
        }

        private void PreSolveListener(Contact contact, ref Manifold oldManifold)
        {
            if (contact.IsTouching)
            {
                Debug.Assert(contact.Manifold.PointCount > 0);

                /* Sometimes a body will tunnel through a portal fixture.  
                 * The circumstances aren't well understood but checking if the contact normal 
                 * is facing from fixtureB to fixtureA (it should always face from A to B 
                 * according to the Box2D documentation) and then reversing the normal if it is  
                 * helps prevent tunneling from occuring.*/
                if (!FixtureExt.GetUserData(contact.FixtureA).IsPortalParentless() || !FixtureExt.GetUserData(contact.FixtureB).IsPortalParentless())
                {
                    Xna.Vector2 normal;
                    FixedArray2<Xna.Vector2> vList;
                    contact.GetWorldManifold(out normal, out vList);

                    Vector2 center0 = FixtureExt.GetCenterWorld(contact.FixtureA);
                    Vector2 center1 = FixtureExt.GetCenterWorld(contact.FixtureB);

                    if (Math.Abs(MathExt.AngleDiff(center1 - center0, Vector2Ext.ConvertTo(normal))) > Math.PI / 2)
                    {
                        contact.Manifold.LocalNormal *= -1;
                    }
                }

                if (!IsContactValid(contact))
                {
                    contact.Enabled = false;
                    return;
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

                    if (contact.Manifold.PointCount == 2)
                    {
                        Model line = ModelFactory.CreateLines(
                           new Line[] {
                            new Line(Vector2Ext.ConvertTo(vList[0]), Vector2Ext.ConvertTo(vList[1]))
                           });
                        if (contact.Enabled)
                        {
                            line.SetColor(new Vector3(1, 1, 0.2f));
                        }
                        else
                        {
                            line.SetColor(new Vector3(0.5f, 0.5f, 0));
                        }
                        line.Transform.Position += new Vector3(0, 0, 5);
                        DebugEntity.AddModel(line);
                    }


                    for (int i = 0; i < 2; i++)
                    {
                        float scale = 0.8f;
                        Vector3 pos = new Vector3(vList[i].X, vList[i].Y, 5);
                        //Ignore contact points that are exactly on the origin. These are almost certainly null values.
                        if (pos.X == 0 && pos.Y == 0)
                        {
                            continue;
                        }
                        Vector2 arrowNormal = Vector2Ext.ConvertTo(normal) * 0.2f * scale;
                        if (i == 0)
                        {
                            arrowNormal *= -1;
                        }
                        Model arrow = ModelFactory.CreateArrow(pos, arrowNormal, 0.02f * scale, 0.05f * scale, 0.03f * scale);
                        DebugEntity.AddModel(arrow);

                        Model model = ModelFactory.CreateCube();
                        model.Transform.Scale = new Vector3(0.08f, 0.08f, 0.08f) * scale;
                        DebugEntity.AddModel(model);
                        if (contact.Enabled)
                        {
                            model.SetColor(new Vector3(1, 1, 0.2f));
                            arrow.SetColor(new Vector3(1, 1, 0.2f));
                        }
                        else
                        {
                            model.SetColor(new Vector3(0.5f, 0.5f, 0));
                            arrow.SetColor(new Vector3(0.5f, 0.5f, 0));
                        }
                        if (userData[0].IsPortalParentless() && userData[1].IsPortalParentless())
                        {
                            arrow.SetColor(new Vector3(0.7f, 0.5f, 0.2f));
                        }
                        model.Transform.Position = pos;

                        if (!userData[i].IsPortalParentless())
                        {
                            IList<Vector2> vertices = Vector2Ext.ConvertTo(((PolygonShape)userData[i].Fixture.Shape).Vertices);
                            Model fixtureModel = ModelFactory.CreatePolygon(vertices);
                            fixtureModel.SetColor(new Vector3(0, 1, 1));
                            fixtureModel.Transform = userData[i].Actor.GetTransform().Get3D();
                            fixtureModel.Transform.Position += new Vector3(0, 0, 5);
                            fixtureModel.Transform.Scale = Vector3.One;

                            DebugEntity.AddModel(fixtureModel);
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
            FixtureUserData[] fixtureData = new FixtureUserData[2];
            fixtureData[0] = FixtureExt.GetUserData(contact.FixtureA);
            fixtureData[1] = FixtureExt.GetUserData(contact.FixtureB);

            BodyUserData[] bodyData = new BodyUserData[2];
            bodyData[0] = BodyExt.GetUserData(contact.FixtureA.Body);
            bodyData[1] = BodyExt.GetUserData(contact.FixtureB.Body);

            Xna.Vector2 normal;
            FixedArray2<Xna.Vector2> vList;
            contact.GetWorldManifold(out normal, out vList);


            if (bodyData[0].IsChild || bodyData[1].IsChild)
            {
                if (bodyData[0].IsChild && bodyData[1].IsChild)
                {
                    return true;
                }

                int childIndex = bodyData[0].IsChild ? 0 : 1;
                int otherIndex = bodyData[0].IsChild ? 1 : 0;
                BodyUserData bodyDataChild = bodyData[childIndex];
                BodyUserData bodyDataOther = bodyData[otherIndex];
                FixtureUserData fixtureDataChild = fixtureData[childIndex];
                FixtureUserData fixtureDataOther = fixtureData[otherIndex];

                //Contact is invalid if it is between two fixtures where one fixture is colliding with a portal on the other fixture.
                if (fixtureData[0].IsPortalParentless() && fixtureData[1].IsPortalParentless())
                {
                    for (int i = 0; i < fixtureData.Length; i++)
                    {
                        int iNext = (i + 1) % fixtureData.Length;
                        var intersection = fixtureData[iNext].GetPortalChildren().Intersect(fixtureData[i].PortalCollisions);
                        if (intersection.Count() > 0)
                        {
                            //Debug.Fail("Fixtures with portal collisions should be filtered.");
                            return false;
                        }
                    }
                }

                return true;
            }

            //Contact is invalid if it is between two fixtures where one fixture is colliding with a portal on the other fixture.
            if (fixtureData[0].IsPortalParentless() && fixtureData[1].IsPortalParentless())
            {
                for (int i = 0; i < fixtureData.Length; i++)
                {
                    int iNext = (i + 1) % fixtureData.Length;
                    var intersection = fixtureData[iNext].GetPortalChildren().Intersect(fixtureData[i].PortalCollisions);
                    if (intersection.Count() > 0)
                    {
                        //Debug.Fail("Fixtures with portal collisions should be filtered.");
                        return false;
                    }
                }
            }

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
                    //Don't consider this portal if its fixtures are part of the contact.
                    if (fixtureData[0].PartOfPortal(portal) || fixtureData[1].PartOfPortal(portal))
                    {
                        continue;
                    }

                    Line line = new Line(Portal.GetWorldVerts(portal));
                    double[] vDist = new double[] {
                        MathExt.PointLineDistance(vList[0], line, true),
                        MathExt.PointLineDistance(vList[1], line, true)
                    };
                    //Only consider contacts that are between the fixture this portal is parented too and some other fixture.
                    if (contact.FixtureA == FixtureExt.GetFixtureAttached(portal) || contact.FixtureB == FixtureExt.GetFixtureAttached(portal))
                    {
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

            //Contact is invalid if it is on the opposite side of a colliding portal.
            for (int i = 0; i < fixtureData.Length; i++)
            {
                int iNext = (i + 1) % fixtureData.Length;
                foreach (IPortal portal in fixtureData[i].PortalCollisions)
                {
                    Line line = new Line(Portal.GetWorldVerts(portal));

                    FixturePortal cast = portal as FixturePortal;
                    if (cast != null)
                    {
                        if (fixtureData[i].PartOfPortal(cast) || fixtureData[iNext].PartOfPortal(cast))
                        {
                            continue;
                        }
                    }

                    //Contact is invalid if it is on the opposite side of the portal from its body origin.
                    Xna.Vector2 pos = BodyExt.GetUserData(fixtureData[i].Fixture.Body).PreviousPosition;
                    bool sideOf = line.GetSideOf(vList[0]) != line.GetSideOf(pos);
                    Debug.Assert(contact.Manifold.PointCount > 0);
                    if (contact.Manifold.PointCount == 1)
                    {
                        if (sideOf)
                        {
                            return false;
                        }
                    }
                    else if (sideOf || line.GetSideOf(vList[1]) != line.GetSideOf(pos))
                    //else if (line.GetSideOf((vList[0] + vList[1])/2) != line.GetSideOf(pos))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
