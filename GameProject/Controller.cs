using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using QuickFont;
using System.ComponentModel;

namespace Game
{
    class Controller : GameWindow
    {
        public Controller() : base(800, 600, new GraphicsMode(32, 24, 0, 4), "Game", GameWindowFlags.FixedWindow)
        {
            ContextExists = true;
        }
        InputExt InputExt;
        Camera cam;
        Vector2 lastMousePos = new Vector2();
        /// <summary>
        /// Intended to keep pointless messages from the Poly2Tri library out of the console window
        /// </summary>
        public static StreamWriter Log = new StreamWriter("Triangulating.txt");
        public static bool ContextExists = false;
        Model background;
        QFont Default;
        Entity box2;
        List<Portal> portals = new List<Portal>();
        public static List<int> iboGarbage = new List<int>();

        List<Entity> objects = new List<Entity>();
        public static Dictionary<string, int> textures = new Dictionary<string, int>();
        public static Dictionary<string, ShaderProgram> Shaders = new Dictionary<string, ShaderProgram>();

        Matrix4 viewMatrix;

        float Time = 0.0f;
        /// <summary>
        /// The difference in seconds between the last OnUpdateEvent and the current OnRenderEvent.
        /// </summary>
        float TimeRenderDelta = 0.0f;
        private int portalCount;
        private Entity player;

        void initProgram()
        {
            Default = new QFont(@"fonts\Times.ttf", 72, new QFontBuilderConfiguration(false));

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
            float size = 100;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUV.Scale = new Vector2(size, size);
            Entity back = new Entity(new Vector2(0f, 0f));
            back.Models.Add(background);
            objects.Add(back);

            Portal portal0 = new Portal(true);
            //portal0.Transform.Rotation = (float)Math.PI/4f;
            portal0.Transform.Position = new Vector2(0.5f, 0f);
            portal0.Transform.Scale = new Vector2(1f, 1f);
            portal0.Models[0].TransformUV.Scale = new Vector2(5f, 5f);
            objects.Add(portal0);
            portals.Add(portal0);

            Portal portal1 = new Portal(true);
            portal1.Transform.Rotation = 0.1f;
            portal1.Transform.Position = new Vector2(-2f, 0f);
            portal1.Transform.Scale = new Vector2(-1f, 1f);
            objects.Add(portal1);
            portals.Add(portal1);
            Portal.Link(portal0, portal1);

            Model tc = Model.CreateCube();
            tc.Transform.Position = new Vector3(1f, 3f, 0);
            Entity box = new Entity(new Vector2(0,0));
            box.Models.Add(tc);
            objects.Add(box);

            Model tc2 = Model.CreateCube();
            tc2.Transform.Position = new Vector3(-1f, 3f, 0);
            tc2.Transform.Rotation = new Quaternion(1, 0, 0, 1);
            box2 = new Entity(new Vector2(0, 0));
            box2.Models.Add(tc2);
            objects.Add(box2);

            player = new Entity();
            Model playerModel = Model.CreatePolygon(new Vector2[] {
                new Vector2(0.5f, 0), 
                new Vector2(0.35f, 0.15f), 
                new Vector2(0.15f, 0.15f), 
                new Vector2(0.15f, 0.35f), 
                new Vector2(0, 0.5f), 
                new Vector2(-0.5f, 0), 
                new Vector2(0, -0.5f)
            });
            player.Transform.Scale = new Vector2(.5f, .5f);
            player.Models.Add(playerModel);
            objects.Add(player);

            Entity last = new Entity();
            last.Models.Add(new Model());
            objects.Add(last);

            cam = Camera.CameraOrtho(new Vector3(0f, 0f, 10f), 10, Width / (float)Height);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            initProgram();

            GL.ClearColor(Color.HotPink);
            GL.ClearStencil(0);
            GL.PointSize(5f);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            OnUpdateFrame(new FrameEventArgs());
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            TimeRenderDelta += (float)e.Time;
            //GL.Viewport(0, 0, Width, Height);

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            Shaders["textured"].EnableVertexAttribArrays();
            Shaders["default"].EnableVertexAttribArrays();

            // Update model view matrices
            viewMatrix = cam.GetViewMatrix();
            //GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
            //GL.Enable(EnableCap.Blend);
            
            
            DrawScene(viewMatrix, (float)e.Time);
            DrawDebug();
            
            Vector2 viewPos = new Vector2(player.Transform.Position.X, player.Transform.Position.Y);
            DrawPortalAll(portals.ToArray(), viewMatrix, viewPos, 4, TimeRenderDelta);
            Shaders["textured"].DisableVertexAttribArrays();
            Shaders["default"].DisableVertexAttribArrays();

            //GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.Flush();
            SwapBuffers();
        }

