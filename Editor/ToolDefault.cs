using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;

namespace Editor
{
    public class ToolDefault : Tool
    {
        public ToolDefault(ControllerEditor controller)
            : base(controller)
        {
        }

        public override void Update()
        {
            EditorObject selected = _controller.GetSelectedEntity();
            if (_input.KeyPress(Key.Delete) && selected != null)
            {
                _controller.Remove(selected);
            }
            if (_input.MousePress(MouseButton.Left))
            {
                _controller.SetSelectedEntity(_controller.GetNearestEntity());
            }
        }

        public override void Enable()
        {
        }

        public override void Disable()
        {
        }

        public override Tool Clone()
        {
            return new ToolDefault(_controller);
        }
    }
}
