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
        Entity translation;
        Model translationModel;
        const float translationScaleOffset = 0.1f;
        DragState _dragState;
        Mode _mode;
        EditorObject dragObject;
        bool dragToggled = false;
        Vector2 mousePosPrev;
        Vector2 dragMouseStart;
        Transform2D dragEntityStartTransform;
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
            EditorObject selected = Controller.GetSelectedEntity();
            if (_dragState == DragState.Neither)
            {
                if (_input.MousePress(MouseButton.Right))
                {
                    Vector2 mousePos = Controller.GetMouseWorldPosition();
                    EditorObject nearest = Controller.GetNearestObject(mousePos);
                    if (nearest != null && (nearest.GetWorldTransform().Position - mousePos).Length > 1)
                    {
                        nearest = null;
                    }
                    Controller.SetSelectedEntity(nearest);
                    if (Controller.GetSelectedEntity() != null)
                    {
                        Transform2D transform = translation.GetTransform();
                        transform.Position = Controller.GetSelectedEntity().GetWorldTransform().Position;
                        translation.SetTransform(transform);
                        translation.Visible = true;
                    }
                    else
                    {
                        translation.Visible = false;
                    }
                }
                if (selected != null)
                {
                    if (_input.KeyPress(Key.Delete))
                    {
                        translation.Visible = false;
                        Controller.Remove(selected);
                    }
                    else if (_input.MousePress(MouseButton.Left))
                    {
                        DragBegin(selected, false, Mode.Position);
                    }
                    else if (_input.KeyPress(Key.G))
                    {
                        DragBegin(Controller.GetSelectedEntity(), true, Mode.Position);
                    }
                    else if (_input.KeyPress(Key.R))
                    {
                        DragBegin(Controller.GetSelectedEntity(), true, Mode.Rotate);
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
        }

        private void SetDragObject(EditorObject dragObject, DragState dragState, Mode mode)
        {
            _dragState = dragState;
            _mode = mode;
            this.dragObject = dragObject;
        }

        private void DragBegin(EditorObject dragObject, bool toggleMode, Mode mode)
        {
            Debug.Assert(this.dragObject == null && _dragState == DragState.Neither);
            Transform2D transform = dragObject.GetWorldTransform();
            Vector2 mousePos = Controller.GetMouseWorldPosition();
            dragToggled = toggleMode;
            dragEntityStartTransform = transform;
            dragMouseStart = mousePos;
            mousePosPrev = mousePos;
            if (toggleMode)
            {
                SetDragObject(dragObject, DragState.Both, mode);
                return;
            }
            Vector2 mouseDiff = (mousePos - transform.Position) / Controller.Level.ActiveCamera.Scale;
            if (mouseDiff.Length < 1.3f * translationScaleOffset)
            {
                float margin = 0.2f * translationScaleOffset;
                if (Math.Abs(mouseDiff.X) < margin && Math.Abs(mouseDiff.Y) < margin)
                {
                    SetDragObject(dragObject, DragState.Both, mode);
                }
                else if (Math.Abs(mouseDiff.X) < margin && mouseDiff.Y > 0)
                {
                    SetDragObject(dragObject, DragState.Vertical, mode);
                }
                else if (Math.Abs(mouseDiff.Y) < margin && mouseDiff.X > 0)
                {
                    SetDragObject(dragObject, DragState.Horizontal, mode);
                }
            }
        }

        private void DragEnd(bool reset)
        {
            if (reset)
            {
                dragObject.SetTransform(dragEntityStartTransform);
                Transform2D transform = translation.GetTransform();
                transform.Position = dragEntityStartTransform.Position;
                translation.SetTransform(transform);
            }
            _dragState = DragState.Neither;
            dragObject = null;
            dragToggled = false;
        }

        private void DragUpdate()
        {
            Transform2D transform = dragObject.GetWorldTransform();
            Vector2 mousePos = Controller.GetMouseWorldPosition();
            Vector2 mouseDiff = mousePos - transform.Position;
            if (_mode == Mode.Position)
            {
                switch (_dragState)
                {
                    case DragState.Both:
                        transform.Position += mousePos - dragMouseStart;
                        break;

                    case DragState.Horizontal:
                        transform.Position += new Vector2(mousePos.X - dragMouseStart.X, 0);
                        break;

                    case DragState.Vertical:
                        transform.Position += new Vector2(0, mousePos.Y - dragMouseStart.Y);
                        break;
                }
                mousePosPrev = mousePos;
            }
            else if (_mode == Mode.Rotate)
            {
                double angle, anglePrev;
                angle = MathExt.AngleVector(mousePos - transform.Position);
                anglePrev = MathExt.AngleVector(mousePosPrev - transform.Position);
                
                if (_input.KeyDown(InputExt.KeyBoth.Control))
                {
                    angle = MathExt.Round(angle, _rotateIncrementSize);
                    transform.Rotation = (float)MathExt.Round(transform.Rotation + MathExt.AngleDiff(angle, anglePrev), _rotateIncrementSize);
                    mousePosPrev = new Vector2((float)Math.Cos(-angle), (float)Math.Sin(-angle)) + transform.Position;
                }
                else
                {
                    transform.Rotation += (float)MathExt.AngleDiff(angle, anglePrev);
                    mousePosPrev = mousePos;
                }
            }
            dragObject.SetTransform(transform);
            transform = dragObject.GetWorldTransform();
            translation.SetPosition(transform.Position);
            dragMouseStart = mousePos;
        }

        public override void Enable()
        {
            base.Enable();
            translation = new Entity(Controller.LevelHud);
            translation.AddModel(translationModel);
            translation.Visible = false;
            _dragState = DragState.Neither;
            dragObject = null;
            Controller.CamControl.CameraMoved += UpdateTranslation;
            UpdateTranslation(Controller.CamControl.Camera);
            _mode = Mode.Position;
        }

        public override void Disable()
        {
            base.Disable();
            DragEnd(false);
            Controller.CamControl.CameraMoved -= UpdateTranslation;
            Controller.LevelHud.RemoveEntity(translation);
        }

        private void UpdateTranslation(ControllerCamera controller, Camera2D camera)
        {
            UpdateTranslation(camera);
        }

        private void UpdateTranslation(Camera2D camera)
        {
            Transform2D transform = translation.GetTransform();
            transform.Scale = new Vector2(camera.Scale, camera.Scale) * translationScaleOffset;
            translation.SetTransform(transform);
        }

        public override Tool Clone()
        {
            return new ToolDefault(Controller);
        }
    }
}
