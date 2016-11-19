using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TankGame.Network
{
    [DataContract]
    public abstract class Message
    {
        /// <summary>
        /// Used to keep track of message order.  Newer messages have larger ids than older messages.
        /// </summary>
        [DataMember]
        public int MessageId;
    }
}
