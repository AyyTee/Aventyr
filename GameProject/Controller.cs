using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.ComponentModel;
using Cgen.Audio;
using System.Threading;


namespace Game
{
    public class Controller
    {
        public Controller(Window window)
            : this(window.ClientSize, window.InputExt)
        {
        }

        public Controller(Size canvasSize, InputExt input)
        {
            ContextExists = true;
            CanvasSize = canvasSize;
            InputExt = input;
        }

        public InputExt InputExt;
        /// <summary>
        /// Intended to keep pointless messages from the Poly2Tri library out of the console window
        /// </summary>
        public static StreamWriter Log = new StreamWriter("Triangulating.txt");
        public static bool ContextExists = false;
        public static Size _canvasSize;
        public const int MICROSECONDS_IN_SECOND = 1000000;
        public static Size CanvasSize
        {
            get { return _canvasSize; }
            set
            {
                _canvasSize = value;
                /*scene.ActiveCamera.Aspect = CanvasSize.Width / (float)CanvasSize.Height;
                hud.ActiveCamera.Aspect = CanvasSize.Width / (float)CanvasSize.Height;
                hud.ActiveCamera.Scale = CanvasSize.Height;*/
            }
        }
        public const int StepsPerSecond = 60;
        public const int DrawsPerSecond = 60;
        private bool SingleStepMode = false;
        Font Default;
        public static List<int> iboGarbage = new List<int>();

