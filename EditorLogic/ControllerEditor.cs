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

namespace EditorLogic
{
    [Affine]
    public class ControllerEditor : Controller
    {
        [AffineMember]
        public Scene Hud, ActiveLevel;
        [AffineMember]
        public EditorScene Level, Clipboard;
        [AffineMember]
        public ControllerCamera CamControl { get; private set; }
        [AffineMember]
        Tool _activeTool;
        public Tool ActiveTool { get { return _activeTool; } }
        [AffineMember]
        public float physicsStepSize { get; set; }
        [AffineMember]
        Tool _toolDefault;
        [AffineMember]
        Tool _nextTool;
        Queue<Action> Actions = new Queue<Action>();
        public Selection selection { get; private set; }
        [AffineMember]
        public StateList StateList { get; private set; }
        [AffineMember]
        public float CanvasAspect { get { return CanvasSize.Width / (float)CanvasSize.Height; } }
        /// <summary>
        /// Lock used to prevent race conditions when adding and reading from the action queue.
        /// </summary>
        object _lockAction = new object();
        [AffineMember]
        bool _isPaused = true;
        [AffineMember]
        int _stepsPending = 0;

        public delegate void EditorObjectHandler(ControllerEditor controller, EditorObject entity);
        public delegate void SceneEventHandler(ControllerEditor controller);
        public event SceneEventHandler ScenePauseEvent;
        public event SceneEventHandler ScenePlayEvent;
        public event SceneEventHandler SceneStopEvent;
        public delegate void SerializationHandler(ControllerEditor controller, string filepath);
        public event SerializationHandler LevelLoaded;
        public event SerializationHandler LevelSaved;
        public event SerializationHandler LevelCreated;
        /// <summary>Called immediately after LevelLoaded and LevelCreated.</summary>
        public event SerializationHandler LevelChanged;
        public delegate void EditorObjectModified(HashSet<EditorObject> modified);
        /// <summary>Called when an EditorObject's public state has been modified.</summary>
        public event EditorObjectModified SceneModified;
        public delegate void ToolEventHandler(ControllerEditor controller, Tool tool);
        public event ToolEventHandler ToolChanged;

        public ControllerEditor(Size canvasSize, InputExt input)
            : base(canvasSize, input)
        {
            physicsStepSize = 1;
        }

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            LevelCreate();
            
            Hud.SetActiveCamera(new Camera2(Hud, new Transform2(new Vector2(CanvasSize.Width / 2, CanvasSize.Height / 2), CanvasSize.Width), CanvasSize.Width / (float)CanvasSize.Height));
            
            Clipboard = new EditorScene();

            InitTools();
            SceneStop();
        }

        public void LevelCreate()
        {
            renderer.RemoveLayer(Hud);
            renderer.RemoveLayer(Level);
            Hud = new Scene();
            Level = new EditorScene();
            renderer.AddLayer(Level);
            renderer.AddLayer(Hud);

            selection = new Selection(Level);
            StateList = new StateList();

            CamControl = new ControllerCamera(this, InputExt, Level);
            Transform2.SetSize(CamControl, 10);
            Hud.SetActiveCamera(CamControl);
            Level.ActiveCamera = CamControl;

            LevelCreated(this, null);
            LevelChanged(this, null);
        }

        public void LevelLoad(string filepath)
        {
            EditorScene load = Serializer.Deserialize(filepath);
            load.ActiveCamera.Controller = this;
            load.ActiveCamera.InputExt = InputExt;
            renderer.AddLayer(load);
            renderer.RemoveLayer(Level);
            Level = load;
            selection = new Selection(Level);
            //Level.Clear();
            LevelLoaded(this, filepath);
            LevelChanged(this, filepath);
        }

        public void LevelSave(string filepath)
        {
            Serializer.Serialize(Level, filepath);
            LevelSaved(this, filepath);
        }

        public override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
        }

        public Vector2 GetMouseWorldPosition()
        {
            if (Level == null)
            {
                return Vector2.Zero;
            }
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

        public override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            lock (_lockAction)
            {
                foreach (Action item in Actions)
                {
                    item();
                }
                Actions.Clear();
            }
            _setTool(_nextTool);

            if (ActiveLevel != null)
            {
                float stepSize = physicsStepSize / 60;
                if (!_isPaused || _stepsPending > 0)
                {
                    if (_stepsPending > 0)
                    {
                        _stepsPending--;
                    }
                    ActiveLevel.Step(stepSize);
                }
                else
                {
                    ActiveLevel.Step(0);
                }
            }
            else
            {
                _activeTool.Update();
                Level.Step(1 / 60);
            }

            HashSet<EditorObject> modified = new HashSet<EditorObject>(Level._children.FindAll(item => item.IsModified));
            foreach (EditorObject reset in modified)
            {
                reset.IsModified = false;
            }
            if (modified.Count > 0)
            {
                SceneModified(modified);
            }
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
            ToolChanged(this, tool);
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
            tempList.AddRange(Level.GetAll().OfType<EditorObject>());
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
                renderer.RemoveLayer(Level);
                renderer.RemoveLayer(Hud);
            }
            _stepsPending = 0;
            _isPaused = false;
            ScenePlayEvent(this);
        }

        public void ScenePause()
        {
            _isPaused = true;
            ScenePauseEvent(this);
        }

        public void SceneStop()
        {
            if (ActiveLevel != null)
            {
                renderer.RemoveLayer(ActiveLevel);
                renderer.AddLayer(Level);
                renderer.AddLayer(Hud);
            }

            ActiveLevel = null;
            _isPaused = true;
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

        public override void OnResize(EventArgs e, Size canvasSize)
        {
            base.OnResize(e, canvasSize);
            Transform2.SetSize((ITransformable2)Hud.ActiveCamera, canvasSize.Height);
        }
    }
}
