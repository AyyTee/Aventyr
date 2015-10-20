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
using FarseerPhysics;
using Xna = Microsoft.Xna.Framework;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;
using System.Diagnostics;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Cgen.Audio;
using System.Threading;


namespace Game
{
    public class Controller : GameWindow
    {
        public Controller()
            : base((int) 800, (int) 600, new GraphicsMode(32, 24, 8, 1), "Game", GameWindowFlags.FixedWindow)
        {
            ContextExists = true;
            ClientSize = base.ClientSize;
        }
        InputExt InputExt;
        Camera cam, hudCam;
        Vector2 lastMousePos = new Vector2();
        /// <summary>
        /// Intended to keep pointless messages from the Poly2Tri library out of the console window
        /// </summary>
        public static StreamWriter Log = new StreamWriter("Triangulating.txt");
        public static bool ContextExists = false;
        public static Size ClientSize;
        public const int StepsPerSecond = 60;
        public const int DrawsPerSecond = 60;
        Model background;
        Font Default;
        Entity box2;
        Entity boxChild;
        Entity intersectDot;
        public static List<int> iboGarbage = new List<int>();

        public static Dictionary<string, int> Textures = new Dictionary<string, int>();
        public static Dictionary<string, ShaderProgram> Shaders = new Dictionary<string, ShaderProgram>();

