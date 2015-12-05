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
using System.Windows.Forms;

namespace Editor
{
    public class ControllerEditor : Controller
    {
        public Scene Level, Hud;
        bool _isPaused;
        ControllerCamera _camControl;
        public delegate void EditorObjectHandler(ControllerEditor controller, EditorObject entity);
        public event EditorObjectHandler EntityAdded;
        public event EditorObjectHandler EntitySelected;
        public delegate void SceneEventHandler(ControllerEditor controller, Scene scene);
        public event SceneEventHandler ScenePaused;
        public event SceneEventHandler ScenePlayed;
        public event SceneEventHandler SceneStopped;
        public delegate void ToolEventHandler(ControllerEditor controller, Tool tool);
        public event ToolEventHandler ToolChanged;
        Entity debugText;
        EditorObject _selectedEntity;
        Entity _gripper;
        Tool _activeTool;
        Tool _toolDefault;
        Tool _nextTool;
        List<EditorEntity> Entities = new List<EditorEntity>();
        List<EditorPortal> Portals = new List<EditorPortal>();
        public Queue<Action> Actions = new Queue<Action>();

        /*public ControllerEditor(Window window)
            : base(window)
        {
            
        }*/

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
            background.Texture = Renderer.Textures["grid.png"];
            background.Transform.Position = new Vector3(0, 0, -10f);
            float size = 100;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUv.Scale = new Vector2(size, size);
            Entity back = new Entity(Level, new Vector2(0f, 0f));
            back.Models.Add(background);

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

        /*public void SetMouseWorldPosition(Vector2 mousePosition)
        {
            Level.ActiveCamera.WorldToScreen(mousePosition);
            //System.Windows.Forms.Cursor.Position = mousePosition;
        }*/

        public EditorEntity CreateLevelEntity()
        {
            EditorEntity entity = new EditorEntity(Level);
            Entities.Add(entity);
            
            if (EntityAdded != null)
            {
                EntityAdded(this, entity);
            }
            return entity;
        }

        public void Remove(EditorObject editorObject)
        {
            if (editorObject.GetType() == typeof(EditorEntity))
            {
                EditorEntity entity = (EditorEntity)editorObject;
                Entities.Remove(entity);
                entity.Remove();
            }
            else if (editorObject.GetType() == typeof(EditorPortal))
            {
                EditorPortal portal = (EditorPortal)editorObject;
                Portals.Remove(portal);
                portal.Remove();
            }
            if (GetSelectedEntity() == editorObject)
            {
                SetSelectedEntity(null);
            }
        }

        public EditorPortal CreateLevelPortal()
        {
            EditorPortal portal = new EditorPortal(Level);
            Portals.Add(portal);
            return portal;
        }

        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            foreach (Action item in Actions)
            {
                item();
            }
            Actions.Clear();
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

        public EditorObject GetNearestEntity()
        {
            Vector2 mouseWorldPos = Level.ActiveCamera.ScreenToWorld(InputExt.MousePos);
            List<EditorObject> tempList = new List<EditorObject>();
            tempList.AddRange(Entities);
            tempList.AddRange(Portals);
            var sorted = tempList.OrderBy(item => (mouseWorldPos - item.GetTransform().Position).Length);
            if (sorted.ToArray().Length > 0)
            {
                EditorObject nearest = sorted.ToArray()[0];
                if ((mouseWorldPos - nearest.GetTransform().Position).Length < 1)
                {
                    return nearest;
                }
            }
            return null;
        }

        public void SetSelectedEntity(EditorObject selected)
        {
            _selectedEntity = selected;
            if (selected != null)
            {
                _gripper.Visible = true;
                _gripper.Transform.Position = _selectedEntity.GetTransform().Position;
            }
            else
            {
                _gripper.Visible = false;
            }
            if (EntitySelected != null)
                EntitySelected(this, selected);
        }

        public EditorObject GetSelectedEntity()
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
