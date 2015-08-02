using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;
using Xna = Microsoft.Xna.Framework;
using FarseerPhysics.Factories;
using FarseerPhysics.Common;
using OpenTK;

namespace Game
{
    class Wall : Polygon
    {
        GameWorld GameWorld;
        Body Body;
        public Wall(GameWorld GameWorld)
        {
            this.GameWorld = GameWorld;
            Body = BodyFactory.CreateBody(GameWorld.PhysWorld, this);
            Body.BodyType = BodyType.Static;
        }
    }
}
