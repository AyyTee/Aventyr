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
        FloatPortal portal2, portal3;
        FixturePortal portal0, portal1;
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
            Camera2D hudCam = new Camera2D(hud, new Vector2(CanvasSize.Width / 2, CanvasSize.Height / 2), (float)CanvasSize.Height, CanvasSize.Width / (float)CanvasSize.Height);

            Model background = ModelFactory.CreatePlane();
            background.Texture = Renderer.Textures["grid.png"];
            background.Transform.Position = new Vector3(0, 0, -10f);
            float size = 100;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUv.Scale = new Vector2(size, size);
            Entity back = new Entity(scene, new Vector2(0f, 0f));
            back.AddModel(background);

            Entity portalEntity2 = new Entity(scene);
            portalEntity2.AddModel(ModelFactory.CreatePlane());
            portalEntity2.ModelList[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity2.ModelList[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity2.AddModel(ModelFactory.CreatePlane());
            portalEntity2.ModelList[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);
            Transform2D transform = portalEntity2.GetTransform();
            transform.Rotation = 0.1f;
            transform.Position = new Vector2(2.1f, 2f);
            transform.Scale = new Vector2(-1f, 1f);
            portalEntity2.SetTransform(transform);

            portal2 = new FloatPortal(scene);
            portal2.SetParent(portalEntity2);
            portal2.IsMirrored = true;

            Entity portalEntity3 = new Entity(scene);
            portalEntity3.AddModel(ModelFactory.CreatePlane());
            portalEntity3.ModelList[0].Transform.Scale = new Vector3(0.1f, 0.05f, 1);
            portalEntity3.ModelList[0].Transform.Position = new Vector3(0.05f, 0.4f, 0.5f);
            portalEntity3.AddModel(ModelFactory.CreatePlane());
            portalEntity3.ModelList[1].Transform.Scale = new Vector3(0.05f, 1, 0.5f);
            portalEntity3.SetTransform(new Transform2D(new Vector2(-1f, 1), 0.4f));

            portal3 = new FloatPortal(scene);
            portal3.SetParent(portalEntity3);

            Portal.SetLinked(portal2, portal3);

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
            });
            playerModel.SetTexture(Renderer.Textures["default.png"]);
            player.IsPortalable = true;
            player.AddModel(playerModel);
            playerModel.SetTexture(Renderer.Textures["default.png"]);
            #endregion

            Entity temp = new Entity(scene);
            temp.SetParent(player);
            temp.AddModel(ModelFactory.CreateArrow(new Vector3(), new Vector2(0, 2), 0.1f, 0.5f, 0.5f));
            portalEntity2.SetParent(temp);
            //portalEntity3.SetParent(temp);

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


            Actor ground = ActorFactory.CreateEntityPolygon(scene, new Transform2D(), v);
            ground.Name = "ground";
            //ground.IsPortalable = true;
            //ground.AddModel(ModelFactory.CreatePolygon(v));
            ground.SetTransform(new Transform2D(new Vector2(0, -4f), 0.05f));
            scene.World.ProcessChanges();
            portal0 = new FixturePortal(scene, null);
            portal1 = new FixturePortal(scene, null);

            portal0.IsMirrored = true;

            FixturePortal.SetLinked(portal0, portal1);//*/

            text = new Entity(hud);
            text.SetTransform(new Transform2D(new Vector2(0, CanvasSize.Height)));
            text2 = new Entity(hud);
            text2.SetTransform(new Transform2D(new Vector2(0, CanvasSize.Height - 40)));

            //new Serializer().Deserialize(scene, "blah.save");
            Camera2D cam = new Camera2D(scene, new Vector2(), 10, CanvasSize.Width / (float)CanvasSize.Height);
            cam.SetParent(scene.FindByName("player"));
            scene.SetActiveCamera(cam);
            hud.SetActiveCamera(hudCam);
            renderer.AddScene(scene);
            renderer.AddScene(hud);

            new Serializer().Serialize(scene.Root, "blah");
            //Thread.Sleep(500);*/
            
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
            tempLine.SetTransform(new Transform2D(player.GetTransform().Position));

            Vector2 mousePos = scene.ActiveCamera.ScreenToWorld(InputExt.MousePos);

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
            Camera2D cam = scene.ActiveCamera;

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
                Actor box = ActorFactory.CreateEntityBox(scene, new Transform2D(mousePos, new Vector2(0.2f, 4.4f)));
                //box.Transform.Rotation = 2.5f * (float)Math.PI / 4;
            }
            if (InputExt.KeyPress(Key.ControlLeft))
            {
                ActorFactory.CreateEntityBox(scene, new Transform2D(mousePos, new Vector2(2.4f, 0.4f)));
            }
            if (InputExt.KeyPress(Key.M))
            {
                SingleStepMode = !SingleStepMode;
            }
            #region camera movement

            Vector2 v = new Vector2();
            float camSpeed = .05f;
            if (InputExt.KeyDown(InputExt.KeyBoth.Shift))
            {
                camSpeed = .005f;
            }

            Transform2D camTransform = cam.GetTransform();
            if (InputExt.KeyDown(Key.R))
            {
                //Quaternion rot = cam.Transform.Rotation;
                camTransform.Rotation += 0.01f;
                //cam.Transform.Rotation = rot;
                player.GetTransform().Rotation += .01f;
            }
            if (InputExt.KeyDown(Key.W))
            {
                v += cam.GetUp() * camSpeed * camTransform.Scale.Y;
            }
            else if (InputExt.KeyDown(Key.S))
            {
                v -= cam.GetUp() * camSpeed * camTransform.Scale.Y;
            }
            if (InputExt.KeyDown(Key.A))
            {
                v -= cam.GetRight() * camSpeed * camTransform.Scale.X;
            }
            else if (InputExt.KeyDown(Key.D))
            {
                v += cam.GetRight() * camSpeed * camTransform.Scale.X;
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
            player.SetVelocity(new Transform2D(v));
            Vector2[] vArray = new Vector2[2];

            /*IntersectPoint intersect = new IntersectPoint();
            Portal portalEnter = null;

            Vector2 posPrev = player.GetWorldTransform().Position;
            Transform2D transform = player.GetTransform();
            transform.Position += new Vector2(v.X, v.Y);
            
            player.SetTransform(transform);
            player.PositionUpdate();
            //portal3.Velocity.Position = new Vector2(-(float)Math.Cos(TimeFixedStep / 5000000) / (float)160, (float)Math.Sin(TimeFixedStep / 5000000) / (float)160);
            //portal2.Velocity.Rotation = -(float)(1 / (32 * Math.PI));
            foreach (Portal p in scene.PortalList)
            {
                if (!p.IsValid())
                {
                    continue;
                }
                vArray = p.GetWorldVerts();
                Line line = new Line(vArray);
                portalEnter = p;
                Line playerLine = new Line(posPrev, player.GetWorldTransform().Position);
                intersect = line.IntersectsParametric(p.GetVelocity().Position, p.GetVelocity().Rotation, playerLine, 5);
                if (intersect.Exists)
                {
                    break;
                }
            }

            foreach (Portal p in scene.PortalList)
            {
                p.Step();
            }

            if (intersect.Exists)
            {
                portalEnter.Enter(player);
            }*/
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
            hud.ActiveCamera.Scale = canvasSize.Height;
        }
    }
}
