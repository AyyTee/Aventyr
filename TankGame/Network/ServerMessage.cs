using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TankGame.Network
{
    /// <summary>
    /// A message sent from the server.
    /// </summary>
    [DataContract]
    public class ServerMessage : Message
    {
        [DataMember]
        public TankData[] TankData;
        [DataMember]
        public long ClientId;
    }
}
