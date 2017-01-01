using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Game.Common;

namespace TankGame.Network
{
    [DataContract]
    public class WallAdded
    {
        [DataMember]
        public int ServerId;
        [DataMember]
        public Vector2[] Vertices;
        [DataMember]
        public Transform2 Transform;

        public WallAdded(Wall wall)
        {
            ServerId = (int)wall.ServerId;
            Vertices = wall.Vertices.ToArray();
            Transform = wall.GetTransform();
        }

        public Wall WallCreate(Scene scene)
        {
            Wall wall = new Wall(scene, Vertices);
            NetworkHelper.SetServerId(wall, ServerId);
            wall.SetTransform(Transform);
            return wall;
        }
    }
}
