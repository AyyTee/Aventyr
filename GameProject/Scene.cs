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
using Game.Portals;

namespace Game
{
    [DataContract]
    public class Scene : IRenderLayer, IScene
    {
        public World World { get; private set; }
        PhyicsListener _contactListener;

        public ICamera2 ActiveCamera { get; private set; }
        public List<SceneNode> SceneNodeList { get { return Tree<SceneNode>.GetDescendents(Root); } }
        [DataMember]
        public List<ISceneObject> SceneObjectList = new List<ISceneObject>();
        /// <summary>
        /// States whether the scene is currently performing a physics step.
        /// </summary>
        public bool InWorldStep { get; private set; }
        /// <summary>Root node to the scene graph.</summary>
        [DataMember]
        public SceneNode Root { get; private set; }
        [DataMember]
        public double Time { get; private set; }

        #region Constructors
        public Scene()
        {
            Root = new SceneNode(this);
            Root.WorldTransform = new Transform2();
            Root.WorldVelocity = Transform2.CreateVelocity();
            SetWorld(new World(new Xna.Vector2(0f, 0f)));
        }
        #endregion

        public void Step()
        {
            Step(1 / (float)Controller.StepsPerSecond);
        }

        class ActorPrev
        {
            public IActor Actor;
            public Transform2 Previous;
            public Transform2 TrueVelocity;
        }

        public void Step(float stepSize)
        {
            Debug.Assert(stepSize >= 0, "Simulation step size cannot be negative.");
            World.ProcessChanges();

            /*foreach (IPortal p in GetPortalList())
            {
                p.WorldTransformPrevious = p.GetWorldTransform();
            }*/

            foreach (IStep s in GetAll().OfType<IStep>())
            {
                s.StepBegin(this, stepSize);
            }
            PortalCommon.UpdateWorldTransform(this, true);
            if (World != null && stepSize > 0)
            {
                List<ActorPrev> actorTemp = new List<ActorPrev>();
                foreach (IActor actor in GetAll().OfType<IActor>())
                {
                    ActorPrev actorPrev = new ActorPrev();
                    actorTemp.Add(actorPrev);
                    actorPrev.Actor = actor;
                    actorPrev.Previous = actor.GetTransform();
                }

                //Perform physics step.
                {
                    _contactListener.StepBegin();
                    InWorldStep = true;
                    World.Step(stepSize);
                    InWorldStep = false;
                    _contactListener.StepEnd();

                    PortalCommon.UpdateWorldTransform(this, true);
                }

                //Replace each actor's velocity with the actor's displacement.
                foreach (ActorPrev prev in actorTemp)
                {
                    prev.TrueVelocity = prev.Actor.GetVelocity();
                    Transform2 velocity = prev.Actor.GetTransform().Minus(prev.Previous).Multiply(1 / stepSize);
                    prev.Actor.SetVelocity(velocity);

                    prev.Actor.SetTransform(prev.Previous);
                }


                PortalCommon.UpdateWorldTransform(this, false, true);
                SimulationStep.Step(GetAll().OfType<IPortalable>(), GetAll().OfType<IPortal>(), stepSize, (EnterCallbackData data) => {
                    IActor actor = data.Instance as IActor;
                    if (actor != null)
                    {
                        ActorPrev prev = actorTemp.Find(item => item.Actor == actor);
                        prev.TrueVelocity = Portal.EnterVelocity(data.EntrancePortal, (float)data.PortalT, prev.TrueVelocity);
                    }
                });

                //Reset the actor's velocity.
                foreach (ActorPrev prev in actorTemp)
                {
                    prev.Actor.SetVelocity(prev.TrueVelocity);
                }
            }

            foreach (IStep s in GetAll().OfType<IStep>())
            {
                s.StepEnd(this, stepSize);
            }
            Time += stepSize;
        }

        public ICamera2 GetCamera()
        {
            return ActiveCamera;
        }

        public List<ISceneObject> GetAll()
        {
            HashSet<ISceneObject> set = new HashSet<ISceneObject>();
            set.UnionWith(SceneObjectList);
            set.UnionWith(SceneNodeList);
            if (ActiveCamera != null)
            {
                set.Add(ActiveCamera);
            }
            return set.ToList();
        }

        public List<IRenderable> GetRenderList()
        {
            return SceneNodeList.OfType<IRenderable>().ToList();
        }

        public List<IPortal> GetPortalList()
        {
            return GetAll().OfType<IPortal>().ToList();
        }

        public List<IPortalable> GetPortalableList()
        {
            return GetAll().OfType<IPortalable>().ToList();
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
            _contactListener = new PhyicsListener(this);
        }
    }
}
