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
                        SceneExt.RayCast(portalable, GetPortalList());
                    }
                    else
                    {
                        portalable.SetTransform(portalable.GetTransform().Add(portalable.GetVelocity()));
                    }
                }
            }
            if (World != null)
            {
                _contactListener.StepBegin();
                World.Step(stepSize);
                _contactListener.StepEnd();
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

        public List<IPortal> GetPortalList()
        {
            return Tree<SceneNode>.FindByType<SceneNode>(Root).OfType<IPortal>().ToList();
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
