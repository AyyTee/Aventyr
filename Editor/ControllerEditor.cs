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
        public ControllerCamera CamControl { get; private set; }
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
        Tool _activeTool;
        Tool _toolDefault;
        Tool _nextTool;
        List<EditorEntity> Entities = new List<EditorEntity>();
        List<EditorPortal> Portals = new List<EditorPortal>();
        Queue<Action> Actions = new Queue<Action>();

        public ControllerEditor(Size canvasSize, InputExt input)
            : base(canvasSize, input)
        {
        }

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Level = new Scene();
            Level.ActiveCamera = Camera.CameraOrtho(new Vector3(0, 0, 10f), 10, CanvasSize.Width / (float)CanvasSize.Height);
            renderer.AddScene(Level);
            Hud = new Scene();
            Hud.ActiveCamera = Camera.CameraOrtho(new Vector3(CanvasSize.Width / 2, CanvasSize.Height / 2, 0), CanvasSize.Height, CanvasSize.Width / (float)CanvasSize.Height);
            renderer.AddScene(Hud);

            Model background = ModelFactory.CreatePlane();
            background.Texture = Renderer.Textures["grid.png"];
            background.Transform.Position = new Vector3(0, 0, -5f);
            float size = 5;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUv.Scale = new Vector2(size, size);
            Entity back = new Entity(Level, new Vector2(0f, 0f));
            back.Models.Add(background);
            back.IsPortalable = true;

            

            CamControl = new ControllerCamera(Level.ActiveCamera, InputExt);

            
            
            debugText = new Entity(Hud);
            debugText.Transform.Position = new Vector2(0, CanvasSize.Height - 40);

            InitTools();
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
            CamControl.Update();
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

        private void InitTools()
        {
            _toolDefault = new ToolDefault(this);
            _activeTool = _toolDefault;
            _nextTool = _activeTool;
            _activeTool.Enable();
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

        public EditorObject GetNearestObject(Vector2 point)
        {
            return GetNearestObject(point, item => true);
            /*Vector2 mouseWorldPos = Level.ActiveCamera.ScreenToWorld(InputExt.MousePos);
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
            return null;*/
        }

        public EditorObject GetNearestObject(Vector2 point, Func<EditorObject, bool> validObject)
        {
            List<EditorObject> tempList = new List<EditorObject>();
            tempList.AddRange(Entities);
            tempList.AddRange(Portals);
            var sorted = tempList.OrderBy(item => (point - item.GetTransform().Position).Length).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                if (validObject.Invoke(sorted[i]))
                {
                    return sorted[i];
                }
            }
            return null;
        }

        public void SetSelectedEntity(EditorObject selected)
        {
            _selectedEntity = selected;
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

        public void AddAction(Action action)
        {
            Actions.Enqueue(action);
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
