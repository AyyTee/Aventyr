using Game;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class ControllerEditor : Controller
    {
        public Scene Level, Hud;
        bool _isPaused;
        ControllerCamera _camControl;
        public delegate void EntityHandler(ControllerEditor controller, Entity entity);
        public event EntityHandler EntityAdded;
        public event EntityHandler EntitySelected;
        public delegate void SceneEventHandler(ControllerEditor controller, Scene scene);
        public event SceneEventHandler ScenePaused;
        public event SceneEventHandler ScenePlayed;
        public event SceneEventHandler SceneStopped;
        public delegate void ToolEventHandler(ControllerEditor controller, Tool tool);
        public event ToolEventHandler ToolChanged;
        Entity debugText;
        Entity _selectedEntity;
        Entity _gripper;
        Tool _activeTool;
        Tool _toolDefault;
        Tool _nextTool;
        List<Entity> Entities = new List<Entity>();
        Model _entiyMarker;

        public ControllerEditor(Window window)
            : base(window)
        {
            
        }

        public ControllerEditor(Size canvasSize, InputExt input)
            : base(canvasSize, input)
        {
            _toolDefault = new ToolDefault(this);
            _activeTool = _toolDefault;
            _nextTool = _activeTool;
        }

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Level = new Scene();
            renderer.AddScene(Level);
            Hud = new Scene();
            renderer.AddScene(Hud);

            _gripper = new Entity(Level);
            _gripper.Models.Add(ModelFactory.CreateCircle(new Vector3(0, 0, 10f), 0.1f, 16));
            _gripper.Visible = false;
            //_gripper.Models[0].SetTexture(Renderer.Textures["default.png"]);

            Model background = ModelFactory.CreatePlane();
            background.TextureId = Renderer.Textures["grid.png"];
            background.Transform.Position = new Vector3(0, 0, -10f);
            float size = 100;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUv.Scale = new Vector2(size, size);
            Entity back = new Entity(Level, new Vector2(0f, 0f));
            back.Models.Add(background);

            _entiyMarker = ModelFactory.CreateCircle(new Vector3(0, 0, 10), 0.05f, 10);

            /*FloatPortal portal = new FloatPortal(Level);
            portal.Transform.Rotation = 4f;
            portal.Transform.Position = new Vector2(1, 1);
            FloatPortal portal2 = new FloatPortal(Level);
            portal2.Transform.Position = new Vector2(-1, 0);
            Portal.ConnectPortals(portal, portal2);*/

            Level.ActiveCamera = Camera.CameraOrtho(new Vector3(0, 0, 10f), 10, CanvasSize.Width / (float)CanvasSize.Height); ;

            _camControl = new ControllerCamera(Level.ActiveCamera, InputExt);

            
            Hud.ActiveCamera = Camera.CameraOrtho(new Vector3(CanvasSize.Width / 2, CanvasSize.Height / 2, 0), CanvasSize.Height, CanvasSize.Width / (float)CanvasSize.Height);

            debugText = new Entity(Hud);
            debugText.Transform.Position = new Vector2(0, CanvasSize.Height - 40);

            ScenePause();
        }

        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            debugText.Models.Clear();
            debugText.Models.Add(FontRenderer.GetModel((Time.ElapsedMilliseconds / RenderCount).ToString()));
        }

        public Vector2 GetMouseWorldPosition()
        {
            Camera cam = Level.ActiveCamera;
            return cam.ScreenToWorld(InputExt.MousePos);
        }

        public void AddLevelEntity(Entity entity)
        {
            Entities.Add(entity);
            Entity marker = new Entity(Level);
            marker.Transform.Parent = entity.Transform;
            marker.Models.Add(_entiyMarker);
            if (EntityAdded != null)
            {
                EntityAdded(this, entity);
            }
        }

        public void RemoveLevelEntity(Entity entity)
        {
            Entities.Remove(entity);
            Level.RemoveEntity(entity);
            if (GetSelectedEntity() == entity)
            {
                SetSelectedEntity(null);
            }
        }

        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            _camControl.Update();
            
            _setTool(_nextTool);
            _activeTool.Update();
            if (!_isPaused)
            {
                Level.Step();
            }
        }

        private void _setTool(Tool tool)
        {
            Debug.Assert(tool != null, "Tool cannot be null.");
            if (_activeTool == _nextTool)
            {
                return;
            }
            _activeTool.Disable();
            _activeTool = tool;
            _activeTool.Enable();
            if (ToolChanged != null)
            {
                ToolChanged(this, tool);
            }
        }

        public void SetTool(Tool tool)
        {
            if (tool == null)
            {
                _nextTool = _toolDefault;
            }
            else
            {
                _nextTool = tool;
            }
        }

        public Entity GetNearestEntity()
        {
            Vector2 mouseWorldPos = Level.ActiveCamera.ScreenToWorld(InputExt.MousePos);
            var sorted = Entities.OrderBy(item => (mouseWorldPos - item.Transform.Position).Length);
            if (sorted.ToArray().Length > 0)
            {
                Entity nearest = sorted.ToArray()[0];
                if ((mouseWorldPos - nearest.Transform.Position).Length < 1)
                {
                    return nearest;
                }
            }
            return null;
        }

        public void SetSelectedEntity(Entity selected)
        {
            _selectedEntity = selected;
            if (selected != null)
            {
                _gripper.Visible = true;
                _gripper.Transform.Position = _selectedEntity.Transform.Position;
            }
            else
            {
                _gripper.Visible = false;
            }
            if (EntitySelected != null)
                EntitySelected(this, selected);
        }

        public Entity GetSelectedEntity()
        {
            return _selectedEntity;
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
