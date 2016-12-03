using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using FarseerPhysics.Dynamics;

namespace TankGame
{
    public class Wall
    {
        public Actor Actor { get; private set; }
        public readonly int ServerId;

        public Wall(Scene scene, IList<Vector2> vertices, int serverId)
        {
            ServerId = serverId;
            Actor = new Actor(scene, vertices);
            Actor.SetBodyType(BodyType.Static);
            Entity entity = new Entity(scene);
            entity.AddModel(ModelFactory.CreatePolygon(Actor.Vertices, new Vector3(0, 0, 1)));
            entity.SetParent(Actor);
        }
    }
}
