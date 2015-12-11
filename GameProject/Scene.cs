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
        /// <summary>
        /// Number of unique ids that have been used for objects in the scene
        /// </summary>
        public int IdCount { get; private set; }
        [NonSerialized]
        private PhysContactListener _contactListener;

        public Camera ActiveCamera { get; set; }
        public List<Portal> PortalList { get; private set; }
        public List<Entity> EntityList { get; private set; }

        #region constructors
        public Scene()
        {
            IdCount = 0;
            PortalList = new List<Portal>();
            EntityList = new List<Entity>();
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
            foreach (Entity e in EntityList)
            {
                e.PositionUpdate();
                if (e.Body != null)
                {
                    Xna.Vector2 v0 = Vector2Ext.ConvertToXna(e.Transform.WorldPosition);
                    e.Body.SetTransform(v0, e.Transform.WorldRotation);
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

            foreach (Entity e in EntityList)
            {
                e.Step();
            }
        }

        public void AddEntity(Entity entity)
        {
            Debug.Assert(!EntityList.Exists(item => item.Equals(entity)), "This entity has already been added to this scene.");
            EntityList.Add(entity);
            IdCount++;
        }

        public Entity GetEntityById(int id)
        {
            return EntityList.Find(item => item.Id == id);
        }

        public Entity GetEntityByName(string name)
        {
            return EntityList.Find(item => (item.Name == name));
        }

        /// <summary>
        /// Remove entity and the entity's linked physics body from the scene.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        /// <returns>If the entity existed within the scene.</returns>
        public bool RemoveEntity(Entity entity)
        {
            if (EntityList.Remove(entity))
            {
                if (entity.Body != null)
                {
                    World.RemoveBody(entity.Body);
                }
                //Entity.IDMap.Remove(entity.ID);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove portal from the scene.
        /// </summary>
        public bool RemovePortal(Portal portal)
        {
            return PortalList.Remove(portal);
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
            
            foreach (Body body in World.BodyList)
            {
                var userData = ((List<BodyUserData>)body.UserData)[0];
                Entity entity = EntityList.Find(item => (item.Id == userData.EntityID));
                BodyExt.SetUserData(body, entity);
                entity.BodyId = body.BodyId;
            }
        }

        public void Save()
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
        }

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
