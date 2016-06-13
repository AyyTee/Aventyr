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
    public class Scene : IRenderLayer, IScene
    {
        public World World { get; private set; }
        PhyicsListener _contactListener;

        public ICamera2 ActiveCamera { get; private set; }
        public List<SceneNode> SceneNodeList { get { return Tree<SceneNode>.GetDescendents(Root); } }
        public List<Entity> EntityList { get { return Tree<SceneNode>.FindByType<Entity>(Root); } }
        /// <summary>
        /// States whether the scene is currently performing a physics step.
        /// </summary>
        public bool InWorldStep { get; private set; }
        /// <summary>Root node to the scene graph.</summary>
        [DataMember]
        public SceneNode Root { get; private set; }
        [DataMember]
        public float Time { get; private set; }

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
            Debug.Assert(stepSize >= 0, "Simulation step size cannot be negative.");
            Time += stepSize;
            World.ProcessChanges();

            foreach (IStep s in GetAll().OfType<IStep>())
            {
                s.StepBegin(stepSize);
            }

            if (World != null && stepSize > 0)
            {
                List<ProxyPortalable> portalablePrevList = new List<ProxyPortalable>();
                foreach (IPortalable s in SceneNodeList.OfType<IPortalable>())
                {
                    if (s is IPortal)
                    {
                        continue;
                    }
                    ProxyPortalable proxy = new ProxyPortalable(s);
                    portalablePrevList.Add(proxy);
                }

                List<ProxyPortal> portalPrevList = Portal.CreateProxies(GetPortalList());

                _contactListener.StepBegin();
                InWorldStep = true;
                World.Step(stepSize);
                InWorldStep = false;
                _contactListener.StepEnd();

                foreach (ProxyPortalable s in portalablePrevList.FindAll(item => item.Portalable is Actor))
                {
                    s.TrueVelocity = s.Portalable.GetVelocity();//.Multiply(stepSize);
                    s.Velocity = s.Portalable.GetTransform().Minus(s.Transform);//.Multiply(1 / stepSize);
                }

                SimulationStep.Step(portalPrevList, portalablePrevList, 1, stepSize);

                /*foreach (SceneNode s in SceneNodeList)
                {
                    //Skip Actors, they handle movement using rigid body physics.
                    if (s is IActor)
                    {
                        continue;
                    }
                    if (s is IPortalable)
                    {
                        IPortalable portalable = (IPortalable)s;
                        //Parented SceneNodes can't perform portal teleportation directly.
                        if (s.Parent == s.Scene.Root)
                        {
                            //SceneExt.RayCast(portalable, GetPortalList(), stepSize);
                            SceneExt.RayCast(portalable, portalPrevList.OfType<IPortal>(), stepSize);
                        }
                        else
                        {
                            portalable.SetTransform(portalable.GetTransform().Add(portalable.GetVelocity().Multiply(stepSize)));
                        }
                    }
                }*/
            }

            foreach (IStep s in GetAll().OfType<IStep>())
            {
                s.StepEnd(stepSize);
            }
        }

        public ICamera2 GetCamera()
        {
            return ActiveCamera;
        }

        public List<ISceneObject> GetAll()
        {
            HashSet<ISceneObject> set = new HashSet<ISceneObject>();
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
