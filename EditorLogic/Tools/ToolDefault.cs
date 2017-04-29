using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EditorLogic.Command;
using Game;
using Game.Common;
using Game.Models;
using Game.Rendering;
using Game.Serialization;
using OpenTK;
using OpenTK.Input;

namespace EditorLogic.Tools
{
    public class ToolDefault : Tool
    {
        Doodad _translator;
        Model _translationModel;
        const float TranslationScaleOffset = 0.1f;
        DragState _dragState;
        Mode _mode;
        bool _dragToggled = false;
        Vector2 _mousePosPrev;
        List<MementoDrag> _dragObjects = new List<MementoDrag>();
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
            _translationModel = loader.LoadObj(filepath);
            if (_translationModel != null)
            {
                _translationModel.Transform.Position = new Vector3(0, 0, 5);
            }
        }

        public override void Update()
        {
            base.Update();
            List<EditorObject> selected = Controller.Selection.GetAll();
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
                if (Input.ButtonDown(KeyBoth.Control))
                {
                    if (Input.ButtonPress(Key.C))
                    {
                        List<IDeepClone> cloneList = new List<IDeepClone>(Controller.Selection.GetAll());
                        if (cloneList.Count > 0)
                        {
                            Controller.Clipboard.Clear();
                            EditorClone.Clone(cloneList, Controller.Clipboard);
                        }
                        
                    }
                    else if (Input.ButtonPress(Key.V))
                    {
                        List<EditorObject> cloned = EditorClone.Clone(Controller.Clipboard, Controller.Level);
                        Controller.Selection.SetRange(cloned);
                    }
                }
                if (Input.ButtonPress(MouseButton.Right))
                {
                    Select();
                }
                if (selected != null)
                {
                    if (Input.ButtonPress(Key.Delete))
                    {
                        _translator.Visible = false;
                        Controller.RemoveRange(selected);
                    }
                    else if (Input.ButtonPress(MouseButton.Left))
                    {
                        DragBegin(selected, false, Mode.Position);
                    }
                    else if (!Input.ButtonDown(KeyBoth.Control))
                    {
                        if (Input.ButtonPress(Key.G))
                        {
                            DragBegin(Controller.Selection.GetAll(), true, Mode.Position);
                        }
                        else if (Input.ButtonPress(Key.R))
                        {
                            DragBegin(Controller.Selection.GetAll(), true, Mode.Rotate);
                        }
                        else if (Input.ButtonPress(Key.S))
                        {
                            DragBegin(Controller.Selection.GetAll(), true, Mode.Scale);
                        }
                    }
                }
            }
            else
            {
                if (Input.ButtonPress(MouseButton.Right))
                {
                    DragEnd(true);
                }
                else if (Input.ButtonRelease(MouseButton.Left) && _dragToggled == false)
                {
                    DragEnd(false);
                }
                else if (Input.ButtonPress(MouseButton.Left) && _dragToggled)
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

        void Select()
        {
            Vector2 mousePos = Controller.GetMouseWorld();
            EditorObject nearest = Controller.GetNearestObject(mousePos);
            if (nearest != null && (nearest.GetWorldTransform().Position - mousePos).Length > 1)
            {
                nearest = null;
            }
            if (Input.ButtonDown(KeyBoth.Shift))
            {
                Controller.Selection.Toggle(nearest);
            }
            else
            {
                Controller.Selection.Set(nearest);
            }
        }

        void TranslatorUpdate()
        {
            List<EditorObject> selection = Controller.Selection.GetAll();
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

        void DragSet(IList<EditorObject> selected, Mode mode, DragState state)
        {
            _dragState = state;
            _mode = mode;
            _totalDrag = Transform2.CreateVelocity();
            _dragObjects.Clear();

            foreach (EditorObject e in Tree<EditorObject>.FindRoots(selected))
            {
                _dragObjects.Add(new MementoDrag(e));
            }
            Active = true;
        }

        public override bool LockCamera()
        {
            return _dragState != DragState.Neither;
        }

        void DragBegin(List<EditorObject> selected, bool toggleMode, Mode mode)
        {
            Debug.Assert(selected != null);
            if (selected.Count <= 0)
            {
                return;
            }
            Transform2 transform = _translator.GetTransform();
            Vector2 mousePos = Controller.GetMouseWorld();
            _dragToggled = toggleMode;
            _mousePosPrev = mousePos;
            if (toggleMode)
            {
                DragSet(selected, mode, DragState.Both);
                return;
            }
            Vector2 mouseDiff = (mousePos - transform.Position) / Controller.Level.ActiveCamera.GetWorldTransform().Size;
            if (mouseDiff.Length < 1.3f * TranslationScaleOffset)
            {
                float margin = 0.2f * TranslationScaleOffset;
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

        void DragEnd(bool reset)
        {
            if (reset)
            {
                foreach (MementoDrag d in _dragObjects)
                {
                    d.ResetTransform();
                }
            }
            else
            {
                Controller.StateList.Add(new Drag(_dragObjects, _totalDrag), true);
            }

            _dragState = DragState.Neither;
            _dragToggled = false;
            Active = false;
        }

        void DragUpdate()
        {
            Transform2 dragAmount = Transform2.CreateVelocity();
            Vector2 mousePos;

            mousePos = Controller.GetMouseWorld();

            if (_mode == Mode.Position)
            {
                switch (_dragState)
                {
                    case DragState.Both:
                        dragAmount.Position += mousePos - _mousePosPrev;
                        break;

                    case DragState.Horizontal:
                        dragAmount.Position += new Vector2(mousePos.X - _mousePosPrev.X, 0);
                        break;

                    case DragState.Vertical:
                        dragAmount.Position += new Vector2(0, mousePos.Y - _mousePosPrev.Y);
                        break;
                }
                _mousePosPrev = mousePos;
            }
            else if (_mode == Mode.Rotate)
            {
                double angle, anglePrev;
                angle = MathExt.AngleVector(mousePos - _translator.GetTransform().Position);
                anglePrev = MathExt.AngleVector(_mousePosPrev - _translator.GetTransform().Position);
                
                if (Input.ButtonDown(KeyBoth.Control))
                {
                    angle = MathExt.Round(angle, _rotateIncrementSize);
                    dragAmount.Rotation = (float)MathExt.Round(dragAmount.Rotation + MathExt.AngleDiff(angle, anglePrev), _rotateIncrementSize);
                    _mousePosPrev = new Vector2((float)Math.Cos(-angle), (float)Math.Sin(-angle)) + _translator.GetTransform().Position;
                }
                else
                {
                    dragAmount.Rotation += (float)MathExt.AngleDiff(angle, anglePrev);
                    _mousePosPrev = mousePos;
                }
            }
            else if (_mode == Mode.Scale)
            {
                dragAmount.Size = mousePos.X - _mousePosPrev.X;
                _mousePosPrev = mousePos;
            }
            _totalDrag = _totalDrag.Add(dragAmount);

            foreach (MementoDrag e in _dragObjects)
            {
                Transform2 t = e.Transformable.GetTransform();
                //if (dragObjects.Exists(item => item is IPortal))
                {
                    t = t.Add(dragAmount);
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

        void UpdateTranslation(ControllerCamera camera)
        {
            Transform2 transform = _translator.GetTransform();
            Transform2 camT = camera.GetWorldTransform();
            transform.Size = camT.Size * TranslationScaleOffset;
            _translator.SetTransform(transform);
        }
    }
}
