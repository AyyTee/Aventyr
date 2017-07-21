using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TimeLoopInc
{
    public static class Serializer
    {
        public static string Serialize<T>(T data)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            return JsonConvert.SerializeObject(data, settings);
        }

        public static T Deserialize<T>(string data)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            return JsonConvert.DeserializeObject<T>(data, settings);
        }
    }
}
