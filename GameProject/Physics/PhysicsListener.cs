using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Game.Common;
using Game.Models;
using Game.Portals;
using Game.Rendering;
using OpenTK;
using Vector2 = OpenTK.Vector2;
using Vector3 = OpenTK.Vector3;
using Xna = Microsoft.Xna.Framework;

namespace Game.Physics
{
    public class PhyicsListener
    {
        public readonly Scene Scene;
        Entity _debugEntity;
        bool _debugMode = false;

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
                fixtures.RemoveAll(item => !FixtureExt.GetData(item).IsPortalParentless());
                foreach (Fixture f in fixtures)
                {
                    FixtureData userData = FixtureExt.GetData(f);
                    userData.ProcessChanges();
                    userData.PortalCollisionsPrevious = new HashSet<IPortal>(userData.PortalCollisions);
                    userData.PortalCollisions.Clear();
                    userData.PortalCollisions.UnionWith(FixtureExt.GetPortalCollisions(f, Scene.GetPortalList()));
                }
                BodyExt.GetData(body).PreviousPosition = body.Position;
            }

            foreach (Actor actor in Scene.GetAll().OfType<Actor>())
            {
                actor.Update();

                Vector2 centroid = actor.GetCentroid();
                foreach (BodyData data in Tree<BodyData>.GetAll(BodyExt.GetData(actor.Body)))
                {
                    //data.Body.LocalCenter = actor.Body.GetLocalPoint((Xna.Vector2)centroid);
                }
                actor.ApplyGravity(Scene.Gravity);
            }

            if (_debugMode)
            {
                if (_debugEntity != null)
                {
                    Scene.MarkForRemoval(_debugEntity);
                }
                _debugEntity = new Entity(Scene) { IsPortalable = false };
            }
            
