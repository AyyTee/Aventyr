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
        /// A unique incrementing id.  A message with a larger id than another message from the same source will be more recent.
        /// </summary>
        [DataMember]
        public int MessageId;
        [DataMember]
        public int StepCount;
        [DataMember]
        public double LocalSendTime;
    }
}
