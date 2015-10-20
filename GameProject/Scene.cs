using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using FarseerPhysics.Dynamics;
using Xna = Microsoft.Xna.Framework;
using System.Diagnostics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;
using Poly2Tri;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;

namespace Game
{
    public class Scene
    {
        //public Dictionary<ResourceID<Portal>, Portal> portalMap = new Dictionary<ResourceID<Portal>, Portal>();
        //public Dictionary<ResourceID<Entity>, Entity> entityMap = new Dictionary<ResourceID<Entity>, Entity>();
        [IgnoreDataMemberAttribute]
        public World PhysWorld;
        public float TimeStepSize = 1 / 60f;
        private Camera _activeCamera;
        public Camera ActiveCamera
        {
            get { return _activeCamera; }
            set { _activeCamera = value; }
        }
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
            PhysWorld = new World(new Xna.Vector2(0f, -9.8f/2));
        }

        public void Step()
        {
            PhysWorld.Step(TimeStepSize);
            foreach (Entity e in EntityList)
            {
                e.Step();
            }
        }

        private void AddEntity(Entity entity)
        {
            Debug.Assert(!EntityList.Exists(item => item.Equals(entity)), "This entity has already been added to this scene.");
            EntityList.Add(entity);
        }

        public Entity CreateEntity()
        {
            return CreateEntity(new Vector2(0, 0));
        }

        public Entity CreateEntity(Vector2 position)
        {
            Entity entity = new Entity(this, position);
            //AddEntity(entity);
            Debug.Assert(!EntityList.Exists(item => item.Equals(entity)), "This entity has already been added to this scene.");
            EntityList.Add(entity);
            return entity;
        }

        public Entity CreateEntityBox(Vector2 position, Vector2 scale)
        {
            Entity box = CreateEntity();
            box.Transform.Position = position;
            box.Models.Add(Model.CreatePlane(scale));

            Body body = BodyFactory.CreateRectangle(box.Scene.PhysWorld, scale.X, scale.Y, 1);
            body.Position = VectorExt2.ConvertToXna(position);
            box.LinkBody(body);
            body.BodyType = BodyType.Dynamic;

            //body.CreateFixture(new CircleShape(1f, 1f));*/

            return box;
        }

        public Entity CreateEntityPolygon(Vector2 position, Vector2 scale, Vector2[] vertices)
        {
            Entity entity = CreateEntity();
            entity.Transform.Position = position;

            Polygon polygon = PolygonFactory.CreatePolygon(vertices);

            entity.Models.Add(Model.CreatePolygon(polygon));

            Xna.Vector2 vPos = VectorExt2.ConvertToXna(entity.Transform.Position);

            List<FarseerPhysics.Common.Vertices> vList = new List<FarseerPhysics.Common.Vertices>();

            Body body = new Body(this.PhysWorld);
            body.Position = VectorExt2.ConvertToXna(position);
            for (int i = 0; i < polygon.Triangles.Count; i++)
            {
                var v1 = new FarseerPhysics.Common.Vertices();

                for (int j = 0; j < polygon.Triangles[i].Points.Count(); j++)
                {
                    v1.Add(VectorExt2.ConvertToXna(polygon.Triangles[i].Points[j]));
                }

                vList.Add(v1);
                PolygonShape shape = new PolygonShape(v1, 1);
                Fixture fixture = body.CreateFixture(shape);
                FixtureUserData userData = new FixtureUserData(fixture);
                for (int j = 0; j < polygon.Triangles[i].Neighbors.Count(); j++)
                {
                    //userData.EdgeIsExterior[j] = polygon.Triangles[i].NeighborAcrossFrom(polygon.Triangles[i].Points[j]) == null;
                    //polygon.Triangles[i].

                    userData.EdgeIsExterior[j] = polygon.Triangles[i].EdgeIsConstrained[(j + 2) % 3];
                }
            }

            entity.LinkBody(body);

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

        public Portal CreatePortal()
        {
            return CreatePortal(new Vector2(0, 0));
        }

        public Portal CreatePortal(Vector2 position)
        {
            Portal portal = new Portal(this, position);
            //AddPortal(portal);
            Debug.Assert(!PortalList.Exists(item => item.Equals(portal)), "This portal has already been added to this scene.");
            PortalList.Add(portal);
            return portal;
        }

        public void RemovePortal(Portal portal)
        {
            PortalList.Remove(portal);
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
            var physSerializer = new FarseerPhysics.Common.WorldXmlSerializer();
            physSerializer.Serialize(PhysWorld, physicsFile);
            
            /*string path = "filepath";
            FileStream outFile = File.Create(path);*/
            /*XmlSerializer formatter = new XmlSerializer(GetType());
            formatter.Serialize(outFile, this);*/
            
            Assembly assembly = Assembly.GetAssembly(GetType());//Assembly.Load(new AssemblyName());
            var types = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.IsSubclassOf(GetType())
                        select t;
            DataContractSerializer serializer = new DataContractSerializer(GetType(), "Game", "Game", types,
            0x7FFF /*maxObjectsInGraph*/,
            false/*ignoreExtensionDataObject*/,
            true/*preserveObjectReferences*/,
            null/*dataContractSurrogate*/,
            new PhysDataContractResolver(assembly));

            serializer.WriteObject(sceneFile, this);
        }

        /// <summary>
        /// Creates a deep copy of this scene.
        /// </summary>
        /*public Scene Copy()
        {
            Scene scene = new Scene();
            //var a = new Body(PhysWorld);
            foreach (Portal p in PortalList)
            {

            }
            foreach (Entity e in EntityList)
            {

            }
        }*/
    }
}