            foreach (Actor actor in Scene.GetAll().OfType<Actor>())
            {
                Actor.AssertTransform(actor);
                Actor.AssertBodyType(actor);
            }
        }

        public void StepEnd()
        {
            #region Debug
            if (_debugMode)
            {
                foreach (Body body in Scene.World.BodyList)
                {
                    Model model = ModelFactory.CreateCube();
                    model.Transform.Scale = new Vector3(0.03f, 0.03f, 0.03f);
                    _debugEntity.AddModel(model);
                    model.Transform.Position = new Vector3(body.Position.X, body.Position.Y, 10);
                    model.SetColor(new Vector3(1, 0.5f, 0.5f));

                    BodyData bodyUserData = BodyExt.GetData(body);
                    Model positionPrev = ModelFactory.CreateCube();
                    positionPrev.Transform.Scale = new Vector3(0.03f, 0.03f, 0.03f);
                    positionPrev.Transform.Rotation = new Quaternion(0, 1, 1, (float)Math.PI / 2);
                    _debugEntity.AddModel(positionPrev);
                    positionPrev.Transform.Position = new Vector3(bodyUserData.PreviousPosition.X, bodyUserData.PreviousPosition.Y, 10);
                    positionPrev.SetColor(new Vector3(1, 0, 0));
                }
            }
            #endregion
        }

        void PreSolveListener(Contact contact, ref Manifold oldManifold)
        {
            if (contact.IsTouching)
            {
                Debug.Assert(contact.Manifold.PointCount > 0);

                /* Sometimes a body will tunnel through a portal fixture.  
                 * The circumstances aren't well understood but checking if the contact normal 
                 * is facing from fixtureB to fixtureA (it should always face from A to B 
                 * according to the Box2D documentation) and then reversing the normal if it is  
                 * helps prevent tunneling from occuring.*/
                if (!FixtureExt.GetData(contact.FixtureA).IsPortalParentless() || !FixtureExt.GetData(contact.FixtureB).IsPortalParentless())
                {
                    Xna.Vector2 normal;
                    FixedArray2<Xna.Vector2> vList;
                    contact.GetWorldManifold(out normal, out vList);

                    Vector2 center0 = FixtureExt.GetCenterWorld(contact.FixtureA);
                    Vector2 center1 = FixtureExt.GetCenterWorld(contact.FixtureB);

                    if (Math.Abs(MathExt.AngleDiff(center1 - center0, (Vector2)normal)) > Math.PI / 2)
                    {
                        contact.Manifold.LocalNormal *= -1;
                    }
                }

                if (!IsContactValid(contact))
                {
                    contact.Enabled = false;
                    //return;
                }
                else
                {
                    Actor actor0 = BodyExt.GetData(contact.FixtureA.Body).Actor;
                    Actor actor1 = BodyExt.GetData(contact.FixtureB.Body).Actor;
                    actor0.CallOnCollision(actor1, true);
                    actor1.CallOnCollision(actor0, false);
                }
                #region Debug
                if (_debugMode)
                {
                    FixtureData[] userData = new FixtureData[2];
                    userData[0] = FixtureExt.GetData(contact.FixtureA);
                    userData[1] = FixtureExt.GetData(contact.FixtureB);
                    Xna.Vector2 normal;
                    FixedArray2<Xna.Vector2> vList;
                    contact.GetWorldManifold(out normal, out vList);

                    if (contact.Manifold.PointCount == 2)
                    {
                        Model line = ModelFactory.CreateLines(
                            new[] {
                            new LineF(vList[0], vList[1])
                            });
                        line.SetColor(contact.Enabled ? 
                            new Vector3(1, 1, 0.2f) : 
                            new Vector3(0.5f, 0.5f, 0));
                        line.Transform.Position += new Vector3(0, 0, 5);
                        _debugEntity.AddModel(line);
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
                        Vector2 arrowNormal = (Vector2)normal * 0.2f * scale;
                        if (i == 0)
                        {
                            arrowNormal *= -1;
                        }
                        Model arrow = ModelFactory.CreateArrow(pos, arrowNormal, 0.02f * scale, 0.05f * scale, 0.03f * scale);
                        _debugEntity.AddModel(arrow);

                        Model model = ModelFactory.CreateCube();
                        model.Transform.Scale = new Vector3(0.08f, 0.08f, 0.08f) * scale;
                        _debugEntity.AddModel(model);
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
                            IList<Vector2> vertices = Vector2Ext.ToOtk(((PolygonShape)userData[i].Fixture.Shape).Vertices);
                            Model fixtureModel = ModelFactory.CreatePolygon(vertices);
                            fixtureModel.SetColor(new Vector3(0, 1, 1));
                            fixtureModel.Transform = userData[i].Actor.GetTransform().Get3D();
                            fixtureModel.Transform.Position += new Vector3(0, 0, 5);
                            fixtureModel.Transform.Scale = Vector3.One;

                            _debugEntity.AddModel(fixtureModel);
                        }
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Returns true if a contact should not be disabled due to portal clipping.
        /// </summary>
        bool IsContactValid(Contact contact)
        {
            FixtureData[] fixtureData = new FixtureData[2];
            fixtureData[0] = FixtureExt.GetData(contact.FixtureA);
            fixtureData[1] = FixtureExt.GetData(contact.FixtureB);

            BodyData[] bodyData = new BodyData[2];
            bodyData[0] = BodyExt.GetData(contact.FixtureA.Body);
            bodyData[1] = BodyExt.GetData(contact.FixtureB.Body);

            Xna.Vector2 normal;
            FixedArray2<Xna.Vector2> vList;
            contact.GetWorldManifold(out normal, out vList);


            if (bodyData[0].IsChild || bodyData[1].IsChild)
            {
                if (bodyData[0].IsChild && bodyData[1].IsChild)
                {
                    //return true;
                }

                int childIndex = bodyData[0].IsChild ? 0 : 1;
                int otherIndex = bodyData[0].IsChild ? 1 : 0;
                BodyData bodyDataChild = bodyData[childIndex];
                BodyData bodyDataOther = bodyData[otherIndex];
                FixtureData fixtureDataChild = fixtureData[childIndex];
                FixtureData fixtureDataOther = fixtureData[otherIndex];

            }

            //Contact is invalid if it is between two fixtures where one fixture is colliding with a portal on the other fixture.
            if (fixtureData[0].IsPortalParentless() && fixtureData[1].IsPortalParentless())
            {
                for (int i = 0; i < fixtureData.Length; i++)
                {
                    int iNext = (i + 1) % fixtureData.Length;
                    var intersection = fixtureData[iNext].GetPortalChildren().Intersect(fixtureData[i].PortalCollisions);
                    if (intersection.Any())
                    {
                        //Debug.Fail("Fixtures with portal collisions should be filtered.");
                        return false;
                    }
                }
            }

            //Contact is invalid if it is too close to a portal.
            foreach (IPortal p in Scene.GetPortalList())
            {
                if (!p.IsValid())
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

                    LineF line = new LineF(portal.GetWorldVerts());
                    double[] vDist = {
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
                        else if (vDist[0] < FixturePortal.CollisionMargin && vDist[1] < FixturePortal.CollisionMargin)
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
                    LineF line = new LineF(portal.GetWorldVerts());

                    FixturePortal cast = portal as FixturePortal;
                    if (cast != null)
                    {
                        if (fixtureData[i].PartOfPortal(cast) || fixtureData[iNext].PartOfPortal(cast))
                        {
                            continue;
                        }
                    }

                    //Contact is invalid if it is on the opposite side of the portal from its body origin.
                    //Xna.Vector2 pos = BodyExt.GetData(fixtureData[i].Fixture.Body).PreviousPosition;
                    Vector2 pos = BodyExt.GetLocalOrigin(fixtureData[i].Fixture.Body);
                    bool oppositeSides0 = line.GetSideOf(vList[0]) != line.GetSideOf(pos);
                    Debug.Assert(contact.Manifold.PointCount > 0);
                    if (contact.Manifold.PointCount == 1)
                    {
                        if (oppositeSides0)
                        {
                            return false;
                        }
                    }
                    else
                    //else if (line.GetSideOf((vList[0] + vList[1])/2) != line.GetSideOf(pos))
                    {
                        bool oppositeSides1 = line.GetSideOf(vList[1]) != line.GetSideOf(pos);
                        /*if (oppositeSides0 && oppositeSides1)
                        {
                            return false;
                        }
                        if (oppositeSides0)
                        {
                            contact.Manifold.Points[0] = contact.Manifold.Points[1];
                        }
                        contact.Manifold.PointCount = 1;
                        
                        return true;*/
                        if (!oppositeSides0 && !oppositeSides1)
                        {
                            continue;
                        }
                        if (!fixtureData[iNext].PortalCollisions.Contains(portal) || !(oppositeSides0 || oppositeSides1))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
