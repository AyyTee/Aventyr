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
            UpdatePortalTransform(_mouseFollow);

            if (_input.MouseDown(MouseButton.Right) || _input.KeyPress(Key.Delete) || _input.KeyPress(Key.Escape))
            {
                Controller.SetTool(null);
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
                    
                    if (!_input.KeyDown(InputExt.KeyBoth.Shift))
                    {
                        Controller.SetTool(null);
                    }
                }
            }
        }

        private void UpdatePortalTransform(EditorPortal portal)
        {
            FixtureEdgeCoord coord = FixtureExt.GetNearestPortalableEdge(Controller.Level.World, Controller.GetMouseWorldPosition(), snapDistance, portal.GetTransform().Scale.X);
            if (coord != null)
            {
                portal.SetTransform(coord.GetWorldTransform());
            }
            else
            {
                Transform2D transform = portal.GetTransform();
                transform.Position = Controller.GetMouseWorldPosition();
                transform.Rotation = _mouseFollow.GetTransform().Rotation;
                transform.Scale = _mouseFollow.GetTransform().Scale;
                portal.SetTransform(transform);
            }
        }

        public override void Enable()
        {
            base.Enable();
            _mouseFollow = new EditorPortal(Controller, Controller.Level);
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
