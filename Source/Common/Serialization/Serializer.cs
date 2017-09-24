using Game.Rendering;
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
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new JsonConverter[] {
                    new Vector2Converter(),
                    new Vector3Converter(),
                    new ICamera2Converter(),
                }
            });
        }

        public static T Deserialize<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
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
                return new Vector2(((float?)temp["X"]).GetValueOrDefault(), ((float?)temp["Y"]).GetValueOrDefault());
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var v = (Vector2)value;
                serializer.Serialize(writer, new { X = v.X, Y = v.Y });
            }
        }

        class Vector3Converter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Vector3);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var temp = JObject.Load(reader);
                return new Vector3(
                    ((float?)temp["X"]).GetValueOrDefault(), 
                    ((float?)temp["Y"]).GetValueOrDefault(), 
                    ((float?)temp["Z"]).GetValueOrDefault());
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var v = (Vector3)value;
                serializer.Serialize(writer, new { X = v.X, Y = v.Y, Z = v.Z });
            }
        }

        class ICamera2Converter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                if (objectType == typeof(SimpleCamera2))
                {
                    return false;
                }
                return objectType.FindInterfaces((t, _) => t == typeof(ICamera2), null).Any();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                //var temp = JObject.Load(reader);
                return serializer.Deserialize(reader, typeof(SimpleCamera2));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var camera = new SimpleCamera2((ICamera2)value);
                serializer.Serialize(writer, camera, typeof(ICamera2));
            }
        }
    }
}
