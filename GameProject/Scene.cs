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
        [NonSerialized]
        private World _physWorld;

        public World PhysWorld
        {
            get { return _physWorld; }
        }
        private int _idCount = 0;
        /// <summary>
        /// Number of unique ids that have been used for objects in the scene
        /// </summary>
        public int IdCount
        {
            get { return _idCount; }
        }
        [NonSerialized]
        private PhysContactListener _contactListener;


        public float TimeStepSize = 1 / 60f;
        public Camera ActiveCamera { get; set; }
        private List<Portal> _portalList = new List<Portal>();
        public List<Portal> PortalList
        {
            get { return _portalList; }
        }
        private List<Entity> _entityList = new List<Entity>();
        public List<Entity> EntityList
        {
            get { return _entityList; }
        }

        public Scene()
        {
            SetPhysicsWorld(new World(new Xna.Vector2(0f, 0f)));
        }

        public void Step()
        {
            PhysWorld.ProcessChanges();
            foreach (Entity e in EntityList)
            {
                if (e.Body != null)
                {
                    Xna.Vector2 v0 = Vector2Ext.ConvertToXna(e.Transform.WorldPosition);
                    if (v0 != e.Body.Position || e.Transform.Rotation != e.Body.Rotation)
                    {
                        e.Body.SetTransform(v0, e.Transform.WorldRotation);
                    }
                }
            }
            if (PhysWorld != null)
            {
                _contactListener.StepBegin();
                PhysWorld.Step(TimeStepSize);
                _contactListener.StepEnd();
            }

            foreach (Entity e in EntityList)
            {
                e.Step();
            }
        }

        private void AddEntity(Entity entity)
        {
            Debug.Assert(!EntityList.Exists(item => item.Equals(entity)), "This entity has already been added to this scene.");
            EntityList.Add(entity);
            _idCount++;
        }

        private void AddPortal(FixturePortal portal)
        {
            Debug.Assert(!PortalList.Exists(item => item.Equals(portal)), "This portal has already been added to this scene.");
            PortalList.Add(portal);
            _idCount++;
        }

        public Entity GetEntityById(int id)
        {
            return EntityList.Find(item => (item.Id == id));
        }

        public Entity GetEntityByName(string name)
        {
            return EntityList.Find(item => (item.Name == name));
        }

        public Entity CreateEntity()
        {
            return CreateEntity(new Vector2(0, 0));
        }

        public Entity CreateEntity(Vector2 position)
        {
            Entity entity = new Entity(this, position);
            AddEntity(entity);
            return entity;
        }

        public Entity CreateEntityBox(Vector2 position, Vector2 scale)
        {
            Entity box = CreateEntity();
            box.IsPortalable = true;
            box.Transform.Position = position;
            box.Models.Add(Model.CreatePlane(scale));

            Body body = BodyFactory.CreateRectangle(box.Scene.PhysWorld, scale.X, scale.Y, 1);
            body.Position = Vector2Ext.ConvertToXna(position);
            box.SetBody(body);
            body.BodyType = BodyType.Dynamic;

            FixtureUserData userData = new FixtureUserData(body.FixtureList[0]);
            
            FixtureExt.SetUserData(body.FixtureList[0], userData);
            return box;
        }

        public Entity CreateEntityPolygon(Vector2 position, Vector2 scale, Vector2[] vertices)
        {
            Entity entity = CreateEntity();
            entity.Transform.Position = position;
            vertices = MathExt.SetHandedness(vertices, false);
            Polygon polygon = PolygonFactory.CreatePolygon(vertices);

            entity.Models.Add(Model.CreatePolygon(polygon));

            Xna.Vector2 vPos = Vector2Ext.ConvertToXna(entity.Transform.Position);

            List<FarseerPhysics.Common.Vertices> vList = new List<FarseerPhysics.Common.Vertices>();

            Body body = new Body(this.PhysWorld);
            body.Position = Vector2Ext.ConvertToXna(position);
            for (int i = 0; i < polygon.Triangles.Count; i++)
            {
                var v1 = new FarseerPhysics.Common.Vertices();

                for (int j = 0; j < polygon.Triangles[i].Points.Count(); j++)
                {
                    v1.Add(Vector2Ext.ConvertToXna(polygon.Triangles[i].Points[j]));
                }

                vList.Add(v1);
                PolygonShape shape = new PolygonShape(v1, 1);
                Fixture fixture = body.CreateFixture(shape);
                FixtureUserData userData = new FixtureUserData(fixture);
                FixtureExt.SetUserData(fixture, userData);
                for (int j = 0; j < polygon.Triangles[i].Neighbors.Count(); j++)
                {
                    userData.EdgeIsExterior[j] = polygon.Triangles[i].EdgeIsConstrained[(j + 2) % 3];
                }
            }
            
            entity.SetBody(body);

            return entity;
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
                    PhysWorld.RemoveBody(entity.Body);
                }
                //Entity.IDMap.Remove(entity.ID);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Assigns a physics world to this scene. Can only be done if there isn't already a physics world assigned.
        /// </summary>
        public void SetPhysicsWorld(World world)
        {
            Debug.Assert(PhysWorld == null, "A physics world has already been assigned to this scene.");
            _physWorld = world;
            PhysWorld.ProcessChanges();
            _contactListener = new PhysContactListener(this);
            
            foreach (Body body in PhysWorld.BodyList)
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
            physicsSerializer.Serialize(PhysWorld, physicsFile);

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
