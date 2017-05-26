using System.Diagnostics;
using System.Linq;
using EditorLogic.Command;
using Game;
using Game.Common;
using Game.Portals;
using OpenTK.Input;
using Game.Rendering;

namespace EditorLogic.Tools
{
    public class ToolAddPortal : Tool
    {
        Doodad _mouseFollow;
        float _snapDistance = 0.2f;
        bool _isSecondPortal;
        EditorPortal _portalPrevious;
        float _unsnapAngle;

        public ToolAddPortal(ControllerEditor controller)
            : base(controller)
        {
        }
        
        public override void Update()
        {
            base.Update();
            _mouseFollow.SetTransform(GetPortalTransform());

            if (Input.ButtonDown(MouseButton.Right) || Input.ButtonPress(Key.Delete) || Input.ButtonPress(Key.Escape))
            {
                Controller.SetTool(null);
            }
            else
            {
                if (Input.ButtonPress(MouseButton.Left))
                {
                    var portal = new EditorPortal(Controller.Level);
                    WallCoord coord = GetEdgeCoord();
                    if (coord != null)
                    {
                        portal.SetTransform(coord.Wall, coord);
                    }
                    else
                    {
                        portal.SetTransform(_mouseFollow.GetTransform());
                    }

                    AddPortal command;
                    if (_isSecondPortal)
                    {
                        Debug.Assert(_portalPrevious != null);
                        command = new AddPortal(Controller, portal, _portalPrevious);
                    }
                    else
                    {
                        command = new AddPortal(Controller, portal);
                    }
                    Controller.StateList.Add(command);
                    
                    if (!Input.ButtonDown(KeyBoth.Shift))
                    {
                        Controller.SetTool(null);
                    }
                    _portalPrevious = portal;
                    _isSecondPortal = !_isSecondPortal;
                }
            }
        }

        WallCoord GetEdgeCoord()
        {
            IWall[] walls = Controller.Level.GetAll().OfType<IWall>().ToArray();
            return PortalPlacer.GetNearestPortalableEdge(
                walls, 
                Controller.GetMouseWorld(), 
                _snapDistance, 
                _mouseFollow.GetTransform().Size);
        }

        Transform2 GetPortalTransform()
        {
            WallCoord coord = GetEdgeCoord();
            if (coord != null)
            {
                return PolygonEx.GetTransform(coord.Wall.GetWorldVertices(), coord).SetSize(1);
            }
            else
            {
                return new Transform2(
                    Controller.GetMouseWorld(), 
                    _mouseFollow.GetTransform().Size, 
                    _unsnapAngle, 
                    _mouseFollow.GetTransform().MirrorX);
            }
        }

        public override void Enable()
        {
            base.Enable();
            _isSecondPortal = false;
            _portalPrevious = null;
            _unsnapAngle = 0;
            _mouseFollow = new Doodad("Add Portal");
            Controller.Level.Doodads.Add(_mouseFollow);
            _mouseFollow.Models.Add(ModelFactory.CreatePortal());
        }

        public override void Disable()
        {
            base.Disable();
            Controller.Level.Doodads.Remove(_mouseFollow);
        }
    }
}
