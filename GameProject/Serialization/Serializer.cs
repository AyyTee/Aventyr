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

        public void Serialize(Scene scene, string filename)
        {
            _serialize(scene, filename);
        }

        public void Serialize<T>(Stream stream, T data)
        {
            GetSerializer().WriteObject(stream, data);
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

        public T Deserialize<T>(string filename)
        {
            var settings = new XmlReaderSettings();
            using (XmlReader reader = XmlReader.Create(filename, settings))
            {
                return (T)GetSerializer().ReadObject(reader);
            }
        }

        DataContractSerializer GetSerializer()
        {
            return new DataContractSerializer(
                typeof(object), 
                "Game", 
                "Game", 
                GetKnownTypes(),
                int.MaxValue,
                false,
                true,
                null);
        }

        protected virtual IEnumerable<Type> GetKnownTypes()
        {
            return typeof(Scene).Assembly
                .GetTypes()
                .Where(item => Attribute.IsDefined(item, typeof(DataContractAttribute)));
        }
    }
}
