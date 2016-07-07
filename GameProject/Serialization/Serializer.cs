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
        public const string fileExtension = "save";
        public const string fileExtensionName = "Save File";

        public Serializer()
        {
        }

        public void Serialize(Scene scene, string filename)
        {
            _serialize(scene, filename);
        }

        public void Serialize(SceneNode rootNode, string filename)
        {
            SceneNode clone = null;// CopyData(rootNode);
            _serialize(clone.Scene, filename);
        }

        /*private SceneNode CopyData(SceneNode rootNode)
        {
            SceneNode clone = rootNode.DeepClone(new Scene());
            clone.Scene.World.ProcessChanges();
            return clone;
        }*/

        public void SerializeAsync(SceneNode rootNode, string filename)
        {
            SceneNode clone = null;// CopyData(rootNode);
            ThreadPool.QueueUserWorkItem(
                new WaitCallback(delegate(object state)
                {
                    _serialize(clone.Scene, filename);
                }), null);
        }

        private void _serialize(Scene scene, string filename)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = false;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(filename, settings))
            {
                GetSerializer().WriteObject(writer, scene);
            }
        }

        public Scene Deserialize(string filename)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            using (XmlReader reader = XmlReader.Create(filename, settings))
            {
                return (Scene)GetSerializer().ReadObject(reader);
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
        }
    }
}
