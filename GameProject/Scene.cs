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
    [Serializable]
    public class Scene
    {
        public World World { get; private set; }
        /// <summary>Number of unique ids that have been used for SceneNodes within this scene.</summary>
        int _idCount = 0;
        [NonSerialized]
        private PhysContactListener _contactListener;

        public Camera2D ActiveCamera { get; private set; }
        public List<SceneNode> SceneNodeList { get { return FindByType<SceneNode>(Root); } }
        public List<Portal> PortalList { get { return FindByType<Portal>(Root); } }
        public List<Entity> EntityList { get { return FindByType<Entity>(Root); } }
        /// <summary>Root node to the scene graph.</summary>
        public SceneNode Root { get; private set; }

        #region constructors
        public Scene()
        {
            Root = new SceneNode(this);
            SetPhysicsWorld(new World(new Xna.Vector2(0f, 0f)));
        }
        #endregion
        
        public void Step()
        {
            Step(1/(float)Controller.StepsPerSecond);
        }

        public void Step(float stepSize)
        {
            World.ProcessChanges();
            foreach (Actor e in FindByType<Actor>())
            {
                //e.PositionUpdate();
                if (e.Body != null)
                {
                    Xna.Vector2 v0 = Vector2Ext.ConvertToXna(e.GetWorldTransform().Position);
                    e.Body.SetTransform(v0, e.GetWorldTransform().Rotation);
                    e.Body.LinearVelocity = Vector2Ext.ConvertToXna(e.Velocity.Position);
                    e.Body.AngularVelocity = e.Velocity.Rotation;
                }
            }
            if (World != null)
            {
                _contactListener.StepBegin();
                World.Step(stepSize);
                _contactListener.StepEnd();
            }

            foreach (Actor e in FindByType<Actor>())
            {
                e.Step();
            }
        }

        public List<T> FindByType<T>() where T : SceneNode
        {
            return FindByType<T>(Root);
        }

        public List<T> FindByType<T>(SceneNode node) where T : SceneNode
        {
            List<T> list = new List<T>();
            foreach (SceneNode p in node.ChildList)
            {
                T nodeCast = p as T;
                if (nodeCast != null)
                {
                    list.Add(nodeCast);
                }
                list.AddRange(FindByType<T>(p));
            }
            return list;
        }

        public SceneNode FindByName(string name)
        {
            return SceneNodeList.Find(item => (item.Name == name));
        }

        /// <summary>
        /// Get a unique SceneNode id.
        /// </summary>
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
        public void SetPhysicsWorld(World world)
        {
            Debug.Assert(World == null, "A physics world has already been assigned to this scene.");
            World = world;
            World.ProcessChanges();
            _contactListener = new PhysContactListener(this);
            
            /*foreach (Body body in World.BodyList)
            {
                var userData = ((List<BodyUserData>)body.UserData)[0];
                Entity entity = EntityList.Find(item => (item.Id == userData.EntityID));
                BodyExt.SetUserData(body, entity);
                entity.BodyId = body.BodyId;
            }*/
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

        /*public void Save()
        {
            FileStream physicsFile = File.Create("savePhys.xml");
            FileStream sceneFile = File.Create("save.xml");
            Save(sceneFile, physicsFile);
            physicsFile.Close();
            sceneFile.Close();
        }

        public void Save(FileStream sceneFile, FileStream physicsFile)
        {
            var physicsSerializer = new FarseerPhysics.Common.WorldXmlSerializer();
            physicsSerializer.Serialize(World, physicsFile);

            DataContractSerializer serializer = GetSceneSerializer();

            serializer.WriteObject(sceneFile, this);
        }

        public static Scene Load()
        {
            FileStream physicsFile = File.OpenRead("savePhys.xml");
            FileStream sceneFile = File.OpenRead("save.xml");
            Scene scene = Load(sceneFile, physicsFile);
            physicsFile.Close();
            sceneFile.Close();
            return scene;
        }

        public static Scene Load(FileStream sceneFile, FileStream physicsFile)
        {
            DataContractSerializer serializer = GetSceneSerializer();
            Scene scene = (Scene)serializer.ReadObject(sceneFile);

            var physicsDeserializer = new FarseerPhysics.Common.WorldXmlDeserializer();
            World physWorld = physicsDeserializer.Deserialize(physicsFile);
            
            scene.SetPhysicsWorld(physWorld);

            return scene;
        }*/

        private static DataContractSerializer GetSceneSerializer()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Scene));
            var types = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.IsSubclassOf(typeof(Scene))
                        select t;
            DataContractSerializer serializer = new DataContractSerializer(typeof(Scene), "Game", "Game", types,
            0x7FFFF /*maxObjectsInGraph*/,
            false/*ignoreExtensionDataObject*/,
            true/*preserveObjectReferences*/,
            null/*dataContractSurrogate*/,
            null);//new PhysDataContractResolver(assembly));
            return serializer;
        }
    }
}
