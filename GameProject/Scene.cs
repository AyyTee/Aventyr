using OpenTK;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Dynamics;
using Xna = Microsoft.Xna.Framework;
using System.Diagnostics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;
using Poly2Tri;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;
using System;
using FarseerPhysics.Dynamics.Contacts;

namespace Game
{
    [DataContract]
    public class Scene : IRenderLayer
    {
        public World World { get; private set; }
        PhysContactListener _contactListener;

        public ICamera2 ActiveCamera { get; private set; }
        public List<SceneNode> SceneNodeList { get { return Tree<SceneNode>.ToList(Root); } }
        public List<Entity> EntityList { get { return Tree<SceneNode>.FindByType<Entity>(Root); } }
        /// <summary>Root node to the scene graph.</summary>
        [DataMember]
        public SceneNode Root { get; private set; }

        #region Constructors
        public Scene()
        {
            Root = new SceneNode(this);
            SetWorld(new World(new Xna.Vector2(0f, 0f)));
        }
        #endregion
        
        public void Step()
        {
            Step(1/(float)Controller.StepsPerSecond);
        }

        public void Step(float stepSize)
        {
            World.ProcessChanges();
            foreach (SceneNode s in FindByType<SceneNode>())
            {
                s.StepBegin();
            }
            foreach (SceneNodePlaceable s in FindByType<SceneNodePlaceable>())
            {
                //Skip Actors, they handle movement using rigid body physics.
                if (s.GetType() == typeof(Actor))
                {
                    continue;
                }
                //Parented SceneNodes can't perform portal teleportation directly.
                if (s.Parent == s.Scene.Root)
                {
                    RayCast(s);
                }
                else
                {
                    s.SetTransform(s.GetTransform().Add(s.GetVelocity()));
                }
            }
            if (World != null)
            {
                _contactListener.StepBegin();
                World.Step(stepSize);
                _contactListener.StepEnd();
            }
            foreach (SceneNode s in FindByType<SceneNode>())
            {
                s.StepEnd();
            }
        }

        public ICamera2 GetCamera()
        {
            return ActiveCamera;
        }

        public List<IRenderable> GetRenderList()
        {
            return SceneNodeList.OfType<IRenderable>().ToList();
        }

        public List<Portal> GetPortalList()
        {
            return Tree<SceneNode>.FindByType<Portal>(Root);
        }

        public void RayCast(SceneNodePlaceable placeable)
        {
            if (placeable.GetVelocity().Position.Length == 0)
            {
                return;
            }
            _rayCast(placeable, placeable.GetVelocity().Position.Length, null, 50);
        }

        private void _rayCast(SceneNodePlaceable placeable, double movementLeft, Portal portalPrevious, int depthMax)
        {
            if (depthMax <= 0)
            {
                return;
            }
            Transform2 begin = placeable.GetTransform();
            Transform2 velocity = placeable.GetVelocity();
            double distanceMin = movementLeft;
            Portal portalNearest = null;
            IntersectPoint intersectNearest = new IntersectPoint();
            foreach (Portal p in FindByType<Portal>())
            {
                if (!p.IsValid())
                {
                    continue;
                }
                if (portalPrevious == p)
                {
                    continue;
                }
                Line portalLine = new Line(p.GetWorldVerts());
                Line ray = new Line(begin.Position, begin.Position + velocity.Position);
                IntersectPoint intersect = portalLine.Intersects(ray, true);
                //IntersectPoint intersect2 = portalLine.IntersectsParametric(p.GetVelocity(), ray, 5);
                double distance = ((Vector2d)begin.Position - intersect.Position).Length;
                if (intersect.Exists && distance < distanceMin)
                {
                    distanceMin = distance;
                    portalNearest = p;
                    intersectNearest = intersect;
                }
            }
            if (portalNearest != null)
            {
                movementLeft -= distanceMin;
                begin.Position = (Vector2)intersectNearest.Position;
                placeable.SetTransform(begin);
                portalNearest.Enter(placeable);
                _rayCast(placeable, movementLeft, portalNearest.Linked, depthMax - 1);
            }
            else
            {
                begin.Position += velocity.Position.Normalized() * (float)movementLeft;
                /*After the end position of the ray has been determined, adjust it's position so that it isn't too close to any portal.  
                Otherwise there is a risk of ambiguity as to which side of a portal the end point is on.*/
                foreach (Portal p in FindByType<Portal>())
                {
                    if (!p.IsValid())
                    {
                        continue;
                    }
                    Line exitLine = new Line(p.GetWorldVerts());
                    Vector2 position = begin.Position;
                    float distanceToPortal = exitLine.PointDistance(position, true);
                    if (distanceToPortal < Portal.EnterMinDistance)
                    {
                        Vector2 exitNormal = p.GetWorldTransform().GetRight();
                        Side sideOf;
                        if (p == portalPrevious)
                        {
                            sideOf = exitLine.GetSideOf(position + velocity.Position);
                        }
                        else
                        {
                            sideOf = exitLine.GetSideOf(position - velocity.Position);
                        }
                        if (sideOf != exitLine.GetSideOf(exitNormal + p.GetWorldTransform().Position))
                        {
                            exitNormal = -exitNormal;
                        }

                        Vector2 pos = exitNormal * (Portal.EnterMinDistance - distanceToPortal);
                        begin.Position += pos;
                        break;
                    }
                }
                placeable.SetTransform(begin);
            }
        }

        public List<T> FindByType<T>() where T : SceneNode
        {
            return Tree<SceneNode>.FindByType<T>(Root);
        }

        public SceneNode FindByName(string name)
        {
            return Root.FindByName(name);
        }

        public void SetActiveCamera(ICamera2 camera)
        {
            ActiveCamera = camera;
        }

        /// <summary>
        /// Assigns a physics world to this scene. Can only be done if there isn't already a physics world assigned.
        /// </summary>
        public void SetWorld(World world)
        {
            //Debug.Assert(World == null, "A physics world has already been assigned to this scene.");
            World = world;
            World.ProcessChanges();
            _contactListener = new PhysContactListener(this);
        }
    }
}
