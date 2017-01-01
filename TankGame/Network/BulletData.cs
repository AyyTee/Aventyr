using Game;
using System.Runtime.Serialization;
using Game.Common;

namespace TankGame.Network
{
    [DataContract]
    public class BulletData
    {
        [DataMember]
        public int ServerId;
        [DataMember]
        public Transform2 Transform;
        [DataMember]
        public Transform2 Velocity;

        public BulletData(Bullet bullet)
        {
            Transform = bullet.GetTransform();
            Velocity = bullet.GetVelocity();
            ServerId = (int)bullet.ServerId;
        }

        public void UpdateBullet(Bullet bullet)
        {
            NetworkHelper.SetServerId(bullet, ServerId);
            bullet.SetTransform(Transform);
            bullet.WorldTransform = Transform;
            bullet.SetVelocity(Velocity);
            bullet.WorldVelocity = Velocity;
            bullet.Children[0].WorldTransform = Transform;
            bullet.Children[0].WorldVelocity = Velocity;
        }
    }
}