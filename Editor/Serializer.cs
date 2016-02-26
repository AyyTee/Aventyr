using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Editor
{
    public static class Serializer
    {
        const string fileSuffixPhysics = "_phys";
        public const string fileExtension = "save";
        public const string fileExtensionName = "Save File";

        private static IEnumerable<Type> GetKnownTypes()
        {
            var types = from t in Assembly.GetAssembly(typeof(Scene)).GetTypes()
                   where Attribute.IsDefined(t, typeof(DataContractAttribute))
                   select t;

            var editorTypes = from t in Assembly.GetExecutingAssembly().GetTypes()
                   where Attribute.IsDefined(t, typeof(DataContractAttribute))
                   select t;
            List<Type> typeList = types.ToList();
            typeList.AddRange(editorTypes);

            return typeList;
        }

        private static DataContractSerializer GetSerializer()
        {
            return new DataContractSerializer(typeof(EditorScene), "Game", "Game", GetKnownTypes(),
            0x7FFFF,
            false,
            true,
            null);
        }

        public static void Serialize(EditorScene scene, string filename)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = false;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(filename, settings))
            {
                EditorScene saveScene = new EditorScene(new Scene());
                EditorClone.Clone(scene, saveScene);
                GetSerializer().WriteObject(writer, saveScene);
            }
        }

        public static EditorScene Deserialize(Scene scene, string filename)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            using (XmlReader reader = XmlReader.Create(filename, settings))
            {
                return (EditorScene)GetSerializer().ReadObject(reader);
            }
        }
    }
}
