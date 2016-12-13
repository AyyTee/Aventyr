using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TankGame.Network
{
    [DataContract]
    public class TankData
    {
        /// <summary>
        /// Remote id of the client.
        /// </summary>
        [DataMember]
        public long OwnerId;
        [DataMember]
        public int ServerId;
        [DataMember]
        public Transform2 Transform;
        [DataMember]
        public Transform2 WorldTransform;
        [DataMember]
        public Transform2 Velocity;
        [DataMember]
        public Transform2 WorldVelocity;
        [DataMember]
        public Transform2 TurretTransform;
        [DataMember]
        public Transform2 TurretWorldTransform;
        [DataMember]
        public double GunFiredTime;

        public TankData()
        {
        }

        public TankData(long ownerId, Tank tank)
        {
            OwnerId = ownerId;
            ServerId = (int)tank.ServerId;
            Transform = tank.GetTransform();
            WorldTransform = tank.WorldTransform;
            Velocity = tank.GetVelocity();
            WorldVelocity = tank.WorldVelocity;
            TurretTransform = tank.Turret.GetTransform();
            TurretWorldTransform = tank.Turret.GetWorldTransform();

            GunFiredTime = tank.GunFiredTime;
        }

        public void UpdateTank(Tank tank)
        {
            NetworkHelper.SetServerId(tank, ServerId);
            tank.SetTransform(Transform);
            tank.SetVelocity(Velocity);
            tank.WorldTransform = WorldTransform;
            tank.WorldVelocity = WorldVelocity;

            foreach (SceneNode node in tank.Children)
            {
                node.WorldTransform = WorldTransform;
                node.WorldVelocity = WorldVelocity;
            }

            tank.Turret.SetTransform(TurretTransform);
            tank.Turret.WorldTransform = TurretWorldTransform;

            tank.GunFiredTime = GunFiredTime;
        }
    }
}
