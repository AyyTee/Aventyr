using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using FarseerPhysics.Dynamics;
using Game.Physics;
using Game.Rendering;
using TankGame.Network;
using OpenTK.Graphics;

namespace TankGame
{
    public class Wall : Actor, INetObject
    {
        public int? ServerId { get; set; }

        public Wall(Scene scene, IList<Vector2> vertices)
            : base(scene, vertices)
        {
            SetBodyType(BodyType.Static);
            Entity entity = new Entity(scene);
            entity.AddModel(ModelFactory.CreatePolygon(vertices, Color4.White, new Vector3(0, 0, 1)));
            entity.SetParent(this);

            SetCollisionCategory(Category.All);
        }
    }
}