        private void DrawPortalAll(Portal[] portals, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta)
        {
            portalCount = 0;

            //stopgap solution. portals will only recursively draw themselves, not any other portals
            IOrderedEnumerable<Portal> portalSort = portals.OrderByDescending(item => (item.Transform.Position - viewPos).Length);
            foreach (Portal p in portalSort)
            {
                GL.Clear(ClearBufferMask.StencilBufferBit);
                DrawPortal(p, viewMatrix, viewPos, depth, timeDelta, 0);
                //break;
            }
            GL.Disable(EnableCap.StencilTest);
            Console.Write(portalCount);
            Console.WriteLine();
        }

        public void DrawPortal(Portal portalEnter, Matrix4 viewMatrix, Matrix4 viewMatrixPrev, Vector2 viewPos, int depth, float timeDelta, int count)
        {
            Vector2[] pv = portalEnter.Linked.GetVerts();
            pv = VectorExt2.Transform(pv, portalEnter.Transform.GetMatrix() * viewMatrix);

            Vector2[] pv2 = portalEnter.GetVerts();
            pv2 = VectorExt2.Transform(pv2, portalEnter.Transform.GetMatrix() * viewMatrixPrev);
            Line portalLine = new Line(pv2);
            Vector2 v = VectorExt2.Transform(viewPos, viewMatrix);
            if (portalLine.IsInsideFOV(v, new Line(pv)))
            {
                DrawPortal(portalEnter, viewMatrix, viewPos, depth, timeDelta, count);
            }
        }

        public void DrawPortal(Portal portalEnter, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta, int count)
        {
            if (depth <= 0)
            {
                return;
            }
            Vector2[] pv = portalEnter.GetVerts();
            pv = VectorExt2.Transform(pv, portalEnter.Transform.GetMatrix() * viewMatrix);
            //this potentially will not correctly cull portals if the viewPos begins outside of the viewspace
            if (MathExt.LineInRectangle(new Vector2(-1, -1), new Vector2(1, 1), pv[0], pv[1]) == false)
            {
                return;
            }
            
            portalCount++;
            GL.Clear(ClearBufferMask.DepthBufferBit);

            //Start using the stencil 
            GL.ColorMask(false, false, false, false);
            GL.DepthMask(false);
            GL.Enable(EnableCap.StencilTest);
            GL.Disable(EnableCap.DepthTest);
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilOp(StencilOp.Incr, StencilOp.Incr, StencilOp.Incr);

            TextWriter console = Console.Out;
            Console.SetOut(Controller.Log);
            Entity fov = new Entity();
            Vector2[] a = portalEnter.GetFOV(viewPos, 50);
            if (a.Length >= 3)
            {
                fov.Models.Add(Model.CreatePolygon(a));
                fov.Render(viewMatrix, timeDelta);
            }
            Console.SetOut(console);

            GL.ColorMask(true, true, true, true);
            GL.DepthMask(true);
            GL.StencilFunc(StencilFunction.Less, count, 0xFF);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);


            GL.Enable(EnableCap.DepthTest);
            Matrix4 portalMatrix = Portal.GetMatrix(portalEnter.Linked, portalEnter) * viewMatrix;
            DrawScene(portalMatrix, timeDelta);

            //GL.Disable(EnableCap.StencilTest);

