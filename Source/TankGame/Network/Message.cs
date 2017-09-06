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
        /// A unique incrementing id.  A message will be more recent than another message if it has 
        /// a larger id and they are from the same source.
        /// </summary>
        [DataMember]
        public int MessageId;
        [DataMember]
        public double LocalSendTime;
    }
}
