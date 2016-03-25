using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using System.IO;
using OpenTK;
using System.Diagnostics;

namespace Editor
{
    public class ToolDefault : Tool
    {
        Entity _translator;
        Model translationModel;
        const float translationScaleOffset = 0.1f;
        DragState _dragState;
        Mode _mode;
        EditorObject dragObject { get { return Controller.selection.First; } }
        bool dragToggled = false;
        Vector2 mousePosPrev;
        List<MementoDrag> _transformPrev = new List<MementoDrag>();
        Transform2 _totalDrag;
        double _rotateIncrementSize = Math.PI / 8;
        public enum DragState
        {
            Vertical,
            Horizontal,
            Both,
            Neither
        }
        public enum Mode
        {
            Position,
            Rotate,
            Scale
        }

        public ToolDefault(ControllerEditor controller)
            : base(controller)
        {
            string filepath = Path.Combine(MainWindow.LocalDirectory, "editor assets", "models", "coordinateArrows.obj");
            ModelLoader loader = new ModelLoader();
            translationModel = loader.LoadObj(filepath);
            translationModel.Transform.Position = new Vector3(0, 0, 5);
        }

        public override void Update()
        {
            base.Update();
            List<EditorObject> selected = Controller.selection.GetAll();
            if (_dragState == DragState.Neither)
            {
                /*if (_input.KeyDown(InputExt.KeyBoth.Control) && _input.KeyPress(Key.P))
                {
                    EditorObject first = Controller.selection.First;
                    foreach (EditorObject e in Controller.selection.GetAll())
                    {
                        if (e != first)
                        {
                            e.SetParent(first);
                        }
                    }
                }*/
                if (_input.KeyDown(InputExt.KeyBoth.Control))
                {
                    if (_input.KeyPress(Key.C))
                    {
                        List<IDeepClone> cloneList = new List<IDeepClone>(Controller.selection.GetAll());
                        if (cloneList.Count > 0)
                        {
                            Controller.Clipboard.Clear();
                            EditorClone.Clone(cloneList, Controller.Clipboard);
                        }
                        
                    }
                    else if (_input.KeyPress(Key.V))
                    {
                        List<EditorObject> cloned = EditorClone.Clone(Controller.Clipboard, Controller.Level);
                        Controller.selection.SetRange(cloned);
                    }
                }
                if (_input.MousePress(MouseButton.Right))
                {
                    Select();
                }
                if (selected != null)
                {
                    if (_input.KeyPress(Key.Delete))
                    {
                        _translator.Visible = false;
                        Controller.RemoveRange(selected);
                    }
                    else if (_input.MousePress(MouseButton.Left))
                    {
                        DragBegin(selected, false, Mode.Position);
                    }
                    else if (_input.KeyPress(Key.G))
                    {
                        DragBegin(Controller.selection.GetAll(), true, Mode.Position);
                    }
                    else if (_input.KeyPress(Key.R))
                    {
                        DragBegin(Controller.selection.GetAll(), true, Mode.Rotate);
                    }
                    else if (_input.KeyPress(Key.S))
                    {
                        DragBegin(Controller.selection.GetAll(), true, Mode.Scale);
                    }
                }
            }
            else
            {
                Debug.Assert(dragObject != null);
                if (_input.MousePress(MouseButton.Right))
                {
                    DragEnd(true);
                }
                else if (_input.MouseRelease(MouseButton.Left) && dragToggled == false)
                {
                    DragEnd(false);
                }
                else if (_input.MousePress(MouseButton.Left) && dragToggled)
                {
                    DragEnd(false);
                }
                else
                {
                    DragUpdate();
                }
            }
            TranslatorUpdate();
        }

        private void Select()
        {
            Vector2 mousePos = Controller.GetMouseWorldPosition();
            EditorObject nearest = Controller.GetNearestObject(mousePos);
            if (nearest != null && (nearest.GetWorldTransform().Position - mousePos).Length > 1)
            {
                nearest = null;
            }
            if (_input.KeyDown(InputExt.KeyBoth.Shift))
            {
                Controller.selection.Toggle(nearest);
            }
            else
            {
                Controller.selection.Set(nearest);
            }
        }

        private void TranslatorUpdate()
        {
            List<EditorObject> selection = Controller.selection.GetAll();
            if (selection.Count > 0)
            {
                float avgX, avgY;
                avgX = selection.Average(item => item.GetWorldTransform().Position.X);
                avgY = selection.Average(item => item.GetWorldTransform().Position.Y);
                Vector2 average = new Vector2(avgX, avgY);
                Transform2.SetPosition(_translator, average);
                _translator.Visible = true;
            }
            else
            {
                _translator.Visible = false;
            }
        }

        private void DragSet(Mode mode, DragState state)
        {
            _dragState = state;
            _mode = mode;
            _totalDrag = new Transform2();
            _transformPrev.Clear();
            foreach (EditorObject e in Controller.selection.GetAll())
            {
                _transformPrev.Add(new MementoDrag(e));
            }
            Active = true;
        }

        public override bool LockCamera()
        {
            return _dragState != DragState.Neither;
        }

