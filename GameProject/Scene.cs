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
    public class Scene
    {
        public World World { get; private set; }
        /// <summary>Number of unique ids that have been used for SceneNodes within this scene.</summary>
        int _idCount = 0;
        PhysContactListener _contactListener;

        public Camera2D ActiveCamera { get; private set; }
        public List<SceneNode> SceneNodeList { get { return Root.FindByType<SceneNode>(); } }
        public List<Portal> PortalList { get { return Root.FindByType<Portal>(); } }
        public List<Entity> EntityList { get { return Root.FindByType<Entity>(); } }
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
                //SceneNodes parented to a SceneNodePlaceable instance can't perform portal teleportation directly.
                if (s.Parent.GetType() != typeof(ITransform2D))
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

        public void RayCast(SceneNodePlaceable placeable)
        {
            Transform2D transform = placeable.GetTransform();
            Transform2D velocity = placeable.GetVelocity();
            RayCast(transform, velocity);
            placeable.SetTransform(transform);
            placeable.SetVelocity(velocity);
        }

        public void RayCast(Transform2D begin, Transform2D velocity)
        {
            _rayCast(begin, velocity, velocity.Position.Length, null);
            begin.Rotation += velocity.Rotation;
            begin.Scale *= velocity.Scale;
        }

        private void _rayCast(Transform2D begin, Transform2D velocity, double movementLeft, Portal portalPrevious)
        {
            if (velocity.Position.Length == 0)
            {
                return;
            }
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
                portalNearest.Enter(begin, velocity);
                _rayCast(begin, velocity, movementLeft, portalNearest.Linked);
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
                        Line.Side sideOf;
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
            }
        }

        public List<T> FindByType<T>() where T : SceneNode
        {
            return Root.FindByType<T>();
        }

        public SceneNode FindByName(string name)
        {
            return Root.FindByName(name);
        }

        /// <summary>Get a unique SceneNode id.</summary>
        public int GetId()
        {
            _idCount++;
            return _idCount - 1;
        }

        public void SetActiveCamera(Camera2D camera)
        {
            Debug.Assert(camera.Scene == this);
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

        public Scene DeepClone()
        {
            return DeepClone(new Scene());
        }

        /// <summary>
        /// Clones everything into a new scene.
        /// </summary>
        /// <param name="scene">Scene to clone into.</param>
        /// <returns></returns>
        public Scene DeepClone(Scene scene)
        {
            Root.DeepClone(scene);
            return scene;
        }
    }
}
