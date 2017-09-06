using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Game;
using System.Runtime.Serialization;
using OpenTK;

namespace CustomDebugVisualizer
{
    public static class Serializer
    {
        public static void Serialize(object data, Stream stream)
        {
            GetSerializer().WriteObject(stream, data);
        }

        public static object Deserialize(Stream stream)
        {
            return GetSerializer().ReadObject(stream);
        }

        static DataContractSerializer GetSerializer()
        {
            return new DataContractSerializer(
                typeof(object),
                "Game",
                "Game", 
                GetKnownTypes(),
                0x7FFFF,
                false,
                true,
                null);
        }

        static IEnumerable<Type> GetKnownTypes()
        {
            var a = from t in Assembly.Load(nameof(Game)).GetTypes()
                   where Attribute.IsDefined(t, typeof(DataContractAttribute))
                   select t;
            var b = a.Concat(from t in Assembly.Load(nameof(GameTests)).GetTypes()
                            where Attribute.IsDefined(t, typeof(DataContractAttribute))
                            select t);
            return b;
        }
    }
}
