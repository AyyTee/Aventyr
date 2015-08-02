using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;
using Xna = Microsoft.Xna.Framework;
using FarseerPhysics.Factories;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Game
{
    class GameWorld
    {
        //public WallList WallList;
        //public ActorList ActorList;
        //public PortalPairList PortalPairList;
        //public Perspective View;
        public World PhysWorld;
        public Body myBody;
        //public const double PIXEL_TO_METER = 64;
        public GameWorld()
        {
            PhysWorld = new World(new Xna.Vector2(0f, -9.82f));
            //WallList = new WallList(this);
            //ActorList = new ActorList(this);
            //PortalPairList = new PortalPairList(this);
            //View = new Perspective(new Vector2d(0, 0), 1/300f);
            //We create a body object and make it dynamic (movable)
            //Body myBody = BodyFactory.CreateBody(PhysWorld);
            myBody = BodyFactory.CreateBody(PhysWorld, new Xna.Vector2(-90f, 150f));
            //Body myBody2 = BodyFactory.CreateRectangle(PhysWorld, 5, 5, 1);
            myBody.BodyType = BodyType.Dynamic;
            new Body(PhysWorld);
            
            //We create a circle shape with a radius of 0.5 meters
            CircleShape circleShape = new CircleShape(5f, 1f);
            //We fix the body and shape together using a Fixture object
            Fixture fixture = myBody.CreateFixture(circleShape);
            PhysWorld.ProcessChanges();
        }
        public void Step(float TimeStep)
        {
            //ActorList.Step();
            PhysWorld.Step(TimeStep);
        }
        public void Draw()
        {
            for (int i = 0; i < PhysWorld.BodyList.Count(); i++)
            {
                GL.Color3(Color.LightCyan);
                GL.Begin(PrimitiveType.Lines);
                var V = PhysWorld.BodyList[i];
                GL.Vertex2(V.Position.X, V.Position.Y);
                GL.Vertex2(V.Position.X + 5, V.Position.Y + 5);
                GL.End();
            }
            //WallList.Draw();
            //ActorList.Draw();
            //PortalPairList.Draw();
        }
    }
}
