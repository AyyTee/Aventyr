using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TankGame
{
    [DataContract]
    public class TankInput
    {
        [DataMember]
        public bool MoveFoward;
        [DataMember]
        public bool MoveBackward;
        [DataMember]
        public bool TurnLeft;
        [DataMember]
        public bool TurnRight;
        /// <summary>
        /// Aiming reticle position in world coordinates.
        /// </summary>
        [DataMember]
        public Vector2 ReticlePos;
        [DataMember]
        public bool FirePortal0;
        [DataMember]
        public bool FirePortal1;
        [DataMember]
        public bool FireGun;
    }
}
