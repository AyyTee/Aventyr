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
using System.ComponentModel;

namespace Game
{
    public class Controller : GameWindow
    {
        public Controller()
            : base((int) CanvasSize.Width, (int) CanvasSize.Height, new GraphicsMode(32, 24, 8, 1), "Game", GameWindowFlags.FixedWindow)
        {
            ContextExists = true;
        }
        InputExt InputExt;
        Camera cam, hudCam;
        Vector2 lastMousePos = new Vector2();
        /// <summary>
        /// Intended to keep pointless messages from the Poly2Tri library out of the console window
        /// </summary>
        public static StreamWriter Log = new StreamWriter("Triangulating.txt");
        public static bool ContextExists = false;
        Model background;
        Font Default;
        Entity box2;
        public static List<int> iboGarbage = new List<int>();
        public static Size CanvasSize = new Size(800, 600);

        public static Dictionary<string, int> textures = new Dictionary<string, int>();
        public static Dictionary<string, ShaderProgram> Shaders = new Dictionary<string, ShaderProgram>();

        Matrix4 viewMatrix;
        Scene scene, hud;
        FontRenderer FontRenderer;
        float Time = 0.0f;
        /// <summary>
        /// The difference in seconds between the last OnUpdateEvent and the current OnRenderEvent.
        /// </summary>
        float TimeRenderDelta = 0.0f;
        private int portalCount;
        private Entity player;
        Portal portal1;
        Entity text;
        void initProgram()
        {
            scene = new Scene(this);
            hud = new Scene(this);
            hudCam = Camera.CameraOrtho(new Vector3(Width/2, Height/2, 0), Height, Width / (float)Height);

            System.Drawing.Text.PrivateFontCollection privateFonts = new System.Drawing.Text.PrivateFontCollection();
            privateFonts.AddFontFile(@"assets\fonts\times.ttf");
            Default = new Font(privateFonts.Families[0], 14);
            FontRenderer = new FontRenderer(Default);

            InputExt = new InputExt(this);
            lastMousePos = new Vector2(Mouse.X, Mouse.Y);

            // Load shaders from file
            Shaders.Add("default", new ShaderProgram(@"assets\shaders\vs.glsl", @"assets\shaders\fs.glsl", true));
            Shaders.Add("textured", new ShaderProgram(@"assets\shaders\vs_tex.glsl", @"assets\shaders\fs_tex.glsl", true));
            Shaders.Add("text", new ShaderProgram(@"assets\shaders\vs_text.glsl", @"assets\shaders\fs_text.glsl", true));

            // Load textures from file
            textures.Add("default.png", loadImage(@"assets\default.png"));
            textures.Add("grid.png", loadImage(@"assets\grid.png"));//"grid.png", FontRenderer.textureID);
            // Create our objects
            

            background = Model.CreatePlane();
            background.TextureID = textures["grid.png"];
            background.Transform.Position = new Vector3(0, 0, -10f);
            float size = 100;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUV.Scale = new Vector2(size, size);
            Entity back = new Entity(new Vector2(0f, 0f));
            back.Models.Add(background);

            Portal portal0 = new Portal(true);
            //portal0.Transform.Rotation = (float)(Math.PI/4f + Math.PI);
            portal0.Transform.Position = new Vector2(.1f, 0f);
            portal0.Transform.Scale = new Vector2(1f, 1f);

            Entity portalEntity0 = new Entity();
            portalEntity0.Transform = portal0.Transform;
            portalEntity0.Models.Add(Model.CreatePlane());
            portalEntity0.Models[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity0.Models[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity0.Models.Add(Model.CreatePlane());
            portalEntity0.Models[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);
            

            portal1 = new Portal(true);
            portal1.Transform.Rotation = 0.1f;
            portal1.Transform.Position = new Vector2(-.1f, 0f);
            portal1.Transform.Scale = new Vector2(-1f, 1f);

            Portal.Link(portal0, portal1);
            Entity portalEntity1 = new Entity();
            portalEntity1.Transform = portal1.Transform;
            portalEntity1.Models.Add(Model.CreatePlane());
            portalEntity1.Models[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity1.Models[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity1.Models.Add(Model.CreatePlane());
            portalEntity1.Models[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);
            

            Portal portal2 = new Portal(true);
            portal2.Transform.Rotation = 0.1f;//(float)Math.PI/4f;
            portal2.Transform.Position = new Vector2(1f, 2f);
            portal2.Transform.Scale = new Vector2(1f, 1f);

            Entity portalEntity2 = new Entity();
            portalEntity2.Transform = portal2.Transform;
            portalEntity2.Models.Add(Model.CreatePlane());
            portalEntity2.Models[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity2.Models[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity2.Models.Add(Model.CreatePlane());
            portalEntity2.Models[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);
            

            Portal portal3 = new Portal(true);
            //portal3.Transform.Rotation = 0.4f;
            portal3.Transform.Position = new Vector2(-1f, -1.2f);
            portal3.Transform.Scale = new Vector2(-1f, 1f);

            Portal.Link(portal2, portal3);
            Entity portalEntity3 = new Entity();
            portalEntity3.Transform = portal3.Transform;
            portalEntity3.Models.Add(Model.CreatePlane());
            portalEntity3.Models[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity3.Models[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity3.Models.Add(Model.CreatePlane());
            portalEntity3.Models[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);

            Model tc = Model.CreateCube();
            tc.Transform.Position = new Vector3(1f, 3f, 0);
            Entity box = new Entity(new Vector2(0,0));
            box.Models.Add(tc);

            Model tc2 = Model.CreateCube();
            tc2.Transform.Position = new Vector3(-1f, 3f, 0);
            tc2.Transform.Rotation = new Quaternion(1, 0, 0, 1);
            box2 = new Entity(new Vector2(0, 0));
            box2.Models.Add(tc2);

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
            //playerModel.Transform.Scale = new Vector3(20, .2f, 1);
            playerModel.SetTexture(Controller.textures["default.png"]);
            player.IsPortalable = true;
            player.Transform.Scale = new Vector2(.5f, .5f);
            player.Transform.Position = new Vector2(0f, 0f);
            player.Models.Add(playerModel);
            playerModel.SetTexture(Controller.textures["default.png"]);

            Entity origin = new Entity();
            origin.Transform.Position = new Vector2(0, 0);
            origin.Models.Add(Model.CreatePlane(new Vector2(0.1f, 0.1f)));

            text = new Entity();
            text.Transform.Position = new Vector2(0, CanvasSize.Height);
            

            hud.AddEntity(text);
            scene.AddEntity(origin);
            scene.AddEntity(back);
            scene.AddPortal(portal0);
            scene.AddPortal(portal1);
            scene.AddPortal(portal2);
            scene.AddPortal(portal3);
            scene.AddEntity(portalEntity0);
            scene.AddEntity(portalEntity1);
            scene.AddEntity(portalEntity2);
            scene.AddEntity(portalEntity3);
            scene.AddEntity(box);
            scene.AddEntity(box2);
            scene.AddEntity(player);

            cam = Camera.CameraOrtho(new Vector3(player.Transform.Position.X, player.Transform.Position.Y, 10f), 10, Width / (float)Height);
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
            
            scene.DrawScene(viewMatrix, (float)e.Time);
            DrawDebug();
            
            Vector2 viewPos = new Vector2(player.Transform.Position.X, player.Transform.Position.Y);
            DrawPortalAll(scene.Portals.ToArray(), viewMatrix, viewPos, 4, TimeRenderDelta, 20);

            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.DepthTest);
            hud.DrawScene(hudCam.GetViewMatrix(), (float)e.Time);
            GL.Enable(EnableCap.DepthTest);
            Shaders["textured"].DisableVertexAttribArrays();
            Shaders["default"].DisableVertexAttribArrays();

            GL.Flush();
            SwapBuffers();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portals"></param>
        /// <param name="viewMatrix"></param>
        /// <param name="viewPos"></param>
        /// <param name="depth"></param>
        /// <param name="timeDelta"></param>
        /// <param name="sceneMaxDepth">The difference between the nearest and farthest object in the scene</param>
        private void DrawPortalAll(Portal[] portals, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta, float sceneDepth)
        {
            portalCount = 0;

            //stopgap solution. portals will only recursively draw themselves, not any other portals
            IOrderedEnumerable<Portal> portalSort = portals.OrderByDescending(item => (item.Transform.Position - viewPos).Length);
            foreach (Portal p in portalSort)
            {
                GL.Clear(ClearBufferMask.StencilBufferBit | ClearBufferMask.DepthBufferBit);
                DrawPortal(p, viewMatrix, viewPos, depth, timeDelta, 0, sceneDepth);
                //break;
            }
            GL.Disable(EnableCap.StencilTest);
        }

        public void DrawPortal(Portal portalEnter, Matrix4 viewMatrix, Matrix4 viewMatrixPrev, Vector2 viewPos, int depth, float timeDelta, int count, float sceneDepth)
        {
            Vector2[] pv = portalEnter.Linked.GetVerts();
            pv = VectorExt2.Transform(pv, portalEnter.Transform.GetMatrix() * viewMatrix);

            Vector2[] pv2 = portalEnter.GetVerts();
            pv2 = VectorExt2.Transform(pv2, portalEnter.Transform.GetMatrix() * viewMatrixPrev);
            Line portalLine = new Line(pv2);
            Vector2 v = VectorExt2.Transform(viewPos, viewMatrix);
            if (portalLine.IsInsideFOV(v, new Line(pv)))
            {
                DrawPortal(portalEnter, viewMatrix, viewPos, depth, timeDelta, count, sceneDepth);
            }
        }

        public void DrawPortal(Portal portalEnter, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta, int count, float sceneDepth)
        {
            if (depth <= 0)
            {
                return;
            }

            if (portalEnter.OneSided)
            {
                Vector2[] pv2 = portalEnter.GetWorldVerts();

                Line portalLine = new Line(pv2);
                if (portalLine.GetSideOf(pv2[0] + portalEnter.Transform.GetNormal()) != portalLine.GetSideOf(viewPos))
                {
                    return;
                }
            }

            Vector2[] pv = portalEnter.GetVerts();
            pv = VectorExt2.Transform(pv, portalEnter.Transform.GetMatrix() * viewMatrix);
            //this will not correctly cull portals if the viewPos begins outside of the viewspace
            if (MathExt.LineInRectangle(new Vector2(-1, -1), new Vector2(1, 1), pv[0], pv[1]) == false)
            {
                return;
            }

            

            viewMatrix = Matrix4.CreateTranslation(new Vector3(0, 0, sceneDepth)) * viewMatrix;
            portalCount++;

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
                fov.Render(scene, viewMatrix, timeDelta);
            }
            Console.SetOut(console);

            GL.ColorMask(true, true, true, true);
            GL.DepthMask(true);
            GL.StencilFunc(StencilFunction.Less, count, 0xFF);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);


            GL.Enable(EnableCap.DepthTest);
            Matrix4 portalMatrix = Portal.GetMatrix(portalEnter.Linked, portalEnter) * viewMatrix;
            scene.DrawScene(portalMatrix, timeDelta);

            //GL.Disable(EnableCap.StencilTest);

            Entity fovOutline = new Entity();
            Vector2[] verts = portalEnter.GetFOV(viewPos, 50, 2);
            if (verts.Length > 0)
            {
                fovOutline.Models.Add(Model.CreateLine(new Vector2[] { verts[1], verts[2] }));
                fovOutline.Models.Add(Model.CreateLine(new Vector2[] { verts[0], verts[3] }));
                foreach (Model model in fovOutline.Models)
                {
                    Vector3 v = model.Transform.Position;
                    v.Z = sceneDepth * (depth + count);
                    model.Transform.Position = v;
                }
            }
            
            GL.LineWidth(2f);
            fovOutline.Render(scene, viewMatrix, timeDelta);
            GL.LineWidth(1f);

            DrawPortal(portalEnter, portalMatrix, viewMatrix, VectorExt2.Transform(viewPos, Portal.GetMatrix(portalEnter, portalEnter.Linked)), depth - 1, timeDelta, count + 1, sceneDepth);
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
                if (InputExt.KeyDown(Key.R))
                {
                    Quaternion rot = cam.Transform.Rotation;
                    rot.W += .01f;
                    cam.Transform.Rotation = rot;
                    player.Transform.Rotation += .01f;
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
                    v += cam.GetRight() * camSpeed * cam.Transform.Scale.X; //new Vector3(.1f, 0, 0);//
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
                foreach (Portal p in scene.Portals)
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
            //portal1.Transform.Rotation += .001f;
            text.Models.Clear();
            text.Models.Add(FontRenderer.GetModel(((float)e.Time).ToString(), new Vector2(0f, 0f), 0));
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
