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
        EditorObject dragObject;
        bool dragToggled = false;
        Vector2 dragMouseStart;
        Vector2 dragEntityStart;
        public enum DragState
        {
            Vertical,
            Horizontal,
            Both,
            Neither
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
                        transform.Position = Controller.GetSelectedEntity().GetTransform().Position;
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
                        DragBegin(selected, false);
                    }
                    else if (_input.KeyPress(Key.G))
                    {
                        DragBegin(Controller.GetSelectedEntity(), true);
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

        private void DragBegin(EditorObject dragObject, bool toggleMode)
        {
            Debug.Assert(this.dragObject == null && _dragState == DragState.Neither);
            Transform2D transform = dragObject.GetTransform();
            Vector2 mousePos = Controller.GetMouseWorldPosition();
            Vector2 mouseDiff = (mousePos - transform.Position) / Controller.Level.ActiveCamera.Scale;
            dragToggled = toggleMode;
            dragEntityStart = transform.Position;
            dragMouseStart = mousePos;
            if (toggleMode)
            {
                _dragState = DragState.Both;
                this.dragObject = dragObject;
                return;
            }
            if (mouseDiff.Length < 1.3f * translationScaleOffset)
            {
                float margin = 0.2f * translationScaleOffset;
                if (Math.Abs(mouseDiff.X) < margin && Math.Abs(mouseDiff.Y) < margin)
                {
                    this.dragObject = dragObject;
                    _dragState = DragState.Both;
                }
                else if (Math.Abs(mouseDiff.X) < margin && mouseDiff.Y > 0)
                {
                    this.dragObject = dragObject;
                    _dragState = DragState.Vertical;
                }
                else if (Math.Abs(mouseDiff.Y) < margin && mouseDiff.X > 0)
                {
                    this.dragObject = dragObject;
                    _dragState = DragState.Horizontal;
                }
            }
        }

        private void DragEnd(bool reset)
        {
            if (reset)
            {
                Transform2D transform = dragObject.GetTransform();
                transform.Position = dragEntityStart;
                dragObject.SetTransform(transform);
                //translation.Transform.Position = dragEntityStart;
                transform = translation.GetTransform();
                transform.Position = dragEntityStart;
                translation.SetTransform(transform);
            }
            _dragState = DragState.Neither;
            dragObject = null;
            dragToggled = false;
        }

        private void DragUpdate()
        {
            Transform2D transform = dragObject.GetTransform();
            Vector2 mousePos = Controller.GetMouseWorldPosition();
            Vector2 mouseDiff = mousePos - transform.Position;
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
            translation.SetTransform(transform);
            //translation.Transform.Position = transform.Position;
            dragObject.SetTransform(transform);
            dragMouseStart = mousePos;
        }

        public override void Enable()
        {
            base.Enable();
            translation = new Entity(Controller.LevelHud);
            translation.Models.Add(translationModel);
            translation.Visible = false;
            _dragState = DragState.Neither;
            Controller.CamControl.CameraMoved += UpdateTranslation;
            UpdateTranslation(Controller.CamControl.Camera);
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
