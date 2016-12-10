﻿using Game;
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
        /// <summary>
        /// Remote id of the client.
        /// </summary>
        [DataMember]
        public long ClientId;
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

        public TankData(long clientId, Tank tank)
        {
            ClientId = clientId;
            Transform = tank.Actor.GetTransform();
            WorldTransform = tank.Actor.WorldTransform;
            Velocity = tank.Actor.Velocity;
            WorldVelocity = tank.Actor.WorldVelocity;
            TurretTransform = tank.Turret.GetTransform();
            TurretWorldTransform = tank.Turret.GetWorldTransform();

            GunFiredTime = tank.GunFiredTime;
        }

        public void UpdateTank(Tank tank)
        {
            tank.Actor.SetTransform(Transform);
            tank.Actor.SetVelocity(Velocity);
            tank.Actor.WorldTransform = WorldTransform;
            tank.Actor.WorldVelocity = WorldVelocity;

            foreach (SceneNode node in tank.Actor.Children)
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