        public static String fontFolder = Path.Combine(new String[2] {
            "assets",
            "fonts"
        });
        public static String shaderFolder = Path.Combine(new String[2] {
            "assets",
            "shaders"
        });
        public static String textureFolder = Path.Combine(new String[2] {
            "assets",
            "textures"
        });
        public static String soundFolder = Path.Combine(new String[2] {
            "assets",
            "sounds"
        });
        Scene scene, hud;
        FontRenderer FontRenderer;
        Entity tempLine;
        Portal portal0, portal1, portal2, portal3;
        float Time = 0.0f;
        /// <summary>
        /// The difference in seconds between the last OnUpdateEvent and the current OnRenderEvent.
        /// </summary>
        float TimeRenderDelta = 0.0f;
        private Entity player;
        Entity text, text2;
        SoundSystem soundPlayer;
        Sound testSound;
        Renderer renderer;
        void initProgram()
        {

            soundPlayer = SoundSystem.Instance();
            soundPlayer.Init();
            
            testSound = new Sound("My Sound", Path.Combine(Controller.soundFolder, "test_sound.ogg"));
            //testSound.Play();
            //testSound.SetLoop(true);
            //sound.SetPosition(1000, 0, 0);
            
            scene = new Scene();
            hud = new Scene();
            hudCam = Camera.CameraOrtho(new Vector3(Width/2, Height/2, 0), Height, Width / (float)Height);

            System.Drawing.Text.PrivateFontCollection privateFonts = new System.Drawing.Text.PrivateFontCollection();
            privateFonts.AddFontFile(Path.Combine(fontFolder, "times.ttf"));
            Default = new Font(privateFonts.Families[0], 14);
            FontRenderer = new FontRenderer(Default);

            InputExt = new InputExt(this);
            lastMousePos = new Vector2(Mouse.X, Mouse.Y);

            // Load shaders from file
            Shaders.Add("default", new ShaderProgram(Path.Combine(shaderFolder, "vs.glsl"), Path.Combine(shaderFolder, "fs.glsl"), true));
            Shaders.Add("textured", new ShaderProgram(Path.Combine(shaderFolder, "vs_tex.glsl"), Path.Combine(shaderFolder, "fs_tex.glsl"), true));
            Shaders.Add("text", new ShaderProgram(Path.Combine(shaderFolder, "vs_text.glsl"), Path.Combine(shaderFolder, "fs_text.glsl"), true));

            // Load textures from file
            Textures.Add("default.png", loadImage(Path.Combine(textureFolder, "default.png")));
            Textures.Add("grid.png", loadImage(Path.Combine(textureFolder, "grid.png")));
            

            background = Model.CreatePlane();
            background.TextureID = Textures["grid.png"];
            background.Transform.Position = new Vector3(0, 0, -10f);
            float size = 100;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUV.Scale = new Vector2(size, size);
            Entity back = scene.CreateEntity(new Vector2(0f, 0f));
            back.Models.Add(background);

            portal0 = scene.CreatePortal();
            portal0.Transform.Rotation = (float)Math.PI;
            portal0.Transform.Position = new Vector2(2.1f, 0f);
            portal0.Transform.Scale = new Vector2(-1.5f, -1.5f);

            Entity portalEntity0 = scene.CreateEntity();
            portalEntity0.Transform.Parent = portal0.Transform;
            portalEntity0.Models.Add(Model.CreatePlane());
            portalEntity0.Models[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity0.Models[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity0.Models.Add(Model.CreatePlane());
            portalEntity0.Models[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);

            portal1 = scene.CreatePortal();
            portal1.Transform.Rotation = 4.4f;
            portal1.Transform.Position = new Vector2(-3f, 0f);
            portal1.Transform.Scale = new Vector2(-1f, -1f);

            Portal.Link(portal0, portal1);
            //Portal.Link(portal1, portal1);
            Entity portalEntity1 = scene.CreateEntity();
            portalEntity1.Transform.Parent = portal1.Transform; 
            portalEntity1.Models.Add(Model.CreatePlane());
            portalEntity1.Models[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity1.Models[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity1.Models.Add(Model.CreatePlane());
            portalEntity1.Models[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);


            portal2 = scene.CreatePortal();
            portal2.Transform.Rotation = 0.1f;//(float)Math.PI/4f;
            portal2.Transform.Position = new Vector2(2.1f, 2f);
            portal2.Transform.Scale = new Vector2(1f, 1f);

            Entity portalEntity2 = scene.CreateEntity();
            portalEntity2.Transform.Parent = portal2.Transform;
            portalEntity2.Models.Add(Model.CreatePlane());
            portalEntity2.Models[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity2.Models[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity2.Models.Add(Model.CreatePlane());
            portalEntity2.Models[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);


            portal3 = scene.CreatePortal();
            portal3.Transform.Rotation = 0.4f;
            portal3.Transform.Position = new Vector2(-2f, 2f);
            portal3.Transform.Scale = new Vector2(-1f, 1f);

            Portal.Link(portal2, portal3);
            Entity portalEntity3 = scene.CreateEntity();
            portalEntity3.Transform.Parent = portal3.Transform;
            portalEntity3.Models.Add(Model.CreatePlane());
            portalEntity3.Models[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity3.Models[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity3.Models.Add(Model.CreatePlane());
            portalEntity3.Models[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);

            #region cubes
            Model tc = Model.CreateCube();
            tc.Transform.Position = new Vector3(1f, 3f, 0);
            Entity box = scene.CreateEntity(new Vector2(0, 0));
            box.Models.Add(tc);

            Model tc2 = Model.CreateCube();
            tc2.Transform.Position = new Vector3(0f, 0f, 0);
            tc2.Transform.Rotation = new Quaternion(0.1f, 0, 0, 1);
            box2 = scene.CreateEntity(new Vector2(0, 0));
            box2.Models.Add(tc2);

            
            boxChild = scene.CreateEntity(new Vector2(0, 1));
            boxChild.Models.Add(tc2);
            boxChild.Transform.Parent = box2.Transform;

            //portal3.Transform.Parent = boxChild.Transform;
            #endregion

            #region player
            player = scene.CreateEntity();
            Model playerModel = Model.CreatePolygon(new Vector2[] {
                new Vector2(0.5f, 0), 
                new Vector2(0.35f, 0.15f), 
                new Vector2(0.15f, 0.15f), 
                new Vector2(0.15f, 0.35f), 
                new Vector2(0, 0.5f), 
                new Vector2(-0.5f, 0), 
                new Vector2(0, -0.5f)
            });
            //playerModel.Transform.Scale = new Vector3(-15, .2f, 1);
            playerModel.SetTexture(Controller.Textures["default.png"]);
            player.IsPortalable = true;
            //player.Transform.Scale = new Vector2(.5f, .5f);
            player.Transform.Position = new Vector2(0f, 0f);
            player.Models.Add(playerModel);
            playerModel.SetTexture(Controller.Textures["default.png"]);
            #endregion

            Entity playerParent = scene.CreateEntity(new Vector2(1, 0));
            //player.Transform.Parent = playerParent.Transform;

            intersectDot = scene.CreateEntity();
            //intersectDot.Models.Add(Model.CreateCube());
            intersectDot.Models = portalEntity0.Models;
            intersectDot.Transform.Scale = new Vector2(1f, 1f);

            Vector2[] v = new Vector2[6] {
                new Vector2(0.01f, 0) * 2,
                new Vector2(1f, 0.5f) * 2,
                new Vector2(1f, -1f) * 2,
                new Vector2(-1f, -1.1f) * 2,
                new Vector2(-0.5f, 0),
                new Vector2(-0.5f, 0.1f)
            };
            
            /*Entity ground = scene.CreateEntityPolygon(new Vector2(0, -4f), new Vector2(0, 0), v);
            ground.Models.Add(Model.CreatePolygon(v));
            ground.Transform.Rotation = 0.5f;
            
            Entity origin = scene.CreateEntityBox(new Vector2(0.4f, 0f), new Vector2(1.5f, 1.5f));
            */
            text = hud.CreateEntity();
            text.Transform.Position = new Vector2(0, ClientSize.Height);
            text2 = hud.CreateEntity();
            text2.Transform.Position = new Vector2(0, ClientSize.Height - 40);

            cam = Camera.CameraOrtho(new Vector3(player.Transform.Position.X, player.Transform.Position.Y, 10f), 10, Width / (float)Height);
            
            scene.ActiveCamera = cam;
            hud.ActiveCamera = hudCam;
            renderer = new Renderer(this);
            renderer.RenderScenes.Add(scene);
            renderer.RenderScenes.Add(hud);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            initProgram();
            Renderer.Init();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            TimeRenderDelta += (float)e.Time;
            /*text.Models.Clear();
            text.Models.Add(FontRenderer.GetModel(((float)e.Time).ToString(), new Vector2(0f, 0f), 0));*/

            renderer.Render();
        }

        private void ToggleFullScreen()
        {
            if (WindowState == OpenTK.WindowState.Normal)
            {
                WindowState = OpenTK.WindowState.Fullscreen;
                cam.Aspect = Width / (float)Height;
                hudCam.Aspect = cam.Aspect;
                hudCam.Scale = Height;
            }
            else if (WindowState == OpenTK.WindowState.Fullscreen)
            {
                WindowState = OpenTK.WindowState.Normal;
                ClientSize = new Size(800, 600);
                cam.Aspect = Width / (float)Height;
                hudCam.Aspect = cam.Aspect;
                hudCam.Scale = Height;
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            Time += (float)e.Time;
            TimeRenderDelta = 0;
            
            InputExt.Update();
            if (InputExt.KeyPress(Key.F4))
            {
                ToggleFullScreen();   
            }

            
            /*if (tempLine != null)
            {
                scene.RemoveEntity(tempLine);
            }
            
            tempLine = scene.CreateEntity();
            tempLine.Transform.Position = player.Transform.Position;

            Vector2 rayBegin = player.Transform.Position;
            Vector2 rayEnd = VectorExt2.Transform(new Vector2(Mouse.X / (float)(ClientSize.Width / 2) - 1f, -(Mouse.Y / (float)(ClientSize.Height / 2) - 1f)), cam.GetViewMatrix().Inverted());
            tempLine.IsPortalable = true;
            tempLine.Models.Add(Model.CreateLine(new Vector2[2] {
                rayBegin - player.Transform.Position, 
                rayEnd - player.Transform.Position
                }));

            if (InputExt.MousePress(MouseButton.Left))
            {
                PortalPlacer.PortalPlace(portal1, new Line(rayBegin, rayEnd));
            }
            

            text2.Models.Clear();
            text2.Models.Add(FontRenderer.GetModel(GC.GetTotalMemory(false).ToString()));
            */
            
            
            if (Focused)
            {
                if (InputExt.KeyPress(Key.Escape))
                {
                    Exit();
                }
                if (InputExt.KeyPress(Key.X))
                {
                    scene.Save();
                }
                if (InputExt.KeyPress(Key.C))
                {
                    scene = Scene.Load();
                    renderer.RenderScenes.Clear();
                    renderer.RenderScenes.Add(scene);
                }
                #region camera movement
                
                Vector3 v = new Vector3();
                float camSpeed = .05f;
                if (InputExt.KeyDown(Key.ShiftLeft))
                {
                    camSpeed = .005f;
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
                    v += cam.GetRight() * camSpeed * cam.Transform.Scale.X;
                }
                if (InputExt.MouseWheelDelta() != 0)
                {
                    cam.Scale /= (float)Math.Pow(1.2, InputExt.MouseWheelDelta());
                }
                else if (InputExt.KeyDown(Key.Q))
                {
                    cam.Scale /= (float)Math.Pow(1.04, 1);
                }
                else if (InputExt.KeyDown(Key.E))
                {
                    cam.Scale /= (float)Math.Pow(1.04, -1);
                }
                Vector2[] vArray = new Vector2[2];
                IntersectPoint i = new IntersectPoint();
                Portal portalEnter = null;

                Vector2 posPrev = player.Transform.WorldPosition;
                player.Transform.Position += new Vector2(v.X, v.Y);
                player.PositionUpdate();
                foreach (Portal p in scene.PortalList)
                {
                    vArray = p.GetWorldVerts();
                    portalEnter = p;
                    Vector2 v1 = player.Transform.WorldPosition;//new Vector2(player.Transform.Position.X, player.Transform.Position.Y);
                    i = MathExt.LineIntersection(vArray[0], vArray[1], posPrev, player.Transform.Position, true);
                    if (i.Exists)
                    {
                        break;
                    }
                }
                
                if (i.Exists)
                {
                    portalEnter.Enter(player.Transform);
                }

                Transform transformNew = player.Transform.GetWorld3D();
                cam.Transform.Position = transformNew.Position;
                cam.Transform.Rotation = transformNew.Rotation;
                cam.Transform.Scale = transformNew.Scale;
                cam.Viewpoint = player.Transform.WorldPosition;
                
                #endregion
            }
            
            /*box2.Transform.Rotation -= 0.01f;
            box2.Transform.Position = new Vector2(1f, 0f);
            boxChild.Transform.Parent = null;*/
            /*portal3.Transform.Rotation = 0f;
            portal3.Transform.Position = new Vector2(1, 0.5f);
            portal3.Transform.Scale = new Vector2(1, 1);
            portal3.Transform.Parent = boxChild.Transform;*/

            scene.PortalList[1].Transform.Rotation += 0.01f;
            //scene.EntityList[1].Transform.Position = new Vector2(1f, -2f);
            //scene.EntityList[1].Transform.Scale = new Vector2(-1f, -1f);
            scene.Step();

            //get rid of all ibo elements no longer used
            lock ("delete")
            {
                foreach (int iboElement in iboGarbage.ToArray())
                {
                    int a = iboElement;
                    GL.DeleteBuffers(1, ref a);
                }
                iboGarbage.Clear();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Log.Close();
            soundPlayer.Dispose();
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