        public static String fontFolder = Path.Combine(new String[2] { "assets", "fonts" });
        public static String shaderFolder = Path.Combine(new String[2] { "assets", "shaders" });
        public static String textureFolder = Path.Combine(new String[2] { "assets", "textures" });
        public static String soundFolder = Path.Combine(new String[2] { "assets", "sounds" });
        Scene scene, hud;
        FontRenderer FontRenderer;
        FloatPortal portal2, portal3;
        FixturePortal portal0, portal1;
        float Time = 0.0f;
        /// <summary>
        /// The difference in seconds between the last OnUpdateEvent and the current OnRenderEvent.
        /// </summary>
        float TimeRenderDelta = 0.0f;
        Entity text, text2;
        SoundSystem soundPlayer;
        Sound testSound;
        Renderer renderer;
        public void InitProgram()
        {
            Renderer.Init();
            //soundPlayer = SoundSystem.Instance();
            if (soundPlayer != null)
            {
                soundPlayer.Init();
                testSound = new Sound("My Sound", Path.Combine(Controller.soundFolder, "test_sound.ogg"));
                //testSound.Play();
                //testSound.SetLoop(true);
                //sound.SetPosition(1000, 0, 0);
            }
            
            scene = new Scene();
            hud = new Scene();
            Camera hudCam = Camera.CameraOrtho(new Vector3(CanvasSize.Width / 2, CanvasSize.Height / 2, 0), CanvasSize.Height, CanvasSize.Width / (float)CanvasSize.Height);

            // Load textures from file
            Renderer.Textures.Add("default.png", Renderer.LoadImage(Path.Combine(textureFolder, "default.png")));
            Renderer.Textures.Add("grid.png", Renderer.LoadImage(Path.Combine(textureFolder, "grid.png")));

            System.Drawing.Text.PrivateFontCollection privateFonts = new System.Drawing.Text.PrivateFontCollection();
            privateFonts.AddFontFile(Path.Combine(fontFolder, "times.ttf"));
            Default = new Font(privateFonts.Families[0], 14);

            FontRenderer = new FontRenderer(Default);

            // Load shaders from file
            Renderer.Shaders.Add("default", new ShaderProgram(Path.Combine(shaderFolder, "vs.glsl"), Path.Combine(shaderFolder, "fs.glsl"), true));
            Renderer.Shaders.Add("textured", new ShaderProgram(Path.Combine(shaderFolder, "vs_tex.glsl"), Path.Combine(shaderFolder, "fs_tex.glsl"), true));
            Renderer.Shaders.Add("text", new ShaderProgram(Path.Combine(shaderFolder, "vs_text.glsl"), Path.Combine(shaderFolder, "fs_text.glsl"), true));

            Model background = Model.CreatePlane();
            background.TextureId = Renderer.Textures["grid.png"];
            background.Transform.Position = new Vector3(0, 0, -10f);
            float size = 100;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUv.Scale = new Vector2(size, size);
            Entity back = new Entity(scene, new Vector2(0f, 0f));
            back.Models.Add(background);

            //Portal.Link(portal1, portal1);
            /*Entity portalEntity1 = scene.CreateEntity();
            portalEntity1.Transform.Parent = portal1.Transform; 
            portalEntity1.Models.Add(Model.CreatePlane());
            portalEntity1.Models[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity1.Models[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity1.Models.Add(Model.CreatePlane());
            portalEntity1.Models[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);*/

            portal2 = new FloatPortal(scene);
            portal2.Transform.Rotation = 0.1f;
            portal2.Transform.Position = new Vector2(2.1f, 2f);
            portal2.Transform.Scale = new Vector2(-1f, 1f);

            Entity portalEntity2 = new Entity(scene);
            portalEntity2.Transform.Parent = portal2.Transform;
            portalEntity2.Models.Add(Model.CreatePlane());
            portalEntity2.Models[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity2.Models[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity2.Models.Add(Model.CreatePlane());
            portalEntity2.Models[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);
            

            portal3 = new FloatPortal(scene);
            portal3.Transform.Rotation = 0.4f;
            portal3.Transform.Position = new Vector2(-1f, 1f);
            //portal3.Transform.Scale = new Vector2(-2f, 2f);

            Portal.ConnectPortals(portal2, portal3);
            Entity portalEntity3 = new Entity(scene);
            portalEntity3.Transform.Parent = portal3.Transform;
            portalEntity3.Models.Add(Model.CreatePlane());
            portalEntity3.Models[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity3.Models[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity3.Models.Add(Model.CreatePlane());
            portalEntity3.Models[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);

            #region cubes
            /*Model tc = Model.CreateCube();
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

            //portal3.Transform.Parent = boxChild.Transform;*/
            #endregion

            #region player
            Entity player = new Entity(scene);
            player.Name = "player";
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
            playerModel.SetTexture(Renderer.Textures["default.png"]);
            player.IsPortalable = true;
            //player.Transform.Scale = new Vector2(.5f, .5f);
            player.Transform.Position = new Vector2(0f, 0f);
            player.Models.Add(playerModel);
            playerModel.SetTexture(Renderer.Textures["default.png"]);
            #endregion

            Entity playerParent = new Entity(scene, new Vector2(1, 0));
            //player.Transform.Parent = playerParent.Transform;

            Entity tempLine = new Entity(scene);
            tempLine.Name = "tempLine";

            /*intersectDot = scene.CreateEntity();
            //intersectDot.Models.Add(Model.CreateCube());
            intersectDot.Models = portalEntity0.Models;
            intersectDot.Transform.Scale = new Vector2(1f, 1f);*/

            Vector2[] v = new Vector2[] {
                new Vector2(-8, -1),
                new Vector2(2, -1),
                new Vector2(3, 0),
                new Vector2(3, -2),
                new Vector2(-10, -2)
            };
            
            Entity ground = EntityFactory.CreateEntityPolygon(scene, new Transform2D(), v);
            ground.Name = "ground";
            //ground.IsPortalable = true;
            ground.Models.Add(Model.CreatePolygon(v));
            ground.Transform.Rotation = 0.05f;
            ground.Transform.Position = new Vector2(0, -4f);
            scene.World.ProcessChanges();
            portal1 = new FixturePortal(scene, new FixtureEdgeCoord(ground.Body.FixtureList[0], 1, 0.3f));

            portal0 = new FixturePortal(scene, new FixtureEdgeCoord(ground.Body.FixtureList[0], 1, 0.6f));
            portal0.IsMirrored = true;
            /*Entity portalEntity0 = scene.CreateEntity();
            portalEntity0.Transform.Parent = portal0.Transform;
            portalEntity0.Models.Add(Model.CreatePlane());
            portalEntity0.Models[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity0.Models[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity0.Models.Add(Model.CreatePlane());
            portalEntity0.Models[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);
            */
            
            FixturePortal.ConnectPortals(portal0, portal1);
            
            Entity origin = EntityFactory.CreateEntityBox(scene, new Transform2D(new Vector2(0.4f, 0f), new Vector2(1.5f, 1.5f)));
            //scene.CreateEntityBox(new Vector2(0.4f, 0f), new Vector2(1.5f, 1.5f));
            
            text = new Entity(hud);
            text.Transform.Position = new Vector2(0, CanvasSize.Height);
            text2 = new Entity(hud);
            text2.Transform.Position = new Vector2(0, CanvasSize.Height - 40);

            Camera cam = Camera.CameraOrtho(new Vector3(player.Transform.Position.X, player.Transform.Position.Y, 10f), 10, CanvasSize.Width / (float)CanvasSize.Height);
            
            scene.ActiveCamera = cam;
            hud.ActiveCamera = hudCam;
            renderer = new Renderer(this);
            renderer.RenderScenes.Add(scene);
            renderer.RenderScenes.Add(hud);
        }

        public void OnLoad(EventArgs e)
        {
            InitProgram();
        }

        public void OnRenderFrame(FrameEventArgs e)
        {
            TimeRenderDelta += (float)e.Time;
            text.Models.Clear();
            text.Models.Add(FontRenderer.GetModel(((float)e.Time).ToString(), new Vector2(0f, 0f), 0));

            renderer.Render();
            /*int sleepTimeSpan = (int)Math.Max((1 / 60 - TimeRenderDelta) * 1000, 0);
            System.Threading.Thread.Sleep(sleepTimeSpan);*/
        }

        public void OnUpdateFrame(FrameEventArgs e)
        {
            Time += MICROSECONDS_IN_SECOND / (float)StepsPerSecond;//(float)e.Time;
            TimeRenderDelta = 0;
            
            Entity player = scene.GetEntityByName("player");
            Entity tempLine = scene.GetEntityByName("tempLine");
            Entity ground = scene.GetEntityByName("ground");
            ground.Velocity.Position = new Vector2((float)Math.Sin(Time), 0);
            tempLine.Transform.Position = player.Transform.Position;

            Vector2 mousePos = scene.ActiveCamera.ScreenToWorld(InputExt.MousePos);

            Vector2 rayBegin = player.Transform.Position;
            Vector2 rayEnd = mousePos;
            tempLine.IsPortalable = true;
            tempLine.Models.Clear();
            tempLine.Models.Add(Model.CreateLine(new Vector2[2] {
                rayBegin - player.Transform.Position, 
                rayEnd - player.Transform.Position
                }));

            
            text2.Models.Clear();
            //text2.Models.Add(FontRenderer.GetModel(GC.GetTotalMemory(false).ToString()));
            //text2.Models.Add(FontRenderer.GetModel(scene.PhysWorld.BodyList.Count.ToString()));
            int fixtureCount = WorldExt.GetFixtures(scene.World).Count;
            int bodyCount = scene.World.BodyList.Count;
            //text2.Models.Add(FontRenderer.GetModel(fixtureCount.ToString() + "        " + bodyCount.ToString()));
            Camera cam = scene.ActiveCamera;
            
            if (InputExt.MousePress(MouseButton.Left))
            {
                PortalPlacer.PortalPlace(portal1, new Line(rayBegin, rayEnd));
            }
            else if (InputExt.MousePress(MouseButton.Right))
            {
                PortalPlacer.PortalPlace(portal0, new Line(rayBegin, rayEnd));
            }
            if (InputExt.KeyPress(Key.Space))
            {
                Entity box = EntityFactory.CreateEntityBox(scene, new Transform2D(mousePos, new Vector2(0.2f, 4.4f)));
                //box.Transform.Rotation = 2.5f * (float)Math.PI / 4;
            }
            if (InputExt.KeyPress(Key.ControlLeft))
            {
                EntityFactory.CreateEntityBox(scene, new Transform2D(mousePos, new Vector2(2.4f, 0.4f)));
            }
            
            if (InputExt.KeyPress(Key.X))
            {
                scene.Save();
            }
            if (InputExt.KeyPress(Key.C))
            {
                renderer.RenderScenes.Remove(scene);
                scene = Scene.Load();
                renderer.RenderScenes.Insert(0, scene);
            }
            if (InputExt.KeyPress(Key.M))
            {
                SingleStepMode = !SingleStepMode;
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
            IntersectPoint intersect = new IntersectPoint();
            Portal portalEnter = null;

            Vector2 posPrev = player.Transform.WorldPosition;
            player.Transform.Position += new Vector2(v.X, v.Y);
            player.PositionUpdate();
            portal3.Velocity.Position = new Vector2(-(float)Math.Cos(Time / 5000000) / (float)160, (float)Math.Sin(Time / 5000000) / (float)160);
            //portal2.Velocity.Rotation = -(float)(1 / (32 * Math.PI));
            foreach (Portal p in scene.PortalList)
            {
                vArray = p.GetWorldVerts();
                Line line = new Line(vArray);
                portalEnter = p;
                Line playerLine = new Line(posPrev, player.Transform.WorldPosition);
                intersect = line.IntersectsParametric(p.GetVelocity().Position, p.GetVelocity().Rotation, playerLine, 5);
                if (intersect.Exists)
                {
                    break;
                }
            }

            foreach (Portal p in scene.PortalList)
            {
                if (p.GetType() == typeof(FloatPortal))
                {
                    FloatPortal portal = (FloatPortal)p;
                    portal.Transform.Position += portal.Velocity.Position;
                    portal.Transform.Rotation += portal.Velocity.Rotation;
                    portal.Transform.Scale *= portal.Velocity.Scale;
                }
            }

            if (intersect.Exists)
            {
                portalEnter.Enter(player);
            }
            #endregion

            if (SingleStepMode == false || InputExt.KeyPress(Key.Enter))
            {
                scene.Step();
            }
            Transform transformNew = player.Transform.GetWorld3D();
            cam = scene.ActiveCamera;
            cam.Transform.Position = transformNew.Position;
            cam.Transform.Rotation = transformNew.Rotation;
            cam.Transform.Scale = transformNew.Scale;
            cam.Viewpoint = player.Transform.WorldPosition;
            
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

        public void OnClosing(CancelEventArgs e)
        {
            Log.Close();
            if (soundPlayer != null)
            {
                soundPlayer.Dispose();
            }
            File.Delete("Triangulating.txt");
        }

        public void OnResize(EventArgs e, Size canvasSize)
        {
            CanvasSize = canvasSize;
            scene.ActiveCamera.Aspect = CanvasSize.Width / (float)CanvasSize.Height;
            hud.ActiveCamera.Aspect = CanvasSize.Width / (float)CanvasSize.Height;
        }
    }
}