        private void DragBegin(List<EditorObject> dragObjects, bool toggleMode, Mode mode)
        {
            Debug.Assert(dragObjects != null);
            if (dragObjects.Count <= 0)
            {
                return;
            }
            Transform2 transform = _translator.GetTransform();
            Vector2 mousePos = Controller.GetMouseWorldPosition();
            dragToggled = toggleMode;
            mousePosPrev = mousePos;
            if (toggleMode)
            {
                DragSet(mode, DragState.Both);
                return;
            }
            Vector2 mouseDiff = (mousePos - transform.Position) / Transform2.GetSize(Controller.Back.ActiveCamera);
            if (mouseDiff.Length < 1.3f * translationScaleOffset)
            {
                float margin = 0.2f * translationScaleOffset;
                if (Math.Abs(mouseDiff.X) < margin && Math.Abs(mouseDiff.Y) < margin)
                {
                    DragSet(mode, DragState.Both);
                }
                else if (Math.Abs(mouseDiff.X) < margin && mouseDiff.Y > 0)
                {
                    DragSet(mode, DragState.Vertical);
                }
                else if (Math.Abs(mouseDiff.Y) < margin && mouseDiff.X > 0)
                {
                    DragSet(mode, DragState.Horizontal);
                }
            }
        }

        private void DragEnd(bool reset)
        {
            if (reset)
            {
                foreach (MementoDrag t in _transformPrev)
                {
                    t.ResetTransform();
                }
            }
            else
            {
                Controller.StateList.Add(new CommandDrag(_transformPrev, _totalDrag), false);
            }
            _dragState = DragState.Neither;
            dragToggled = false;
            Active = false;
        }

        private void DragUpdate()
        {
            Vector2 mousePos = Controller.GetMouseWorldPosition();
            if (_mode == Mode.Position)
            {
                switch (_dragState)
                {
                    case DragState.Both:
                        _totalDrag.Position += mousePos - mousePosPrev;
                        break;

                    case DragState.Horizontal:
                        _totalDrag.Position += new Vector2(mousePos.X - mousePosPrev.X, 0);
                        break;

                    case DragState.Vertical:
                        _totalDrag.Position += new Vector2(0, mousePos.Y - mousePosPrev.Y);
                        break;
                }
                mousePosPrev = mousePos;
            }
            else if (_mode == Mode.Rotate)
            {
                double angle, anglePrev;
                angle = MathExt.AngleVector(mousePos - _translator.GetTransform().Position);
                anglePrev = MathExt.AngleVector(mousePosPrev - _translator.GetTransform().Position);
                
                if (_input.KeyDown(InputExt.KeyBoth.Control))
                {
                    angle = MathExt.Round(angle, _rotateIncrementSize);
                    _totalDrag.Rotation = (float)MathExt.Round(_totalDrag.Rotation + MathExt.AngleDiff(angle, anglePrev), _rotateIncrementSize);
                    mousePosPrev = new Vector2((float)Math.Cos(-angle), (float)Math.Sin(-angle)) + _translator.GetTransform().Position;
                }
                else
                {
                    _totalDrag.Rotation += (float)MathExt.AngleDiff(angle, anglePrev);
                    mousePosPrev = mousePos;
                }
            }
            else if (_mode == Mode.Scale)
            {
                Vector2 v = mousePos - Transform2.GetPosition(_translator);
                Vector2 vPrev = mousePosPrev - Transform2.GetPosition(_translator);
                const float minDist = 0.01f;
                float lengthPrev = Math.Max(vPrev.Length, Transform2.GetSize(Controller.Back.ActiveCamera) * minDist);
                float length = Math.Max(v.Length, Transform2.GetSize(Controller.Back.ActiveCamera) * minDist);
                if (!float.IsPositiveInfinity(length / lengthPrev))
                {
                    _totalDrag.Size = length / lengthPrev;
                }
            }
            foreach (MementoDrag e in _transformPrev)
            {
                Transform2 t = e.Transformable.GetTransform();
                e.Transformable.SetTransform(e.GetTransform().Add(_totalDrag));
            }
        }

        public override void Enable()
        {
            base.Enable();
            _translator = new Entity(Controller.Back);
            _translator.AddModel(translationModel);
            _translator.Visible = false;
            _translator.DrawOverPortals = true;
            _dragState = DragState.Neither;
            Controller.CamControl.CameraMoved += UpdateTranslation;
            UpdateTranslation(Controller.CamControl.Camera);
            _mode = Mode.Position;
        }

        public override void Disable()
        {
            base.Disable();
            if (DragState.Neither != _dragState)
            {
                DragEnd(false);
            }
            Controller.CamControl.CameraMoved -= UpdateTranslation;
            _translator.Remove();
        }

        private void UpdateTranslation(ControllerCamera controller, Camera2 camera)
        {
            UpdateTranslation(camera);
        }

        private void UpdateTranslation(Camera2 camera)
        {
            Transform2 transform = _translator.GetTransform();
            //transform.Scale = new Vector2(camera.Zoom, camera.Zoom) * translationScaleOffset;
            transform.Size = Transform2.GetSize(camera) * translationScaleOffset;
            _translator.SetTransform(transform);
        }

        public override Tool Clone()
        {
            return new ToolDefault(Controller);
        }
    }
}
