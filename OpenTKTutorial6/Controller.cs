using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Poly2Tri;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;
using Xna = Microsoft.Xna.Framework;
using FarseerPhysics.Factories;

namespace Game
{
    class Controller
    {
        private Vector2d ViewSize;
        private GameWindow Ctx;
        Matrix4d Projection;
        InputExt Input;
        Wall Shape;
        float avg;
        int StepCounter;
        Random Rand = new Random();
        public PolygonCoordinate SC;
        PortalPair PPair;
        //QFont FontDefault;
        GameWorld GameWorld = new GameWorld();
        Actor Player;
        //FBO FBO;

        int ibo_elements = 0;
       // Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();
        string activeShader = "default";

        public Controller(GameWindow Ctx)
        {
            GL.GenBuffers(1, out ibo_elements);
            //shaders.Add("default", new Shader("vs.glsl", "fs.glsl", true));


            //GameWorld.ActorList.Add(new Actor(new Vector2d(0, 0)));
            //Player = GameWorld.ActorList.GetIndex(0);
            //FBO = new FBO(Ctx.ClientSize);
            //FontDefault = new QFont(new Font("Courier New", 12));
            this.Ctx = Ctx;
            double AspectRatio = (double) this.Ctx.Width / (double) this.Ctx.Height;
            ViewSize = new Vector2d(AspectRatio,1);
            Input = new InputExt(this.Ctx);
            #region AddPolygon
            Shape = new Wall(GameWorld);
            //GameWorld.WallList.Add(Shape);
            Shape.AddSegment(0, new Vector2d(-100, 100), 0);
            Shape.AddSegment(0, new Vector2d(-110, 90), -.1);
            Shape.AddSegment(0, new Vector2d(150, -70), 0);
            Shape.AddSegment(0, new Vector2d(150, 100), .1);
            
            Shape.AddGeometry(0);
            Shape.AddSegment(1, new Vector2d(10, 50), .1);
            Shape.AddSegment(1, new Vector2d(20, 10), 0);
            Shape.AddSegment(1, new Vector2d(100, 10), 0);
            Shape.AddSegment(1, new Vector2d(100, 50), -.1);

            Shape.AddGeometry(1);
            Shape.AddSegment(2, new Vector2d(20, 40), 0);
            Shape.AddSegment(2, new Vector2d(30, 30), 0);
            Shape.AddSegment(2, new Vector2d(90, 20), 0);
            Shape.AddSegment(2, new Vector2d(90, 40), 0);

            Shape.AddGeometry(Polygon.GEOMETRY_NO_PARENT);
            Shape.AddSegment(3, new Vector2d(-100, -100), .2);
            Shape.AddSegment(3, new Vector2d(-100, -50), -1);
            Shape.AddSegment(3, new Vector2d(-150, -50), 0);
            Shape.AddSegment(3, new Vector2d(-150, -100), 0);
            Shape.BufferVertices(BufferUsageHint.StaticDraw);
            #endregion
            SC = new PolygonCoordinate(Shape, 3, 1.4, false);
            Portal P0 = new Portal(SC, false);
            P0.SetSize(100);
            Portal P1 = new Portal(new PolygonCoordinate(Shape, 0, 0.2, false), true);
            PPair = new PortalPair(P0, P1);
            //GameWorld.PortalPairList.Add(PPair);
            Portal P2 = new Portal(new PolygonCoordinate(Shape, 2, 2.2, false), false);
            Portal P3 = new Portal(new PolygonCoordinate(Shape, 0, 2.2, false), false);
            PPair = new PortalPair(P2, P3);
            //GameWorld.PortalPairList.Add(PPair);
            avg = 60;
            StepCounter = 0;
        }
        public void step() 
        {
            Input.Step();
            PlayerInput();
            GameWorld.Step(1 / 60f);
            //ViewControlsDebug(GameWorld.View);
            
            //GameWorld.View.POV = Player.GetPosition();
            //PPair.UpdateFOV(GameWorld.View, 200);
            SC.SetSegmentT((double)-StepCounter/5, true);
            if (Input.MousePress(MouseButton.Left) && Ctx.Focused)
            {
                Shape.AddSegment(0, ViewToWorld(Input.MousePosition()), Rand.NextDouble() - .5);
                Shape.BufferVertices(BufferUsageHint.DynamicDraw);
            }
            StepCounter++;
        }
        public void Draw()
        {
            //shaders[activeShader].EnableVertexAttribArrays();


            /*SetView(GameWorld.View);
            if (Input.KeyDown(Key.F))
            {
                FBO.Set();
            }
            
            SC.DrawDebug();
            GameWorld.Draw();
            PortalSegments PS = GameWorld.PortalPairList.LineProject(Player.GetPosition(), ViewToWorld(Input.MousePosition()),20);
            GL.Color3(Color.GreenYellow);
            PS.GetLines().DrawDebug();
            if (Input.KeyDown(Key.F))
            {
                FBO.Unset();
                GL.PushMatrix();
                GL.LoadIdentity();
                FBO.Draw();
                GL.PopMatrix();
            }
            
            QFont.Begin();
            avg = avg * .995f + (float)Ctx.RenderFrequency * .005f;
            FontDefault.Print(avg.ToString());
            QFont.End();*/
            GL.Disable(EnableCap.Blend);


            //shaders[activeShader].DisableVertexAttribArrays();
        }
        public void SetView(Perspective P)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Projection = Matrix4d.Identity;
            Projection *= P.GetTransform();
            Projection *= Matrix4d.CreateOrthographic(ViewSize.X, ViewSize.Y, -2000, 2000.0);
            
