using Game;
using Game.Portals;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EditorLogic
{
    public class ControllerEditor : Controller
    {
        public Scene Hud, ActiveLevel;
        public EditorScene Level, Clipboard;
        public ControllerCamera CamControl { get; private set; }
        Tool _activeTool;
        public Tool ActiveTool { get { return _activeTool; } }
        public float physicsStepSize { get; set; }
        Tool _toolDefault;
        Tool _nextTool;
        Queue<Action> Actions = new Queue<Action>();
        public Selection selection { get; private set; }
        public StateList StateList { get; private set; }
        public float CanvasAspect { get { return CanvasSize.Width / (float)CanvasSize.Height; } }
        /// <summary>
        /// Lock used to prevent race conditions when adding and reading from the action queue.
        /// </summary>
        object _lockAction = new object();
        /// <summary>
        /// Use this lock to prevent the OGL thread from being killed while doing something important .
        /// </summary>
        public object ClosingLock { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsStopped { get { return ActiveLevel == null; } }
        int _stepsPending = 0;

        public delegate void UpdateHandler(ControllerEditor controller);
        public event UpdateHandler Update;
        public delegate void SceneEventHandler(ControllerEditor controller);
        public event SceneEventHandler ScenePauseEvent;
        public event SceneEventHandler ScenePlayEvent;
        public event SceneEventHandler SceneStopEvent;
        public delegate void TimeChangedHandler(ControllerEditor controller, double seconds);
        /// <summary>
        /// Called if the Level or ActiveLevel's time changes.  Time value is in milliseconds.
        /// </summary>
        public event TimeChangedHandler TimeChanged;
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
            IsPaused = true;
            physicsStepSize = 1;
            ClosingLock = new object();
        }

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            Clipboard = new EditorScene();

            LevelCreate();
            Hud.SetActiveCamera(new Camera2(Hud, new Transform2(new Vector2(CanvasSize.Width / 2, CanvasSize.Height / 2), CanvasSize.Width), CanvasSize.Width / (float)CanvasSize.Height));

            PortalCommon.UpdateWorldTransform(Hud);
            PortalCommon.UpdateWorldTransform(Level);

            InitTools();
            SceneStop();
        }

        public void LevelCreate()
        {
            //SceneStop();
            SetTool(null);
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
            TimeChanged(this, Level.Time);
        }

        public void LevelLoad(string filepath)
        {
            SceneStop();
            SetTool(null);
            EditorScene load = Serializer.Deserialize(filepath);
            load.ActiveCamera.Controller = this;
            load.ActiveCamera.InputExt = InputExt;
            renderer.AddLayer(load);
            renderer.RemoveLayer(Level);
            Level = load;
            selection = new Selection(Level);

            LevelLoaded(this, filepath);
            LevelChanged(this, filepath);
            TimeChanged(this, Level.Time);
        }

        public void LevelSave(string filepath)
        {
            lock (ClosingLock)
            {
                Serializer.Serialize(Level, filepath);
            }
            LevelSaved(this, filepath);
        }

        /// <summary>
        /// Set the current time in the level.
        /// </summary>
        /// <returns>True if time is set, otherwise false.</returns>
        public bool SetTime(double time)
        {
            if (IsStopped && Level.Time != time)
            {
                Level.SetTime(Math.Max(0, time));
                TimeChanged(this, Level.Time);
            }
            return !IsStopped;
        }

        public double GetTime()
        {
            if (IsStopped)
            {
                return Level.Time;
            }
            return ActiveLevel.Time;
        }

        public override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
        }

        public Vector2 GetMouseWorld()
        {
            if (Level == null)
            {
                return Vector2.Zero;
            }
            return CameraExt.ScreenToWorld(Level.ActiveCamera, InputExt.MousePos);
        }

        public Vector2 GetMouseWorldPortal(IEnumerable<IPortal> portals)
        {
            if (Level == null)
            {
                return Vector2.Zero;
            }
            if (!renderer.PortalRenderEnabled)
            {
                return GetMouseWorld();
            }
            Transform2 transform = CameraExt.GetWorldViewpoint(Level.ActiveCamera);
            Vector2 mousePos = CameraExt.ScreenToWorld(Level.ActiveCamera, InputExt.MousePos);
            Portalable portalable = new Portalable(null, transform, Transform2.CreateVelocity(mousePos - transform.Position));
            Ray.RayCast(portalable, Level.GetPortalList(), new Ray.Settings());
            return portalable.GetTransform().Position;
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

            //Avoid a potential deadlock by making a copy of the action queue and releasing 
            //the lock before executing actions in the copy.
            {
                Queue<Action> copy;
                lock (_lockAction)
                {
                    copy = new Queue<Action>(Actions);
                    Actions.Clear();
                }
                foreach (Action item in copy)
                {
                    item();
                }
            }
            Update(this);

            _setTool(_nextTool);

            if (ActiveLevel != null)
            {
                float stepSize = physicsStepSize / 60;
                if (!IsPaused || _stepsPending > 0)
                {
                    if (_stepsPending > 0)
                    {
                        _stepsPending--;
                    }
                    ActiveLevel.Step(stepSize);
                    TimeChanged(this, ActiveLevel.Time);
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
                PortalCommon.UpdateWorldTransform(Level);
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
            IsPaused = false;
            ScenePlayEvent(this);
        }

        public void ScenePause()
        {
            IsPaused = true;
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
            IsPaused = true;
            SceneStopEvent(this);
        }

        public void SceneStep()
        {
            if (ActiveLevel == null)
            {
                ScenePlay();
            }
            if (!IsPaused)
            {
                ScenePause();
            }
            _stepsPending++;
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
