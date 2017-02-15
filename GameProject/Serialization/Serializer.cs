using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml;

namespace Game.Serialization
{
    public class Serializer
    {
        public const string FileExtension = "save";
        public const string FileExtensionName = "Save File";

        public Serializer()
        {
        }

        public void Serialize(Scene scene, Stream stream)
        {
            _serialize(scene, stream);
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

        void _serialize(Scene scene, string filename)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                NewLineOnAttributes = false,
                OmitXmlDeclaration = true
            };
            using (XmlWriter writer = XmlWriter.Create(filename, settings))
            {
                GetSerializer().WriteObject(writer, scene);
            }
        }

        void _serialize(Scene scene, Stream stream)
        {
            GetSerializer().WriteObject(stream, scene);
        }

        public Scene Deserialize(string filename)
        {
            var settings = new XmlReaderSettings();
            using (XmlReader reader = XmlReader.Create(filename, settings))
            {
                return (Scene)GetSerializer().ReadObject(reader);
            }
        }

        DataContractSerializer GetSerializer()
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
