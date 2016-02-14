using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Game
{
    public class Serializer
    {
        const string fileSuffixPhysics = "_phys";
        public const string fileExtension = "save";
        public const string fileExtensionName = "Save File";

        /*public Serializer()
        {
        }

        public void Serialize(SceneNode rootNode, string filename, string filenamePhysics)
        {
            SceneNode clone = CopyData(rootNode);
            _serialize(clone.Scene, filename, filenamePhysics);
        }

        private SceneNode CopyData(SceneNode rootNode)
        {
            SceneNode clone = rootNode.DeepClone(new Scene());
            clone.Scene.World.ProcessChanges();
            return clone;
        }

        public void SerializeAsync(SceneNode rootNode, string filename, string filenamePhysics)
        {
            SceneNode clone = CopyData(rootNode);
            ThreadPool.QueueUserWorkItem(
                new WaitCallback(delegate(object state)
                {
                    _serialize(clone.Scene, filename, filenamePhysics);
                }), null);
        }

        private void _serialize(Scene scene, string filename, string filenamePhysics)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = false;
            settings.OmitXmlDeclaration = true;
            WorldSerializer.Serialize(scene.World, filenamePhysics);
            using (XmlWriter writer = XmlWriter.Create(filename, settings))
            {
                GetSerializer().WriteObject(writer, scene);
            }
        }

        public void Deserialize(Scene scene, string filename, string filenamePhysics)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            using (XmlReader reader = XmlReader.Create(filename, settings))
            {
                Scene sceneNew = (Scene)GetSerializer().ReadObject(reader);
                World world = WorldSerializer.Deserialize(filenamePhysics);
                sceneNew.SetWorld(world);

                List<Actor> actorList = sceneNew.FindByType<Actor>();
                foreach (Body body in sceneNew.World.BodyList)
                {
                    BodyUserData userData = BodyExt.GetUserData(body);
                    Actor actor = actorList.Find(item => (item.BodyId == userData.BodyId));
                    Debug.Assert(actor != null, "Actor is missing.");
                    BodyExt.SetUserData(body, actor);
                    actor.SetBody(body);
                    actorList.Remove(actor);
                }

                Debug.Assert(actorList.Count == 0);
                Debug.Assert(sceneNew.Root.Children.Count == 1);
                sceneNew.Root.Children[0].DeepClone(scene);
            }
        }

        private DataContractSerializer GetSerializer()
        {
            return new DataContractSerializer(typeof(SceneNode), "Game", "Game", GetKnownTypes(),
            0x7FFFF,
            false,
            true,
            null);
        }

        protected virtual IEnumerable<Type> GetKnownTypes()
        {
            return from t in Assembly.GetExecutingAssembly().GetTypes()
                   where Attribute.IsDefined(t, typeof(DataContractAttribute))
                   select t;
        }*/
    }
}
