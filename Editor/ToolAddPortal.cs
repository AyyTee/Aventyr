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
        EditorPortal _mouseFollow;
        float snapDistance = 0.2f;
        bool isSecondPortal = false;
        EditorPortal portalPrevious = null;

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
                    EditorPortal portal = new EditorPortal(Controller.Level);
                    FixtureEdgeCoord coord = FixtureExt.GetNearestPortalableEdge(Controller.Level.Scene.World, Controller.GetMouseWorldPosition(), snapDistance, _mouseFollow.Portal.Size);
                    if (coord != null)
                    {
                        //Clumsy way of determining which EditorWall this EditorPortal instance needs to parent itself to.
                        EditorObject wall = portal.Scene.FindByType<EditorWall>().Find(item => item.Wall == coord.Actor);
                        //coord.Actor
                        Debug.Assert(wall != null);
                        portal.SetParent(wall);
                        
                        //editorPortal = Controller.CreateLevelPortal(new FixturePortal(Controller.Level, coord));
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

        private Transform2 GetPortalTransform()
        {
            FixtureEdgeCoord coord = FixtureExt.GetNearestPortalableEdge(Controller.Level.Scene.World, Controller.GetMouseWorldPosition(), snapDistance, 1);
            if (coord != null)
            {
                return coord.GetWorldTransform();
            }
            else
            {
                Transform2 transform = new Transform2();
                transform.Position = Controller.GetMouseWorldPosition();
                transform.Rotation = _mouseFollow.GetTransform().Rotation;
                //transform.Scale = _mouseFollow.GetTransform().Scale;
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
            _mouseFollow = new EditorPortal(Controller.Level);
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
