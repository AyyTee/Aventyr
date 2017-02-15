using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.Common;

namespace TankGame.Network
{
    [DataContract]
    public class PortalData
    {
        [DataMember]
        public int ServerId;
        [DataMember]
        public int? WallServerId;
        [DataMember]
        public PolygonCoord Coord;
        [DataMember]
        public Transform2 WorldTransform;
        [DataMember]
        public Transform2 WorldVelocity;

        public PortalData(TankPortal portal)
        {
            Coord = (PolygonCoord) portal.Position;
            ServerId = (int)portal.ServerId;
            if (portal.Parent != null)
            {
                WallServerId = (int)((Wall)portal.Parent).ServerId;
            }
            
        }

        public void Update(TankPortal portal, Scene scene)
        {
            Debug.Assert(portal.ServerId == ServerId || portal.ServerId == null);

            if (WallServerId != null)
            {
                Wall parent = scene.GetAll().OfType<Wall>().First(item => item.ServerId == WallServerId);
                portal.SetPosition(new WallCoord(parent, Coord));
            }
            else
            {
                portal.SetPosition(null);
            }
            
            portal.WorldTransform = WorldTransform;
            portal.WorldVelocity = WorldVelocity;
        }
    }
}
