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
        float snapDistance = 0.2f;
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
                    UpdatePortalTransform(_mouseFollow);
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
                _stateCurrent = State.Placing;
            }
            else if (_input.MouseDown(MouseButton.Right))
            {
                Controller.SetTool(null);
                //_stateCurrent = State.Orienting;
            }
            else
            {
                if (_input.MousePress(MouseButton.Left))
                {
                    FixtureEdgeCoord coord = FixtureExt.GetNearestPortalableEdge(Controller.Level.World, Controller.GetMouseWorldPosition(), snapDistance, _mouseFollow.Portal.Size);
                    EditorPortal editorPortal;
                    if (coord != null)
                    {
                        editorPortal = Controller.CreateLevelPortal(new FixturePortal(Controller.Level, coord));
                    }
                    else
                    {
                        editorPortal = Controller.CreateLevelPortal();
                        UpdatePortalTransform(editorPortal);
                    }
                    
                    //editorPortal.Marker.IsPortalable = true;
                    
                    //Controller.SetSelectedEntity(editorPortal);
                    if (!(_input.KeyDown(Key.ShiftLeft) || _input.KeyDown(Key.ShiftRight)))
                    {
                        Controller.SetTool(null);
                    }
                }
            }
        }

        private void UpdatePortalTransform(EditorPortal portal)
        {
            Transform2D transform = portal.GetTransform();
            FixtureEdgeCoord coord = FixtureExt.GetNearestPortalableEdge(Controller.Level.World, Controller.GetMouseWorldPosition(), snapDistance, portal.GetTransform().Scale.X);
            if (coord != null)
            {
                /*transform.Position = coord.GetTransform().WorldPosition;
                transform.Rotation = coord.GetTransform().WorldRotation;
                transform.Scale = coord.GetTransform().WorldScale;*/
                transform.SetLocal(coord.GetWorldTransform());
            }
            else
            {
                transform.Position = Controller.GetMouseWorldPosition();
                transform.Rotation = _mouseFollow.GetTransform().Rotation;
                transform.Scale = _mouseFollow.GetTransform().Scale;
            }
            portal.SetTransform(transform);
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
