using Game;
using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using EditorLogic.Tools;
using Game.Common;
using Game.Rendering;

namespace EditorLogic
{
    public class ControllerEditor : Controller
    {
        public Scene Hud, ActiveLevel;
        public EditorScene Level, Clipboard;
        public ControllerCamera CamControl { get; private set; }
        public Tool ActiveTool { get; set; }

        public float PhysicsStepSize { get; set; }
        Tool _toolDefault;
        Tool _nextTool;
        readonly Queue<Action> _actions = new Queue<Action>();
        public Selection Selection { get; private set; }
        public StateList StateList { get; private set; }
        public float CanvasAspect => CanvasSize.Width / (float)CanvasSize.Height;

        /// <summary>
        /// Lock used to prevent race conditions when adding and reading from the action queue.
        /// </summary>
        readonly object _lockAction = new object();

        /// <summary>
        /// Use this lock to prevent the OGL thread from being killed while doing something important .
        /// </summary>
        public object ClosingLock { get; } = new object();
        public bool IsPaused { get; private set; }
        public bool IsStopped => ActiveLevel == null;
        int _stepsPending;

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

        public ControllerEditor(Size canvasSize, Input input)
            : base(canvasSize, input)
        {
            IsPaused = true;
            PhysicsStepSize = 1;
        }

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            Clipboard = new EditorScene();

            LevelCreate();
            Hud.SetActiveCamera(new Camera2(Hud, new Transform2(new Vector2(CanvasSize.Width / 2f, CanvasSize.Height / 2f), CanvasSize.Width), CanvasSize.Width / (float)CanvasSize.Height));

            PortalCommon.UpdateWorldTransform(Hud);
            PortalCommon.UpdateWorldTransform(Level);

            InitTools();
            SceneStop();
        }

        public void LevelCreate()
        {
            //SceneStop();
            SetTool(null);
            Renderer.RemoveLayer(Hud);
            Renderer.RemoveLayer(Level);
            Hud = new Scene();
            Level = new EditorScene(Renderer);
            Renderer.AddLayer(Level);
            Renderer.AddLayer(Hud);

            Selection = new Selection(Level);
            StateList = new StateList();

            CamControl = new ControllerCamera(this, Input, Level);
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
            load.ActiveCamera.InputExt = Input;
            load.SetRenderer(Renderer);
            Renderer.AddLayer(load);
            Renderer.RemoveLayer(Level);
            Level = load;
            Selection = new Selection(Level);

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
            return IsStopped ? Level.Time : ActiveLevel.Time;
        }

        public Vector2 GetMouseWorld()
        {
            return Level == null ? 
                Vector2.Zero : 
                CameraExt.ScreenToWorld(Level.ActiveCamera, Input.MousePos, Vector2Ext.ToOtk(CanvasSize));
        }

        public Vector2 GetMouseWorldPortal(IEnumerable<IPortal> portals)
        {
            if (Level == null)
            {
                return Vector2.Zero;
            }
            if (!Renderer.PortalRenderEnabled)
            {
                return GetMouseWorld();
            }
            Transform2 transform = CameraExt.GetWorldViewpoint(Level.ActiveCamera);
            Vector2 mousePos = CameraExt.ScreenToWorld(Level.ActiveCamera, Input.MousePos, Vector2Ext.ToOtk(CanvasSize));
            var portalable = new Portalable(null, transform, Transform2.CreateVelocity(mousePos - transform.Position));
            Ray.RayCast(portalable, Level.GetPortalList(), new Ray.Settings());
            return portalable.GetTransform().Position;
        }

        public void Remove(EditorObject editorObject)
        {
            editorObject.Remove();
            Selection.Remove(editorObject);
        }

        public void RemoveRange(List<EditorObject> editorObjects)
        {
            foreach (EditorObject e in editorObjects)
            {
                e.Remove();
                Selection.Remove(e);
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
                    copy = new Queue<Action>(_actions);
                    _actions.Clear();
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
                float stepSize = PhysicsStepSize / 60;
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
                ActiveTool.Update();
                Level.Step(1 / 60);
                PortalCommon.UpdateWorldTransform(Level);
            }

            HashSet<EditorObject> modified = new HashSet<EditorObject>(Level.Children.FindAll(item => item.IsModified));
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
            if (!ActiveTool.Active)
            {
                StateList.Undo();
            }
        }

        public void Redo()
        {
            if (!ActiveTool.Active)
            {
                StateList.Redo();
            }
        }

        void _setTool(Tool tool)
        {
            Debug.Assert(tool != null, "Tool cannot be null.");
            if (ActiveTool == _nextTool)
            {
                return;
            }
            ActiveTool.Disable();
            ActiveTool = tool;
            ActiveTool.Enable();
            ToolChanged(this, tool);
        }

        void InitTools()
        {
            _toolDefault = new ToolDefault(this);
            ActiveTool = _toolDefault;
            _nextTool = ActiveTool;
            ActiveTool.Enable();
        }

        public void SetTool(Tool tool)
        {
            _nextTool = tool ?? _toolDefault;
        }

        public EditorObject GetNearestObject(Vector2 point)
        {
            return GetNearestObject(point, item => true);
        }

        public EditorObject GetNearestObject(Vector2 point, Func<EditorObject, bool> validObject)
        {
            var tempList = new List<EditorObject>();
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
                ActiveLevel = LevelExport.Export(Level, this);
                Renderer.AddLayer(ActiveLevel);
                Renderer.RemoveLayer(Level);
                Renderer.RemoveLayer(Hud);
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
                Renderer.RemoveLayer(ActiveLevel);
                Renderer.AddLayer(Level);
                Renderer.AddLayer(Hud);
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
                _actions.Enqueue(action);
            }
        }

        public override void OnResize(EventArgs e, Size canvasSize)
        {
            base.OnResize(e, canvasSize);
            Transform2.SetSize((ITransformable2)Hud.ActiveCamera, canvasSize.Height);
        }
    }
}
