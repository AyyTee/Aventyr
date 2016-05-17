using Cgen.Audio;
using FarseerPhysics.Dynamics;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Game
{
    public class ControllerGame : Controller
    {
        Entity text, text2;
        Sound testSound;
        Scene scene, hud;
        Camera2 camera;
        private bool SingleStepMode = false;

        public ControllerGame(Window window)
            :base(window)
        {
        }

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (SoundEnabled)
            {
                testSound = new Sound(Path.Combine(Controller.soundFolder, "test_sound.ogg"));
                testSound.Play();
            }
            
            scene = new Scene();
            hud = new Scene();
            Camera2 hudCam = new Camera2(hud, new Transform2(new Vector2(CanvasSize.Width / 2, CanvasSize.Height / 2), (float)CanvasSize.Height), CanvasSize.Width / (float)CanvasSize.Height);
            #region create scene
            ///*
            Model background = ModelFactory.CreatePlane();
            background.Texture = Renderer.Textures["grid.png"];
            background.Transform.Position = new Vector3(0, 0, -10f);
            float size = 100;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUv.Size = size;
            Entity back = new Entity(scene, new Vector2(0f, 0f));
            back.AddModel(background);

            Entity portalEntity2 = new Entity(scene);
            portalEntity2.AddModel(ModelFactory.CreatePlane());
            portalEntity2.ModelList[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity2.ModelList[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity2.AddModel(ModelFactory.CreatePlane());
            portalEntity2.ModelList[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);
            Transform2 transform = portalEntity2.GetTransform();
            transform.Rotation = 1f;
            transform.Position = new Vector2(2.1f, 2f);
            transform.Size = 1.5f;
            transform.IsMirrored = true;
            portalEntity2.SetTransform(transform);

            FloatPortal portal2 = new FloatPortal(scene);
            portal2.SetParent(portalEntity2);
            portal2.OneSided = true;
            portal2.Name = "portal2";

            Entity portalEntity3 = new Entity(scene);
            portalEntity3.IsPortalable = false;
            portalEntity3.AddModel(ModelFactory.CreatePlane());
            portalEntity3.ModelList[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity3.ModelList[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity3.AddModel(ModelFactory.CreatePlane());
            portalEntity3.ModelList[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);
            portalEntity3.SetTransform(new Transform2(new Vector2(-1f, 1)));
            //portalEntity3.SetVelocity(new Transform2D(new Vector2(), 0.005f));

            FloatPortal portal3 = new FloatPortal(scene);
            portal3.SetParent(portalEntity3);
            portal3.OneSided = true;
            
            //portal3.IsMirrored = true;
            portal2.Linked = portal3;
            portal3.Linked = portal2;

            #region player
            Entity player = new Entity(scene);
            player.Name = "player";
            Model playerModel = ModelFactory.CreatePolygon(new Vector2[] {
                new Vector2(0.5f, 0), 
                new Vector2(0.35f, 0.15f), 
                new Vector2(0.15f, 0.15f), 
                new Vector2(0.15f, 0.35f), 
                new Vector2(0, 0.5f), 
                new Vector2(-0.5f, 0), 
                new Vector2(0, -0.5f)
            }, new Vector3(0, 0, 1));
            playerModel.SetTexture(Renderer.Textures["default.png"]);
            player.IsPortalable = true;
            player.AddModel(playerModel);
            playerModel.SetTexture(Renderer.Textures["default.png"]);
            #endregion

            Entity temp = new Entity(scene);
            //temp.SetParent(player);
            temp.AddModel(ModelFactory.CreateArrow(new Vector3(), new Vector2(0, 2), 0.1f, 0.5f, 0.5f));
            //portalEntity2.SetParent(temp);
            portalEntity3.SetParent(temp);

            //Entity playerNew = (Entity)player.DeepClone();

            //temp.SetParent(null);

            Entity playerParent = new Entity(scene, new Vector2(1, 0));

            Entity tempLine = new Entity(scene);
            tempLine.Name = "tempLine";

            Vector2[] v = new Vector2[] {
                new Vector2(-8, -1),
                new Vector2(2, -1),
                new Vector2(3, 0),
                new Vector2(3, -2),
                new Vector2(-10, -2)
            };


            Actor ground = ActorFactory.CreateEntityPolygon(scene, new Transform2(), v);
            ground.Name = "ground";
            //ground.IsPortalable = true;
            //ground.AddModel(ModelFactory.CreatePolygon(v));
            ground.SetTransform(new Transform2(new Vector2(0, -4f), 1f, 0.05f));
            scene.World.ProcessChanges();
            FixturePortal portal0 = new FixturePortal(scene);
            portal0.Name = "portalLeft";
            FixturePortal portal1 = new FixturePortal(scene);
            portal1.Name = "portalRight";

            portal0.IsMirrored = true;
            /*portal0.Linked = portal1;
            portal1.Linked = portal0;*/

            text = new Entity(hud);
            text.SetTransform(new Transform2(new Vector2(0, CanvasSize.Height)));
            text2 = new Entity(hud);
            text2.SetTransform(new Transform2(new Vector2(0, CanvasSize.Height - 40)));
            #endregion
            //new Serializer().Deserialize(scene, "blah.xml", "blah_phys.xml");
            camera = new Camera2(scene, new Transform2(new Vector2(), 10), CanvasSize.Width / (float)CanvasSize.Height);
            //Camera2 cam = new Camera2(scene, new Transform2(new Vector2(), 1), 1);
            //cam.SetRotation((float)Math.PI / 2);
            camera.SetParent(scene.FindByName("player"));
            scene.SetActiveCamera(camera);
            hud.SetActiveCamera(hudCam);
            renderer.AddLayer(scene);
            renderer.AddLayer(hud);

            //new Serializer().Serialize(scene.Root, "blah.xml", "blah_phys.xml");
        }

        public override void OnRenderFrame(FrameEventArgs e)
        {
            text.RemoveAllModels();
            text.AddModel(FontRenderer.GetModel(((float)e.Time).ToString(), new Vector2(0f, 0f), 0));
            base.OnRenderFrame(e);
            text2.AddModel(FontRenderer.GetModel((Time.ElapsedMilliseconds / RenderCount).ToString()));
        }

        public override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            Entity player = scene.FindByName("player") as Entity;
            Entity tempLine = scene.FindByName("tempLine") as Entity;
            Entity ground = scene.FindByName("ground") as Entity;
            //ground.Velocity.Position = new Vector2((float)Math.Sin(TimeFixedStep), 0);
            tempLine.SetTransform(new Transform2(player.GetTransform().Position));

            Vector2 mousePos = CameraExt.ScreenToWorld(scene.ActiveCamera, InputExt.MousePos);

            Vector2 rayBegin = player.GetTransform().Position;
            Vector2 rayEnd = mousePos;
            tempLine.IsPortalable = true;
            tempLine.RemoveAllModels();
            tempLine.AddModel(ModelFactory.CreateLineStrip(new Vector2[2] {
                rayBegin - player.GetTransform().Position, 
                rayEnd - player.GetTransform().Position
                }));

            text2.RemoveAllModels();
            //text2.Models.Add(FontRenderer.GetModel(GC.GetTotalMemory(false).ToString()));
            //text2.Models.Add(FontRenderer.GetModel(scene.PhysWorld.BodyList.Count.ToString()));
            int fixtureCount = WorldExt.GetFixtures(scene.World).Count;
            int bodyCount = scene.World.BodyList.Count;
            //text2.Models.Add(FontRenderer.GetModel(fixtureCount.ToString() + "        " + bodyCount.ToString()));
            ICamera2 cam = scene.ActiveCamera;

            if (InputExt.MousePress(MouseButton.Left))
            {
                PortalPlacer.PortalPlace((FixturePortal)scene.FindByName("portalLeft"), new Line(rayBegin, rayEnd));
            }
            else if (InputExt.MousePress(MouseButton.Right))
            {
                PortalPlacer.PortalPlace((FixturePortal)scene.FindByName("portalRight"), new Line(rayBegin, rayEnd));
            }
            if (InputExt.KeyPress(Key.Space))
            {
                Actor box = ActorFactory.CreateEntityBox(scene, new Transform2(mousePos, 1));
                //box.Transform.Rotation = 2.5f * (float)Math.PI / 4;
            }
            if (InputExt.KeyPress(Key.ControlLeft))
            {
                ActorFactory.CreateEntityBox(scene, new Transform2(mousePos, 1));
            }
            if (InputExt.KeyPress(Key.M))
            {
                SingleStepMode = !SingleStepMode;
            }
            #region camera movement
            float camSpeed = .05f;
            if (InputExt.KeyDown(InputExt.KeyBoth.Shift))
            {
                camSpeed = .005f;
            }
            Vector2 v = new Vector2();
            Transform2 transform = player.GetTransform();
            camSpeed *= Math.Abs(transform.Size);
            Vector2 up = transform.GetUp();
            Vector2 right = transform.GetRight();
            if (InputExt.KeyDown(Key.R))
            {
                Transform2.SetRotation(player, Transform2.GetRotation(player) + 0.1f);
            }
            if (InputExt.KeyDown(Key.W))
            {
                v += up * camSpeed;
            }
            else if (InputExt.KeyDown(Key.S))
            {
                v += -up * camSpeed;
            }
            if (InputExt.KeyDown(Key.A))
            {
                v += -right * camSpeed;
            }
            else if (InputExt.KeyDown(Key.D))
            {
                v += right * camSpeed;
            }
            /*if (InputExt.MousePress(MouseButton.Left))
            {
                Entity bullet = new Entity(scene, new Transform2D(player.GetTransform().Position));
                bullet.AddModel(ModelFactory.CreateCircle(new Vector3(), 0.1f, 10));
                Transform2D velocity = new Transform2D();
                velocity.Position = (rayEnd - rayBegin).Normalized() * 5;
                bullet.SetVelocity(velocity);
            }*/

            if (InputExt.MouseWheelDelta() != 0)
            {
                Transform2.SetScale(camera, Transform2.GetScale(camera) / (float)Math.Pow(1.2, InputExt.MouseWheelDelta()));
            }
            else if (InputExt.KeyDown(Key.Q))
            {
                Transform2.SetScale(camera, Transform2.GetScale(camera) / (float)Math.Pow(1.04, 1));
                //cam.Zoom /= (float)Math.Pow(1.04, 1);
            }
            else if (InputExt.KeyDown(Key.E))
            {
                Transform2.SetScale(camera, Transform2.GetScale(camera) / (float)Math.Pow(1.04, -1));
                //cam.Zoom /= (float)Math.Pow(1.04, -1);
            }
            /*if (InputExt.KeyPress(Key.F))
            {
                FloatPortal portal2 = (FloatPortal)scene.FindByName("portal2");
                portal2.IsMirrored = !portal2.IsMirrored;
            }*/
            player.SetVelocity(new Transform2(v));
            #endregion

            if (SingleStepMode == false || InputExt.KeyPress(Key.Enter))
            {
                scene.Step();
            }
        }

        public override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Log.Close();
            File.Delete("Triangulating.txt");
        }

        public override void OnResize(EventArgs e, Size canvasSize)
        {
            base.OnResize(e, canvasSize);
            scene.ActiveCamera.Aspect = CanvasSize.Width / (float)CanvasSize.Height;
            hud.ActiveCamera.Aspect = CanvasSize.Width / (float)CanvasSize.Height;
            Transform2.SetSize((ITransformable2)hud.ActiveCamera, CanvasSize.Height);
        }
    }
}
