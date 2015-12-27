using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;

namespace Editor
{
    public class ToolPortalLinker : Tool
    {
        Entity line;
        EditorPortal _portalPrevious;

        public ToolPortalLinker(ControllerEditor controller)
            : base(controller)
        {
        }

        public override void Update()
        {
            base.Update();
            if (_input.MousePress(MouseButton.Left))
            {
                EditorPortal portal = (EditorPortal)Controller.GetNearestObject(Controller.GetMouseWorldPosition(), 
                    item => item.GetType() == typeof(EditorPortal));
                if (portal != null && portal != _portalPrevious)
                {
                    if (_portalPrevious == null)
                    {
                        _portalPrevious = portal;
                    }
                    else
                    {
                        Portal.SetLinked(portal.Portal, _portalPrevious.Portal);
                        portal.Portal.IsMirrored = true;
                        _portalPrevious.Portal.IsMirrored = false;
                        if (_input.KeyDown(InputExt.KeyBoth.Control))
                        {
                            Transform2D t = portal.GetTransform();
                            t.Scale = new Vector2(1, -1);
                            portal.SetTransform(t);
                            t = _portalPrevious.GetTransform();
                            t.Scale = new Vector2(1, 1);
                            _portalPrevious.SetTransform(t);
                        }
                        else
                        {
                            Transform2D t = portal.GetTransform();
                            t.Scale = new Vector2(1, 1);
                            portal.SetTransform(t);
                            t = _portalPrevious.GetTransform();
                            t.Scale = new Vector2(1, 1);
                            _portalPrevious.SetTransform(t);
                        }
                        
                        
                        _portalPrevious = null;
                        Controller.SetTool(null);
                    }
                }
            }
            if (_portalPrevious != null)
            {
                line.Models.Clear();
                Model lineModel = ModelFactory.CreateLineStrip(new Vector2[] {
                    Controller.GetMouseWorldPosition(),
                    _portalPrevious.GetWorldTransform().Position
                });
                lineModel.SetColor(new Vector3(0.1f, 0.7f, 0.1f));
                line.Models.Add(lineModel);
            }
        }

        public override void Enable()
        {
            base.Enable();
            _portalPrevious = null;
            line = new Entity(Controller.Level);
        }

        public override void Disable()
        {
            base.Disable();
            Controller.Level.RemoveEntity(line);
        }

        public override Tool Clone()
        {
            throw new NotImplementedException();
        }
    }
}
