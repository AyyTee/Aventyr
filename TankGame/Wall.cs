using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using FarseerPhysics.Dynamics;
using TankGame.Network;

namespace TankGame
{
    public class Wall : INetObject
    {
        public Actor Actor { get; private set; }
        public int? ServerId { get; set; }

        public Wall(Scene scene, IList<Vector2> vertices)
        {
            Actor = new Actor(scene, vertices);
            Actor.SetBodyType(BodyType.Static);
            Entity entity = new Entity(scene);
            entity.AddModel(ModelFactory.CreatePolygon(Actor.Vertices, new Vector3(0, 0, 1)));
            entity.SetParent(Actor);
        }
    }
}
