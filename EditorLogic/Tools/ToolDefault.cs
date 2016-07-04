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
using EditorLogic.Command;

namespace EditorLogic
{
    public class ToolDefault : Tool
    {
        Doodad _translator;
        Model translationModel;
        const float translationScaleOffset = 0.1f;
        DragState _dragState;
        Mode _mode;
        bool dragToggled = false;
        Vector2 mousePosPrev;
        List<MementoDrag> dragObjects = new List<MementoDrag>();
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
            
            string filepath = Path.Combine(Directory.GetCurrentDirectory(), "editor assets", "models", "coordinateArrows.obj");
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
                    else if (!_input.KeyDown(InputExt.KeyBoth.Control))
                    {
                        if (_input.KeyPress(Key.G))
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
            }
            else
            {
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
            Vector2 mousePos = Controller.GetMouseWorld();
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

        private void DragSet(IList<EditorObject> selected, Mode mode, DragState state)
        {
            _dragState = state;
            _mode = mode;
            _totalDrag = Transform2.CreateVelocity();
            dragObjects.Clear();

            foreach (EditorObject e in Tree<EditorObject>.FindRoots(selected))
            {
                dragObjects.Add(new MementoDrag(e));
            }
            Active = true;
        }

        public override bool LockCamera()
        {
            return _dragState != DragState.Neither;
        }

        private void DragBegin(List<EditorObject> selected, bool toggleMode, Mode mode)
        {
            Debug.Assert(selected != null);
            if (selected.Count <= 0)
            {
                return;
            }
            Transform2 transform = _translator.GetTransform();
            Vector2 mousePos = Controller.GetMouseWorld();
            dragToggled = toggleMode;
            mousePosPrev = mousePos;
            if (toggleMode)
            {
                DragSet(selected, mode, DragState.Both);
                return;
            }
            Vector2 mouseDiff = (mousePos - transform.Position) / Controller.Level.ActiveCamera.GetWorldTransform().Size;
            if (mouseDiff.Length < 1.3f * translationScaleOffset)
            {
                float margin = 0.2f * translationScaleOffset;
                if (Math.Abs(mouseDiff.X) < margin && Math.Abs(mouseDiff.Y) < margin)
                {
                    DragSet(selected, mode, DragState.Both);
                }
                else if (Math.Abs(mouseDiff.X) < margin && mouseDiff.Y > 0)
                {
                    DragSet(selected, mode, DragState.Vertical);
                }
                else if (Math.Abs(mouseDiff.Y) < margin && mouseDiff.X > 0)
                {
                    DragSet(selected, mode, DragState.Horizontal);
                }
            }
        }

        private void DragEnd(bool reset)
        {
            if (reset)
            {
                foreach (MementoDrag d in dragObjects)
                {
                    d.ResetTransform();
                }
            }
            else
            {
                Controller.StateList.Add(new Drag(dragObjects, _totalDrag), true);
            }

            _dragState = DragState.Neither;
            dragToggled = false;
            Active = false;
        }

        private void DragUpdate()
        {
            Transform2 _dragAmount = Transform2.CreateVelocity();
            Vector2 mousePos;

            mousePos = Controller.GetMouseWorld();

            if (_mode == Mode.Position)
            {
                switch (_dragState)
                {
                    case DragState.Both:
                        _dragAmount.Position += mousePos - mousePosPrev;
                        break;

                    case DragState.Horizontal:
                        _dragAmount.Position += new Vector2(mousePos.X - mousePosPrev.X, 0);
                        break;

                    case DragState.Vertical:
                        _dragAmount.Position += new Vector2(0, mousePos.Y - mousePosPrev.Y);
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
                    _dragAmount.Rotation = (float)MathExt.Round(_dragAmount.Rotation + MathExt.AngleDiff(angle, anglePrev), _rotateIncrementSize);
                    mousePosPrev = new Vector2((float)Math.Cos(-angle), (float)Math.Sin(-angle)) + _translator.GetTransform().Position;
                }
                else
                {
                    _dragAmount.Rotation += (float)MathExt.AngleDiff(angle, anglePrev);
                    mousePosPrev = mousePos;
                }
            }
            else if (_mode == Mode.Scale)
            {
                _dragAmount.Size = mousePos.X - mousePosPrev.X;
                mousePosPrev = mousePos;
            }
            _totalDrag = _totalDrag.Add(_dragAmount);

            foreach (MementoDrag e in dragObjects)
            {
                Transform2 t = e.Transformable.GetTransform();
                //if (dragObjects.Exists(item => item is IPortal))
                {
                    t = t.Add(_dragAmount);
                }
                /*else
                {
                    Portalable temp = new Portalable(t, _dragAmount);
                    Ray.RayCast(temp, Controller.Level.GetPortalList(), new Ray.Settings());
                    t = temp.GetTransform();
                }*/
                e.Transformable.SetTransform(t);
            }
        }

        public override void Enable()
        {
            base.Enable();
            _translator = new Doodad(Controller.Level);
            //_translator.Models.Add(translationModel);
            //_translator.Visible = true;
            //_translator.DrawOverPortals = true;
            _dragState = DragState.Neither;
            Controller.CamControl.CameraMoved += UpdateTranslation;
            UpdateTranslation(Controller.CamControl);
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
            Controller.Level.Doodads.Remove(_translator);
        }

        private void UpdateTranslation(ControllerCamera camera)
        {
            Transform2 transform = _translator.GetTransform();
            Transform2 camT = camera.GetWorldTransform();
            transform.Size = camT.Size * translationScaleOffset;
            _translator.SetTransform(transform);
        }
    }
}
