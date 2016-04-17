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
        public Scene Hud, ActiveLevel;
        public EditorScene Level, Clipboard;
        public ControllerCamera CamControl { get; private set; }
        public delegate void EditorObjectHandler(ControllerEditor controller, EditorObject entity);
        public event EditorObjectHandler EntityAdded;
        public delegate void SceneEventHandler(ControllerEditor controller);
        public event SceneEventHandler ScenePauseEvent;
        public event SceneEventHandler ScenePlayEvent;
        public event SceneEventHandler SceneStopEvent;
        public delegate void LevelLoadedHandler(ControllerEditor controller, string filepath);
        public event LevelLoadedHandler LevelLoaded;
        /// <summary>Called when an EditorObject's public state has been modified.</summary>
        public event SceneEventHandler SceneModified;
        public delegate void ToolEventHandler(ControllerEditor controller, Tool tool);
        public event ToolEventHandler ToolChanged;
        bool _editorObjectModified;
        Tool _activeTool;
        public Tool ActiveTool { get { return _activeTool; } }
        Tool _toolDefault;
        Tool _nextTool;
        Queue<Action> Actions = new Queue<Action>();
        public Selection selection { get; private set; }
        public StateList StateList { get; private set; }
        /// <summary>
        /// Prevent race conditions when adding and reading from the action queue.
        /// </summary>
        object _lockAction = new object();
        bool _isPaused = true;
        int _stepsPending = 0;

        public ControllerEditor(Size canvasSize, InputExt input)
            : base(canvasSize, input)
        {
        }

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            LevelNew();
            
            Hud.SetActiveCamera(new Camera2(Hud, new Transform2(new Vector2(CanvasSize.Width / 2, CanvasSize.Height / 2), CanvasSize.Width), CanvasSize.Width / (float)CanvasSize.Height));
            
            Clipboard = new EditorScene();
            /*debugText = new Entity(Hud);
            debugText.SetTransform(new Transform2D(new Vector2(0, CanvasSize.Height - 40)));
            */
            InitTools();
            SceneStop();
        }

        public void LevelNew()
        {
            Hud = new Scene();
            Level = new EditorScene();
            renderer.AddLayer(Level);
            renderer.AddLayer(Hud);

            selection = new Selection(Level);
            StateList = new StateList();

            //Level.ActiveCamera = new Camera2(null, new Transform2(new Vector2(), 10), CanvasSize.Width / (float)CanvasSize.Height);

            CamControl = new ControllerCamera(this, InputExt);
            Transform2.SetSize(CamControl, 10);
            Hud.SetActiveCamera(CamControl);
            Level.ActiveCamera = CamControl;
        }

        public void LevelLoad(string filepath)
        {
            //EditorScene load = Serializer.Deserialize(Back, filepath);
            Level.Clear();

            /*foreach (EditorObject e in load.FindAll())
            {
                //e.SetMarker();
                e.SetScene(Level);
            }*/

            if (LevelLoaded != null)
            {
                LevelLoaded(this, filepath);
            }
        }

        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            /*debugText.Models.Clear();
            debugText.Models.Add(FontRenderer.GetModel((Time.ElapsedMilliseconds / RenderCount).ToString()));*/
        }

        public Vector2 GetMouseWorldPosition()
        {
            return CameraExt.ScreenToWorld(Level.ActiveCamera, InputExt.MousePos);
        }

        public void Remove(EditorObject editorObject)
        {
            editorObject.Remove();
            selection.Remove(editorObject);
        }

        public void RemoveRange(List<EditorObject> editorObjects)
        {
            foreach (EditorObject e in editorObjects)
            {
                e.Remove();
                selection.Remove(e);
            }
        }

        public void SetEditorObjectModified()
        {
            _editorObjectModified = true;
        }

        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            lock (_lockAction)
            {
                foreach (Action item in Actions)
                {
                    item();
                }
            }
            Actions.Clear();
            CamControl.Update();
            _setTool(_nextTool);
            _activeTool.Update();
            if (_editorObjectModified && SceneModified != null)
            {
                //SceneModified(this, Level);
                _editorObjectModified = false;
            }
            if ((!_isPaused || _stepsPending > 0) && ActiveLevel != null)
            {
                if (_stepsPending > 0)
                {
                    _stepsPending--;
                }
                ActiveLevel.Step();
            }
            //Back.Step();
        }

        public void Undo()
        {
            if (!_activeTool.Active)
            {
                StateList.Undo();
            }
        }

        public void Redo()
        {
            if (!_activeTool.Active)
            {
                StateList.Redo();
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
        }

        public EditorObject GetNearestObject(Vector2 point, Func<EditorObject, bool> validObject)
        {
            List<EditorObject> tempList = new List<EditorObject>();
            tempList.AddRange(Level.FindByType<EditorObject>());
            var sorted = tempList.OrderBy(item => (point - item.GetWorldTransform().Position).Length).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                if (validObject.Invoke(sorted[i]))
                {
                    return sorted[i];
                }
            }
            return null;
        }

        public void ScenePlay()
        {
            if (ActiveLevel == null)
            {
                ActiveLevel = LevelExport.Export(Level);
                renderer.AddLayer(ActiveLevel);
                //renderer.RemoveLayer(Back);
                renderer.RemoveLayer(Hud);
            }
            _stepsPending = 0;
            _isPaused = false;
            if (ScenePlayEvent != null)
                ScenePlayEvent(this);
        }

        public void ScenePause()
        {
            _isPaused = true;
            if (ScenePauseEvent != null)
                ScenePauseEvent(this);
        }

        public void SceneStop()
        {
            if (ActiveLevel != null)
            {
                renderer.RemoveLayer(ActiveLevel);
                renderer.AddLayer(Hud);
            }

            ActiveLevel = null;
            _isPaused = true;
            if (SceneStopEvent != null)
                SceneStopEvent(this);
        }

        public void SceneStep()
        {
            if (ActiveLevel != null)
            {
                if (!_isPaused)
                {
                    ScenePause();
                }
                _stepsPending++;
            }   
        }

        public void AddAction(Action action)
        {
            lock (_lockAction)
            {
                Actions.Enqueue(action);
            }
        }

        public override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        public override void OnResize(EventArgs e, System.Drawing.Size canvasSize)
        {
            base.OnResize(e, canvasSize);
            Hud.ActiveCamera.Aspect = CanvasSize.Width / (float)CanvasSize.Height;
            Level.ActiveCamera.Aspect = CanvasSize.Width / (float)CanvasSize.Height;
            Transform2.SetSize((Camera2)Hud.ActiveCamera, canvasSize.Height);
        }
    }
}
