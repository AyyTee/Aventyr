using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game.Serialization
{
    public static class Serializer
    {
        public static string Serialize<T>(T data)
        {
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Converters = new[] { new Vector2Converter() }
            });
        }

        public static T Deserialize<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        class Vector2Converter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Vector2);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var temp = JObject.Load(reader);
                return new OpenTK.Vector2(((float?)temp["X"]).GetValueOrDefault(), ((float?)temp["Y"]).GetValueOrDefault());
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var v = (Vector2)value;
                serializer.Serialize(writer, new { X = v.X, Y = v.Y });
            }
        }
    }
}
