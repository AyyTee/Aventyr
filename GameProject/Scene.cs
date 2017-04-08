using System;
using OpenTK;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Dynamics;
using Xna = Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Runtime.Serialization;
using Game.Common;
using Game.Physics;
using Game.Portals;
using Game.Rendering;

namespace Game
{
    [DataContract]
    public class Scene : IRenderLayer, IScene
    {
        public World World { get; private set; }
        PhyicsListener _contactListener;
        public ICamera2 ActiveCamera { get; private set; }

        [DataMember]
        public List<ISceneObject> SceneObjects = new List<ISceneObject>();
        public readonly HashSet<ISceneObject> ToBeRemoved = new HashSet<ISceneObject>();
        /// <summary>
        /// Whether the scene is currently performing a physics step.  
        /// This is useful in cases where changing physics state can break FSE.
        /// </summary>
        public bool InWorldStep { get; private set; }
        public bool InStep { get; private set; }
        [DataMember]
        public double Time { get; set; }
        [DataMember]
        public Vector2 Gravity { get; set; } = new Vector2(0, -4.9f);

        public Scene()
        {
            SetWorld(new World(new Xna.Vector2(0f, 0f)));
        }

        class ActorPrev
        {
            public Actor Actor;
            public Transform2 Previous;
            public Transform2 TrueVelocity;
        }

        public void MarkForRemoval(ISceneObject sceneObject) => ToBeRemoved.Add(sceneObject);

        public void Step(float stepSize = 1 / (float)Controller.StepsPerSecond)
        {
            Debug.Assert(stepSize >= 0, "Simulation step size cannot be negative.");
            InStep = true;
            World.ProcessChanges();

            foreach (IStep s in GetAll().OfType<IStep>())
            {
                s.StepBegin(this, stepSize);
            }
            PortalCommon.UpdateWorldTransform(this, true);
            if (World != null && stepSize > 0)
            {
                var actorTemp = GetAll()
                    .OfType<Actor>()
                    .Select(actor => new ActorPrev
                    {
                        Actor = actor,
                        Previous = actor.GetTransform()
                    }).ToList();

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
                SimulationStep.Step(GetAll().OfType<IPortalCommon>(), GetAll().OfType<IPortal>(), stepSize, data => {
                    var actor = data.Instance as Actor;
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

            InStep = false;

            foreach (ISceneObject s in ToBeRemoved)
            {
                s.Remove();
                SceneObjects.Remove(s);
                if (ActiveCamera == s)
                {
                    ActiveCamera = null;
                }
            }
            ToBeRemoved.Clear();

            Time += stepSize;
        }

        public ICamera2 GetCamera() => ActiveCamera;

        public List<ISceneObject> GetAll()
        {
            HashSet<ISceneObject> set = new HashSet<ISceneObject>();
            set.UnionWith(SceneObjects);
            if (ActiveCamera != null)
            {
                set.Add(ActiveCamera);
            }
            return set.ToList();
        }

        public List<IRenderable> GetRenderList() => SceneObjects.OfType<IRenderable>().ToList();

        public List<IPortal> GetPortalList() => GetAll().OfType<IPortal>().ToList();

        public List<IPortalable> GetPortalableList() => GetAll().OfType<IPortalable>().ToList();

        public void SetActiveCamera(ICamera2 camera) => ActiveCamera = camera;

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