            Entity fovOutline = new Entity();
            Vector2[] verts = portalEnter.GetFOV(viewPos, 50, 2);
            if (verts.Length > 0)
            {
                fovOutline.Models.Add(Model.CreateLine(new Vector2[] { verts[1], verts[2] }));
                fovOutline.Models.Add(Model.CreateLine(new Vector2[] { verts[0], verts[3] }));
            }
            GL.LineWidth(2f);
            fovOutline.Render(viewMatrix, timeDelta);
            GL.LineWidth(1f);

            DrawPortal(portalEnter, portalMatrix, viewMatrix, VectorExt2.Transform(viewPos, Portal.GetMatrix(portalEnter, portalEnter.Linked)), depth - 1, timeDelta, count + 1);
        }

        private void DrawDebug()
        {
            /*GL.LineWidth(2f);
            Matrix4 view = cam.Transform.GetMatrix();
            Vector2[] verts = VectorExt2.Transform(cam.GetVerts(), view);
            GL.Begin(PrimitiveType.LineLoop);
            foreach (Vector2 v in verts)
            {
                GL.Vertex3(v.X, v.Y, 1);
            }
            GL.End();
            GL.LineWidth(1f);*/
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
            
            #region camera movement
            if (Focused)
            {
                if (InputExt.KeyPress(Key.Escape))
                {
                    Exit();
                }
                Vector3 v = new Vector3();
                float camSpeed = .009f;
                if (InputExt.KeyDown(Key.ShiftLeft))
                {
                    camSpeed = .0009f;
                }
                
                if (InputExt.KeyDown(Key.W))
                {
                    v += cam.GetUp() * camSpeed * cam.Transform.Scale.Y;
                }
                else if (InputExt.KeyDown(Key.S))
                {
                    v -= cam.GetUp() * camSpeed * cam.Transform.Scale.Y;
                }
                if (InputExt.KeyDown(Key.A))
                {
                    v -= cam.GetRight() * camSpeed * cam.Transform.Scale.X;
                }
                else if (InputExt.KeyDown(Key.D))
                {
                    v += cam.GetRight() * camSpeed * cam.Transform.Scale.X;
                }
                if (InputExt.MouseWheelDelta() != 0)
                {
                    cam.Transform.Scale /= (float)Math.Pow(1.2, InputExt.MouseWheelDelta());
                }
                else if (InputExt.KeyDown(Key.Q))
                {
                    cam.Transform.Scale /= (float)Math.Pow(1.04, 1);
                }
                else if (InputExt.KeyDown(Key.E))
                {
                    cam.Transform.Scale /= (float)Math.Pow(1.04, -1);
                }
                Vector2[] vArray = new Vector2[2];
                IntersectPoint i = new IntersectPoint();
                Portal portalEnter = null;
                foreach (Portal p in portals)
                {
                    vArray = VectorExt2.Transform(p.GetVerts(), p.Transform.GetMatrix());
                    portalEnter = p;
                    Vector2 v1 = new Vector2(player.Transform.Position.X, player.Transform.Position.Y);
                    i = MathExt.LineIntersection(vArray[0], vArray[1], v1, v1 + new Vector2(v.X, v.Y), true);
                    if (i.Exists)
                    {
                        break;
                    }
                }
                player.Transform.Position += new Vector2(v.X, v.Y);
                cam.Transform.Position += new Vector3(v.X, v.Y, 0);
                if (i.Exists)
                {
                    portalEnter.Enter(cam.Transform);
                    portalEnter.Enter(player.Transform);
                }
            }
            #endregion

            /*Console.Write(box2.Models[0].Transform.Rotation.W);
            Console.WriteLine();*/

            box2.Models[0].Transform.Rotation += new Quaternion(0, 0, 0, .01f);
            
            /*foreach (Entity v in objects)
            {
                v.StepUpdate();
            }*/
            
            //get rid of all ibo elements no longer used
            foreach (int iboElement in iboGarbage.ToArray())
            {
                int a = iboElement;
                GL.DeleteBuffers(1, ref a);
            }
            iboGarbage.Clear();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Log.Close();
            
            File.Delete("Triangulating.txt");
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
