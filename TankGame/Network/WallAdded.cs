using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TankGame.Network
{
    [DataContract]
    public class WallAdded
    {
        [DataMember]
        public int WallServerId { get; set; }
        [DataMember]
        public Vector2[] Vertices { get; set; }
        [DataMember]
        public Transform2 Transform { get; set; }

        public WallAdded(Wall wall)
        {
            WallServerId = wall.ServerId;
            Vertices = wall.Actor.Vertices.ToArray();
            Transform = wall.Actor.GetTransform();
        }

        public Wall WallCreate(Scene scene)
        {
            Wall wall = new Wall(scene, Vertices, WallServerId);
            wall.Actor.SetTransform(Transform);
            return wall;
        }
    }
}
