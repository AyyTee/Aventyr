using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TankGame.Network
{
    /// <summary>
    /// A message sent from a client.
    /// </summary>
    [DataContract]
    public class MessageToServer : Message
    {
        [DataMember]
        public TankInput Input;
    }
}
