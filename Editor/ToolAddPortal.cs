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

        public ToolAddPortal(ControllerEditor controller)
            : base(controller)
        {
        }

        public override void Update()
        {
            base.Update();
            _mouseFollow.SetTransform(GetPortalTransform());

            if (_input.MouseDown(MouseButton.Right) || _input.KeyPress(Key.Delete) || _input.KeyPress(Key.Escape))
            {
                Controller.SetTool(null);
            }
            else
            {
                if (_input.MousePress(MouseButton.Left))
                {
                    //FixtureEdgeCoord coord = FixtureExt.GetNearestPortalableEdge(Controller.Level.World, Controller.GetMouseWorldPosition(), snapDistance, _mouseFollow.Portal.Size);
                    /*if (coord != null)
                    {
                        editorPortal = Controller.CreateLevelPortal(new FixturePortal(Controller.Level, coord));
                    }
                    else*/
                    {
                        Controller.StateList.Add(new CommandAddPortal(Controller, GetPortalTransform()), true);
                    }
                    
                    if (!_input.KeyDown(InputExt.KeyBoth.Shift))
                    {
                        Controller.SetTool(null);
                    }
                }
            }
        }

        private Transform2D GetPortalTransform()
        {
            /*FixtureEdgeCoord coord = FixtureExt.GetNearestPortalableEdge(Controller.Level.World, Controller.GetMouseWorldPosition(), snapDistance, 1);
            if (coord != null)
            {
                return coord.GetWorldTransform();
            }
            else*/
            {
                Transform2D transform = new Transform2D();
                transform.Position = Controller.GetMouseWorldPosition();
                transform.Rotation = _mouseFollow.GetTransform().Rotation;
                transform.Scale = _mouseFollow.GetTransform().Scale;
                return transform;
            }
        }

        public override void Enable()
        {
            base.Enable();
            _mouseFollow = new EditorPortal(Controller);
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
