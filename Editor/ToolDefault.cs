using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class ToolDefault : Tool
    {
        public ToolDefault(ControllerEditor controller) 
            : base(controller)
        {
        }

        public override void LeftClick()
        {
            _controller.SetSelectedEntity(_controller.GetNearestEntity());
        }

        public override void RightClick()
        {
        }

        public override void Update()
        {
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
