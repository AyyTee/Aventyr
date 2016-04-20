using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;
using System.Drawing;
using System.Diagnostics;

namespace Editor
{
    class ToolAddPortal : Tool
    {
        Doodad _mouseFollow;
        float snapDistance = 0.2f;
        bool isSecondPortal = false;
        EditorPortal portalPrevious = null;
        float unsnapAngle = 0;

        public ToolAddPortal(ControllerEditor controller)
            : base(controller)
        {
        }

        public override void Update()
        {
            base.Update();
            _mouseFollow.SetTransform(GetPortalTransform());
            Transform2.SetRotation(_mouseFollow, unsnapAngle);

            if (_input.MouseDown(MouseButton.Right) || _input.KeyPress(Key.Delete) || _input.KeyPress(Key.Escape))
            {
                Controller.SetTool(null);
            }
            else
            {
                if (_input.MousePress(MouseButton.Left))
                {
                    EditorPortal portal = new EditorPortal(Controller.Level);
                    FixtureEdgeCoord coord = GetEdgeCoord();
                    if (coord != null)
                    {
                        portal.SetParent((EditorWall)coord.Actor);
                        portal.SetTransform(coord.GetTransform());
                    }
                    else
                    {
                        portal.SetTransform(_mouseFollow.GetTransform());
                    }

                    CommandAddPortal command;
                    if (isSecondPortal)
                    {
                        Debug.Assert(portalPrevious != null);
                        command = new CommandAddPortal(Controller, portal, portalPrevious);
                    }
                    else
                    {
                        command = new CommandAddPortal(Controller, portal);
                    }
                    Controller.StateList.Add(command, true);
                    
                    if (!_input.KeyDown(InputExt.KeyBoth.Shift))
                    {
                        Controller.SetTool(null);
                    }
                    portalPrevious = portal;
                    isSecondPortal = !isSecondPortal;
                }
            }
        }

        private FixtureEdgeCoord GetEdgeCoord()
        {
            float size = Transform2.GetSize(_mouseFollow);
            return FixtureExt.GetNearestPortalableEdge(Controller.Level.World, Controller.GetMouseWorldPosition(), snapDistance, size);
        }

        private Transform2 GetPortalTransform()
        {
            FixtureEdgeCoord coord = GetEdgeCoord();
            if (coord != null)
            {
                return coord.GetWorldTransform();
            }
            else
            {
                Transform2 transform = new Transform2();
                transform.Position = Controller.GetMouseWorldPosition();
                transform.Rotation = _mouseFollow.GetTransform().Rotation;
                transform.Size = _mouseFollow.GetTransform().Size;
                transform.IsMirrored = _mouseFollow.GetTransform().IsMirrored;
                return transform;
            }
        }

        public override void Enable()
        {
            base.Enable();
            isSecondPortal = false;
            portalPrevious = null;
            unsnapAngle = 0;
            _mouseFollow = new Doodad(Controller.Level);
            _mouseFollow.Models.AddRange(EditorModelFactory.CreatePortal());
        }

        public override void Disable()
        {
            base.Disable();
            Controller.Level.Doodads.Remove(_mouseFollow);
        }

        public override Tool Clone()
        {
            throw new NotImplementedException();
        }
    }
}
