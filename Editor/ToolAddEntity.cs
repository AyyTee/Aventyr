using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class ToolAddEntity : Tool
    {
        Entity _mouseFollow;

        public ToolAddEntity(ControllerEditor controller) 
            : base(controller)
        {
        }

        public override void LeftClick()
        {
            Entity entity = new Entity(_controller.Level);
            entity.Transform.Position = _controller.GetMouseWorldPosition();
            entity.Models.Add(ModelFactory.CreateCube());
            entity.Velocity.Rotation = .1f;
            _controller.AddEntity(entity);
            _controller.SetSelectedEntity(entity);

            if (!(_controller.InputExt.KeyDown(OpenTK.Input.Key.ShiftLeft) || _controller.InputExt.KeyDown(OpenTK.Input.Key.ShiftRight)))
            {
                _controller.SetTool(null);
            }
        }

        public override void RightClick()
        {
            _controller.SetTool(null);
        }

        public override void Update()
        {
            if (_mouseFollow != null)
            {
                _mouseFollow.Transform.Position = _controller.GetMouseWorldPosition();
            }
        }

        public override void Enable()
        {
            _mouseFollow = new Entity(_controller.Level);
            _mouseFollow.Models.Add(ModelFactory.CreateCube());
        }

        public override void Disable()
        {
            _controller.Level.RemoveEntity(_mouseFollow);
        }

        public override Tool Clone()
        {
            return new ToolAddEntity(_controller);
        }
    }
}
