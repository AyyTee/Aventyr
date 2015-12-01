using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;

namespace Editor
{
    public class ToolAddEntity : Tool
    {
        Entity _mouseFollow;

        public ToolAddEntity(ControllerEditor controller)
            : base(controller)
        {
        }

        public override void Update()
        {
            if (_mouseFollow != null)
            {
                _mouseFollow.Transform.Position = _controller.GetMouseWorldPosition();
            }
            if (_input.KeyPress(Key.Delete) || _input.KeyPress(Key.Escape) || _input.MousePress(MouseButton.Right))
            {
                _controller.SetTool(null);
            }
            else if (_input.MousePress(MouseButton.Left))
            {
                Entity entity = new Entity(_controller.Level);
                entity.Transform.Position = _controller.GetMouseWorldPosition();
                entity.Models.Add(ModelFactory.CreateCube());
                entity.Velocity.Rotation = .1f;
                _controller.AddLevelEntity(entity);
                _controller.SetSelectedEntity(entity);

                if (!(_input.KeyDown(Key.ShiftLeft) || _input.KeyDown(Key.ShiftRight)))
                {
                    _controller.SetTool(null);
                }
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
