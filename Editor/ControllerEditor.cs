using Game;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class ControllerEditor : Controller
    {
        Scene Level, Hud;
        bool _isPaused;
        ControllerCamera _camControl;
        public delegate void EntityAddedHandler(ControllerEditor controller, Entity entity);
        public event EntityAddedHandler EntityAdded;
        public delegate void SceneEventHandler(ControllerEditor controller, Scene scene);
        public event SceneEventHandler ScenePaused;
        public event SceneEventHandler ScenePlayed;
        public event SceneEventHandler SceneStopped;
        Entity debugText;

        public ControllerEditor(Window window)
            : base(window)
        {
        }

        public ControllerEditor(Size canvasSize, InputExt input)
            : base(canvasSize, input)
        {
        }

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Level = new Scene();
            renderer.AddScene(Level);
            Hud = new Scene();
            renderer.AddScene(Hud);

            Model background = Model.CreatePlane();
            background.TextureId = Renderer.Textures["grid.png"];
            background.Transform.Position = new Vector3(0, 0, -10f);
            float size = 100;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUv.Scale = new Vector2(size, size);
            Entity back = new Entity(Level, new Vector2(0f, 0f));
            back.Models.Add(background);

            FloatPortal portal = new FloatPortal(Level);
            portal.Transform.Rotation = 4f;
            portal.Transform.Position = new Vector2(1, 1);
            FloatPortal portal2 = new FloatPortal(Level);
            portal2.Transform.Position = new Vector2(-1, 0);
            Portal.ConnectPortals(portal, portal2);

            Level.ActiveCamera = Camera.CameraOrtho(new Vector3(0, 0, 10f), 10, CanvasSize.Width / (float)CanvasSize.Height); ;

            _camControl = new ControllerCamera(Level.ActiveCamera, InputExt);

            
            Hud.ActiveCamera = Camera.CameraOrtho(new Vector3(CanvasSize.Width / 2, CanvasSize.Height / 2, 0), CanvasSize.Height, CanvasSize.Width / (float)CanvasSize.Height);

            debugText = new Entity(Hud);
            debugText.Transform.Position = new Vector2(0, CanvasSize.Height - 40);
        }

        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            debugText.Models.Clear();
            debugText.Models.Add(FontRenderer.GetModel((Time.ElapsedMilliseconds / RenderCount).ToString()));
        }

        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            _camControl.Update();
            if (InputExt.MouseInside)
            {
                if (InputExt.MousePress(MouseButton.Right))
                {
                    Camera cam = Level.ActiveCamera;
                    Entity entity = new Entity(Level);
                    entity.Transform.Position = cam.ScreenToWorld(InputExt.MousePos);
                    entity.Models.Add(Model.CreateCube());
                    if (EntityAdded != null)
                    {
                        EntityAdded(this, entity);
                    }
                }
            }
            if (!_isPaused)
            {
                Level.Step();
            }
        }

        public void ScenePlay()
        {
            _isPaused = false;
            if (ScenePlayed != null)
                ScenePlayed(this, Level);
        }

        public void ScenePause()
        {
            _isPaused = true;
            if (ScenePaused != null)
                ScenePaused(this, Level);
        }

        public void SceneStop()
        {
            _isPaused = true;
            if (SceneStopped != null)
                SceneStopped(this, Level);
        }

        public override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        public override void OnResize(EventArgs e, System.Drawing.Size canvasSize)
        {
            base.OnResize(e, canvasSize);
            Level.ActiveCamera.Aspect = CanvasSize.Width / (float)CanvasSize.Height;
            Hud.ActiveCamera.Aspect = CanvasSize.Width / (float)CanvasSize.Height;
        }
    }
}
