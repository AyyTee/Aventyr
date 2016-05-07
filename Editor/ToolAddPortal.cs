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

            if (_input.MouseDown(MouseButton.Right) || _input.KeyPress(Key.Delete) || _input.KeyPress(Key.Escape))
            {
                Controller.SetTool(null);
            }
            else
            {
                if (_input.MousePress(MouseButton.Left))
                {
                    EditorPortal portal = new EditorPortal(Controller.Level);
                    var coord = GetEdgeCoord();
                    if (coord.Item1 != null)
                    {
                        //portal.SetParent((EditorObject)coord.Actor);
                        portal.SetTransform(coord.Item1, coord.Item2);
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

        private Tuple<IWall, PolygonCoord> GetEdgeCoord()
        {
            float size = Transform2.GetSize(_mouseFollow);
            IWall[] walls = Controller.Level.FindAll().OfType<IWall>().ToArray();
            return PortalPlacer.GetNearestPortalableEdge(walls, Controller.GetMouseWorldPosition(), snapDistance, size);
        }

        private Transform2 GetPortalTransform()
        {
            var coord = GetEdgeCoord();
            if (coord.Item1 != null)
            {
                Transform2 transform = PolygonExt.GetTransform(coord.Item1.GetWorldVertices(), coord.Item2);
                transform.Size = 1;
                return transform;
            }
            else
            {
                Transform2 transform = new Transform2();
                transform.Position = Controller.GetMouseWorldPosition();
                //transform.Rotation = _mouseFollow.GetTransform().Rotation;
                transform.Rotation = unsnapAngle;
                //Transform2.SetRotation(_mouseFollow, unsnapAngle);
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
            _mouseFollow.Models.Add(ModelFactory.CreatePortal());
        }

        public override void Disable()
        {
            base.Disable();
            Controller.Level.Doodads.Remove(_mouseFollow);
        }
    }
}
