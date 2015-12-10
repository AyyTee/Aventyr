using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;

namespace Editor
{
    public class ToolPortalLinker : Tool
    {
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
                        Portal.ConnectPortals(portal.Portal, _portalPrevious.Portal);
                        portal.Portal.IsMirrored = true;
                        _portalPrevious = null;
                    }
                }
            }
        }

        public override void Enable()
        {
            base.Enable();
        }

        public override void Disable()
        {
            base.Disable();
        }

        public override Tool Clone()
        {
            throw new NotImplementedException();
        }
    }
}
