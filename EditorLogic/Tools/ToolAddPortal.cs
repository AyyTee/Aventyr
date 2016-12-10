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
using EditorLogic.Command;
using Game.Portals;

namespace EditorLogic
{
    public class ToolAddPortal : Tool
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
                    if (coord != null)
                    {
                        //portal.SetParent((EditorObject)coord.Actor);
                        portal.SetTransform(coord.Wall, coord);
                    }
                    else
                    {
                        portal.SetTransform(_mouseFollow.GetTransform());
                    }

                    AddPortal command;
                    if (isSecondPortal)
                    {
                        Debug.Assert(portalPrevious != null);
                        command = new AddPortal(Controller, portal, portalPrevious);
                    }
                    else
                    {
                        command = new AddPortal(Controller, portal);
                    }
                    Controller.StateList.Add(command, true);
                    
                    if (!_input.KeyDown(KeyBoth.Shift))
                    {
                        Controller.SetTool(null);
                    }
                    portalPrevious = portal;
                    isSecondPortal = !isSecondPortal;
                }
            }
        }

        private WallCoord GetEdgeCoord()
        {
            float size = Transform2.GetSize(_mouseFollow);
            IWall[] walls = Controller.Level.GetAll().OfType<IWall>().ToArray();
            return PortalPlacer.GetNearestPortalableEdge(walls, Controller.GetMouseWorld(), snapDistance, size);
        }

        private Transform2 GetPortalTransform()
        {
            var coord = GetEdgeCoord();
            if (coord != null)
            {
                Transform2 transform = PolygonExt.GetTransform(coord.Wall.GetWorldVertices(), coord);
                transform.Size = 1;
                return transform;
            }
            else
            {
                Transform2 transform = new Transform2();
                transform.Position = Controller.GetMouseWorld();
                //transform.Rotation = _mouseFollow.GetTransform().Rotation;
                transform.Rotation = unsnapAngle;
                //Transform2.SetRotation(_mouseFollow, unsnapAngle);
                transform.Size = _mouseFollow.GetTransform().Size;
                transform.MirrorX = _mouseFollow.GetTransform().MirrorX;
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
