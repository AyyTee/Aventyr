using Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Game.Common;

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
        public readonly PortalData[] PortalData = new PortalData[2];
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

            Debug.Assert(tank.PortalPair.Length == 2);
            for (int i = 0; i < tank.PortalPair.Length; i++)
            {
                PortalData[i] = new PortalData(tank.PortalPair[i]);
            }
        }

        public void UpdateTank(Tank tank, Scene scene)
        {
            Debug.Assert(tank.ServerId == ServerId || tank.ServerId == null);

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

            Debug.Assert(tank.PortalPair.Length == 2);
            for (int i = 0; i < tank.PortalPair.Length; i++)
            {
                PortalData[i]?.Update(tank.PortalPair[i], scene);
            }
        }
    }
}
