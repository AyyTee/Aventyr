using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;

namespace Editor
{
    class ToolAddPortal : Tool
    {
        FloatPortal _mouseFollow;

        public ToolAddPortal(ControllerEditor controller)
            : base(controller)
        {
        }

        public override void Update()
        {
            if (_mouseFollow != null)
            {
                _mouseFollow.Transform.Position = _controller.GetMouseWorldPosition();
            }
            if (_input.KeyPress(Key.Delete) || _input.KeyPress(Key.Escape))
            {
                _controller.SetTool(null);
            }
        }

        public override void Enable()
        {
            _mouseFollow = new FloatPortal(_controller.Level);
            //_mouseFollow.Models.Add(ModelFactory.CreateCube());
        }

        public override void Disable()
        {
            //_controller.Level.RemovePortal(_mouseFollow);
        }

        public override Tool Clone()
        {
            throw new NotImplementedException();
        }
    }
}
