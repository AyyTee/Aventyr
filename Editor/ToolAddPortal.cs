using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;
using System.Drawing;

namespace Editor
{
    class ToolAddPortal : Tool
    {
        EditorPortal _mouseFollow;
        State _stateCurrent;
        double _rotationSnapSize = 2 * Math.PI / 24;
        enum State
        {
            Placing,
            Orienting
        }

        public ToolAddPortal(ControllerEditor controller)
            : base(controller)
        {
        }

        public override void Update()
        {
            base.Update();
            if (_mouseFollow != null)
            {
                Transform2D transform = _mouseFollow.GetTransform();
                Vector2 mousePos = Controller.GetMouseWorldPosition();
                if (_stateCurrent == State.Placing)
                {
                    transform.Position = mousePos;
                }
                else if (_stateCurrent == State.Orienting)
                {
                    transform.Rotation = (float)(-MathExt.AngleVector(transform.Position - mousePos)+Math.PI);
                    if (_input.KeyDown(Key.ControlLeft) || _input.KeyDown(Key.ControlRight))
                    {
                        transform.Rotation = (float)MathExt.Round(transform.Rotation, _rotationSnapSize);
                    }
                }
                _mouseFollow.SetTransform(transform);
            }

            if (_input.KeyPress(Key.Delete) || _input.KeyPress(Key.Escape))
            {
                Controller.SetTool(null);
            }

            if (_input.MouseRelease(MouseButton.Right) && _stateCurrent == State.Orienting)
            {
                //_controller.SetCursor(new Vector2());
                _stateCurrent = State.Placing;
            }
            else if (_input.MouseDown(MouseButton.Right))
            {
                _stateCurrent = State.Orienting;
            }
            else
            {
                
                if (_input.MousePress(MouseButton.Left))
                {
                    EditorPortal editorPortal = Controller.CreateLevelPortal();
                    FloatPortal portal = (FloatPortal)editorPortal.Portal;
                    Transform2D transform = _mouseFollow.GetTransform();
                    editorPortal.SetTransform(transform);
                    Controller.SetSelectedEntity(editorPortal);
                    if (!(_input.KeyDown(Key.ShiftLeft) || _input.KeyDown(Key.ShiftRight)))
                    {
                        Controller.SetTool(null);
                    }
                }
            }
            /*else if (_input.MousePress(MouseButton.Left))
            {
                if (_stateCurrent == State.Placing)
                {
                    _stateCurrent = State.Orienting;
                }
                else if (_stateCurrent == State.Orienting)
                {
                    _stateCurrent = State.Placing;
                    EditorPortal editorPortal = _controller.CreateLevelPortal();
                    FloatPortal portal = (FloatPortal)editorPortal.Portal;
                    Transform2D transform;
                    _mouseFollow.GetTransform(out transform);
                    editorPortal.SetTransform(transform);
                    if (!(_input.KeyDown(Key.ShiftLeft) || _input.KeyDown(Key.ShiftRight)))
                    {
                        _controller.SetTool(null);
                    }
                }
            }*/
        }

        public override void Enable()
        {
            base.Enable();
            _mouseFollow = new EditorPortal(Controller.Level);
            _stateCurrent = State.Placing;
        }

        public override void Disable()
        {
            base.Disable();
            Controller.Remove(_mouseFollow);
        }

        public override Tool Clone()
        {
            throw new NotImplementedException();
        }
    }
}
