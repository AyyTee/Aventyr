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
        public const string fileExtension = "xml";
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
                /*EditorScene saveScene = new EditorScene();
                EditorClone.Clone(scene, saveScene);*/
                GetSerializer().WriteObject(writer, scene);
            }
        }

        public static EditorScene Deserialize(string filename)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            using (XmlReader reader = XmlReader.Create(filename, settings))
            {
                EditorScene editorScene = (EditorScene)GetSerializer().ReadObject(reader);
                foreach (EditorObject e in editorScene.GetAll().OfType<EditorObject>())
                {
                    e.Initialize();
                }
                return editorScene;
            }
        }
    }
}
