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
        [DataMember]
        public int MessageId;
        [DataMember]
        public int StepCount;
        [DataMember]
        public double LocalSendTime;
    }
}
