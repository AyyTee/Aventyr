using Game;
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
    public class MessageToClient : Message
    {
        [DataMember]
        public TankData[] TankData = new TankData[0];
        [DataMember]
        public double SceneTime;
        [DataMember]
        public WallAdded[] WallsAdded = new WallAdded[0];
        [DataMember]
        public BulletData[] BulletData = new BulletData[0];
    }
}