            //Projection = Matrix4d.Mult(Matrix4d.CreateRotationZ((double)0), Projection);
            //Projection = Matrix4d.Mult(Matrix4d.CreateTranslation(new Vector3d(0, 10, 0)),Projection);
            GL.MultMatrix(ref Projection);
            
        }
        public Vector2d ViewToWorld(Vector2d V0)
        {
            var w = Ctx.ClientSize.Width;
            var h = Ctx.ClientSize.Height;
            V0.X = (2 * V0.X / ((double)w)) - 1;
            V0.Y = (2 * -V0.Y / ((double)h)) + 1;
            Vector3d V1 = new Vector3d(V0);
            V1 = Vector3d.Transform(V1, Matrix4d.Invert(Projection));
            return new Vector2d(V1.X, V1.Y);
        }
        public void ViewControlsDebug(Perspective P)
        {
            if (Input.MouseDown(MouseButton.Right) && Ctx.Focused)
            {
                var XDelta = (2f / Ctx.Width) * -(Input.MousePrevious.X - Input.MouseCurrent.X) / P.Orient.Scale;
                if (P.Orient.Mirrored == true)
                {
                    XDelta *= -1;
                }
                var YDelta = (2f / Ctx.Width) * (Input.MousePrevious.Y - Input.MouseCurrent.Y) / P.Orient.Scale;
                P.Orient.Position += new Vector2d(XDelta, YDelta);
            }
            if (Input.MouseWheelDelta() != 0)
            {
                P.Orient.Scale *= Input.MouseWheelDelta() / 5 + 1;
            }
        }
        public void PlayerInput()
        {
            double w = 0;
            double h = 0;
            if (Input.KeyDown(Key.D) != Input.KeyDown(Key.A))
            {
                if (Input.KeyDown(Key.D))
                {
                    w = 2;
                }
                else
                {
                    w = -2;
                }
            }
            if (Input.KeyDown(Key.W) != Input.KeyDown(Key.S))
            {
                if (Input.KeyDown(Key.W))
                {
                    h = 2;
                }
                else
                {
                    h = -2;
                }
            }
            if (w != 0 || h != 0)
            {
                Player.SetSpeed(new Vector2d(w, h));
            }
            
        }
    }
}
