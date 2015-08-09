using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Game
{
    class Controller : GameWindow
    {
        public Controller() : base(1024, 768, new GraphicsMode(32, 24, 0, 4), "Game", GameWindowFlags.FixedWindow)
        {
            
        }
        InputExt InputExt;
        Camera cam;
        Vector2 lastMousePos = new Vector2();
        /// <summary>
        /// Intended to keep pointless messages from the Poly2Tri library out of the console window
        /// </summary>
        StreamWriter Log = new StreamWriter("outputLog.txt");

        Model background;
        Entity fov;
        PortalPair portalPair;

        List<Entity> objects = new List<Entity>();
        public static Dictionary<string, int> textures = new Dictionary<string, int>();
        public static Dictionary<string, ShaderProgram> Shaders = new Dictionary<string, ShaderProgram>();

        Matrix4 viewMatrix;

        float Time = 0.0f;
        /// <summary>
        /// The difference in seconds between the last OnUpdateEvent and the current OnRenderEvent.
        /// </summary>
        float TimeRenderDelta = 0.0f;

        void initProgram()
        {
            //GraphicsContext.CurrentContext.SwapInterval = 1;
            InputExt = new InputExt(this);
            lastMousePos = new Vector2(Mouse.X, Mouse.Y);

            // Load shaders from file
            Shaders.Add("default", new ShaderProgram(@"assets\shaders\vs.glsl", @"assets\shaders\fs.glsl", true));
            Shaders.Add("textured", new ShaderProgram(@"assets\shaders\vs_tex.glsl", @"assets\shaders\fs_tex.glsl", true));

            // Load textures from file
            textures.Add("default.png", loadImage(@"assets\default.png"));
            textures.Add("grid.png", loadImage(@"assets\grid.png"));
            // Create our objects

            background = Model.CreatePlane();
            background.TextureID = textures["grid.png"];
            background.Transform.Position = new Vector3(0, 0, -10f);
            background.Transform.Scale = new Vector3(5f, 5f, 5f);
            background.TransformUV.Scale = new Vector2(5f, 5f);
            Entity back = new Entity(new Vector2(0f, 0f));
            back.Models.Add(background);
            objects.Add(back);

            Portal portal0 = new Portal(true);
            portal0.Transform.Rotation = -4f;
            portal0.Transform.Position = new Vector2(-2f, 0f);
            //portal0.Transform.Scale = new Vector2(-3f, -3f);
            portal0.Models[0].TransformUV.Scale = new Vector2(5f, 5f);
            objects.Add(portal0);

            Portal portal1 = new Portal(true);
            portal1.Transform.Rotation = -4.5f;
            portal1.Transform.Position = new Vector2(1f, 1f);
            portal1.Transform.Scale = new Vector2(1f, -1f);
            objects.Add(portal1);

            portalPair = new PortalPair(portal0, portal1);

            Model tc = Model.CreateCube();
            tc.Transform.Position = new Vector3(1f, 3f, 0);
            Entity box = new Entity(new Vector2(0,0));
            box.Models.Add(tc);
            objects.Add(box);

            Entity last = new Entity();
            last.Models.Add(new Model());
            objects.Add(last);

            fov = new Entity();
            //objects.Add(fov);
            cam = Camera.CameraOrtho(new Vector3(0f, 0f, 10f), 10, Width / (float)Height);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            initProgram();

            GL.ClearColor(Color.CornflowerBlue);
            GL.ClearStencil(0);
            //GL.ClearDepth(1);
            GL.PointSize(5f);

            OnUpdateFrame(new FrameEventArgs());
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            TimeRenderDelta += (float)e.Time;
            GL.Viewport(0, 0, Width, Height);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            Shaders["textured"].EnableVertexAttribArrays();
            Shaders["default"].EnableVertexAttribArrays();

            // Update model view matrices
            viewMatrix = cam.GetViewMatrix();
            GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
            
            DrawScene(viewMatrix, (float)e.Time);
            DrawDebug();

            DrawPortal(portalPair.Portals[0], portalPair.Portals[1], e);
            DrawPortal(portalPair.Portals[1], portalPair.Portals[0], e);
            Shaders["textured"].DisableVertexAttribArrays();
            Shaders["default"].DisableVertexAttribArrays();
            GL.Flush();
            SwapBuffers();
        }

        private void DrawPortal(Portal portalEnter, Portal portalExit, FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            //Start using the stencil 
            GL.ColorMask(false, false, false, false);
            //GL.DepthMask(false);
            GL.Enable(EnableCap.StencilTest);
            GL.Disable(EnableCap.DepthTest);
            //Place a 1 where rendered 
            GL.StencilFunc(StencilFunction.Always, 1, 1);
            //Replace where rendered 
            GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
            //Render stencil triangle 
            Console.SetOut(Log);
            fov.Models.Clear();
            Vector2[] a = portalEnter.GetFOV(new Vector2(cam.Transform.Position.X, cam.Transform.Position.Y), 5);
            if (a.Length >= 3)
            {
                fov.Models.Add(Model.CreatePolygon(a));
                fov.Render(viewMatrix, TimeRenderDelta);
            }
            Console.SetOut(Console.Out);
            
            //Reenable color 
            GL.ColorMask(true, true, true, true);
            //GL.DepthMask(true);
            //Where a 1 was not rendered 
            GL.StencilFunc(StencilFunction.Equal, 1, 1);
            //Keep the pixel 
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);


            GL.Enable(EnableCap.DepthTest);
            DrawScene(Portal.GetMatrix(portalExit, portalEnter) * cam.GetViewMatrix(), (float)e.Time);
        }

        private void DrawDebug()
        {
            Vector3[] vector = new Vector3[6];
            vector[0] = new Vector3((float)cam.Transform.Position.X - 0.2f, (float)cam.Transform.Position.Y, 0);
            vector[1] = new Vector3((float)cam.Transform.Position.X + 0.2f, (float)cam.Transform.Position.Y, 0);
            vector[2] = new Vector3((float)cam.Transform.Position.X, (float)cam.Transform.Position.Y, 0);
            vector[3] = new Vector3((float)cam.Transform.Position.X, (float)cam.Transform.Position.Y + 0.2f, 0);
            vector[4] = new Vector3((float)cam.Transform.Position.X, (float)cam.Transform.Position.Y, 0);
            vector[5] = new Vector3((float)cam.Transform.Position.X + 0.2f, (float)cam.Transform.Position.Y + 0.2f, 0);
            GL.LineWidth(2f);
            GL.Begin(PrimitiveType.Lines);
            foreach (Vector3 v in vector)
            {
                GL.Vertex3(v);
            }
            Matrix4 m = Portal.GetMatrix(portalPair.Portals[0], portalPair.Portals[1]);
            foreach (Vector3 v in vector)
            {
                GL.Vertex3(Vector3.Transform(v, m));
            }
            GL.End();
            GL.LineWidth(1f);
        }

        private void DrawScene(Matrix4 viewMatrix, float timeDelta)
        {
            
            // Draw all our objects
            foreach (Entity v in objects)
            {
                v.Render(viewMatrix, (float)Math.Min(TimeRenderDelta, 1 / UpdateFrequency));
            }
            
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            Time += (float)e.Time;
            TimeRenderDelta = 0;

            InputExt.Update();
            if (Focused)
            {
                if (InputExt.KeyPress(Key.Escape))
                {
                    Exit();
                }
                Vector3 v = new Vector3();
                if (InputExt.KeyDown(Key.W))
                {
                    v += cam.GetUp() * 0.021f * cam.Transform.Scale.Y;
                }
                else if (InputExt.KeyDown(Key.S))
                {
                    v -= cam.GetUp() * 0.021f * cam.Transform.Scale.Y;
                }
                if (InputExt.KeyDown(Key.A))
                {
                    v -= cam.GetRight() * 0.021f * cam.Transform.Scale.X;
                }
                else if (InputExt.KeyDown(Key.D))
                {
                    v += cam.GetRight() * 0.021f * cam.Transform.Scale.X;
                }
                if (InputExt.MouseWheelDelta() != 0)
                {
                    cam.Transform.Scale /= (float)Math.Pow(1.2, InputExt.MouseWheelDelta());
                }
                Vector2[] vArray = new Vector2[2];
                IntersectPoint i = new IntersectPoint();
                Portal portalEnter = null;
                foreach (Portal p in portalPair.Portals)
                {
                    vArray = VectorExt2.Transform(p.GetVerts(), p.Transform.GetMatrix());
                    portalEnter = p;
                    Vector2 camPos = new Vector2(cam.Transform.Position.X, cam.Transform.Position.Y);
                    i = MathExt.LineIntersection(vArray[0], vArray[1], camPos, camPos + new Vector2(v.X, v.Y), true);
                    if (i.Exists)
                    {
                        break;
                    }
                }
                cam.Transform.Position += v;
                if (i.Exists)
                {
                    Portal portalExit = portalPair.Portals[0];
                    if (portalPair.Portals[0] == portalEnter)
                    {
                        portalExit = portalPair.Portals[1];
                    }
                    //Transform2D t = Portal.GetTransform(portalLast, portalExit);
                    Matrix4 m = Portal.GetMatrix(portalEnter, portalExit);
                    Transform2D t = cam.Transform.GetTransform2D();
                    Vector2 v0 = new Vector2(t.Position.X, t.Position.Y);
                    Vector2 v1 = v0 + new Vector2(1, 0);
                    Vector2 v2 = v0 + new Vector2(0, 1);
                    Vector2 v3 = VectorExt2.Transform(v0, m);
                    Vector2 v4 = VectorExt2.Transform(v1, m);
                    Vector2 v5 = VectorExt2.Transform(v2, m);
                    Transform2D t0 = portalEnter.Transform;
                    Transform2D t1 = portalExit.Transform;

                    cam.Transform.Position = new Vector3(v3.X, v3.Y, cam.Transform.Position.Z);
                    /*cam.Transform.Rotation += new Quaternion(0, 0, 0, (float)(MathExt.AngleLine(v3, v5) + Math.PI));
                    float angle0 = (float)MathExt.AngleLine(v3, v4);
                    float angle1 = (float)MathExt.AngleLine(v3, v5);*/
                    Transform2D tEnter = portalEnter.Transform;
                    Transform2D tExit = portalExit.Transform;
                    float flipX = 1;
                    float flipY = 1;
                    if (Math.Sign(tEnter.Scale.X) == Math.Sign(tExit.Scale.X))
                    {
                        flipX = -1;
                    }
                    if (Math.Sign(tEnter.Scale.Y) != Math.Sign(tExit.Scale.Y))
                    {
                        flipY = -1;
                    }
                    cam.Transform.Scale *= new Vector3(flipX * (v4 - v3).Length, flipY * (v5 - v3).Length, 1);

                    float angle;
                    if (flipX != flipY)
                    {
                        //Vector2 v6 = MathExt.VectorMirror(v4 - v3, );
                        /*Vector2 v6 = new Vector2(v4.X, -v4.Y);
                        angle = (float)MathExt.AngleLine(v6, v3);*/
                        angle = 0;
                        /*{
                            if (portalExit.Transform.Rotation == 0 || portalEnter.Transform.Rotation == 0)
                            {
                                if (portalExit.Transform.Rotation < 0 || portalEnter.Transform.Rotation < 0)
                                {
                                    angle = -Math.Abs(portalExit.Transform.Rotation - portalEnter.Transform.Rotation);
                                }
                                else
                                {
                                    angle = Math.Abs(portalExit.Transform.Rotation - portalEnter.Transform.Rotation);
                                }
                                cam.Transform.Rotation += new Quaternion(0, 0, 0, -2f * cam.Transform.Rotation.W);
                            }
                            else
                            {
                                angle = portalExit.Transform.Rotation - portalEnter.Transform.Rotation + (float)Math.PI;
                                //cam.Transform.Rotation += new Quaternion(0, 0, 0, -2f * cam.Transform.Rotation.W);
                            }
                            //cam.Transform.Rotation += new Quaternion(0, 0, 0, -2f * cam.Transform.Rotation.W);
                        }*/
                    }
                    else
                    {
                        angle = portalExit.Transform.Rotation - portalEnter.Transform.Rotation;
                    }
                    cam.Transform.Rotation += new Quaternion(0, 0, 0, angle);
                }
            }
            /*Console.SetOut(Log);
            fov.Models.Clear();
            //portalPair.Portals[1].Transform.Rotation += 0.005f;
            foreach (Portal portal in portalPair.Portals)
            {
                Vector2[] a = portal.GetFOV(new Vector2(cam.Transform.Position.X, cam.Transform.Position.Y), 5);
                if (a.Length >= 3)
                {
                    fov.Models.Add(Model.CreatePolygon(a));
                }
                break;
            }
            Console.SetOut(Console.Out);
            */
            

            //objects[1].Transform.Rotation += 0.1f;
            //background.TransformUV.Rotation += 0.01f;
            //portal.GetFOV;
            
            foreach (Entity v in objects)
            {
                v.StepUpdate();
            }
        }

        int loadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }

        int loadImage(string filename)
        {
            try
            {
                Bitmap file = new Bitmap(filename);
                return loadImage(file);
            }
            catch (FileNotFoundException e)
            {
                return -1;
            }
        }
    }
}
