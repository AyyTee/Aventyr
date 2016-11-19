using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TankGame
{
    [DataContract]
    public class TankData
    {
        [DataMember]
        public long ClientId;
        [DataMember]
        public Transform2 Transform;
        [DataMember]
        public Transform2 Velocity;
    }
}
