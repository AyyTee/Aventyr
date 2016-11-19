using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;

namespace TankGame.Network
{
    public class NetworkSerializer
    {
        public static MemoryStream Serialize<T>(T data)
        {
            MemoryStream stream = new MemoryStream();
            new DataContractJsonSerializer(typeof(T)).WriteObject(stream, data);
            return stream;
        }

        public static T Deserialize<T>(byte[] data)
        {
            return (T)new DataContractJsonSerializer(typeof(T))
                .ReadObject(new MemoryStream(data));
        }
    }
}
