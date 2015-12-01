using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    class ToolAddPortal : Tool
    {
        FloatPortal _mouseFollow;

        public ToolAddPortal(ControllerEditor controller) 
            : base(controller)
        {
        }

        public override void LeftClick()
        {

        }

        public override void RightClick()
        {
            Entity entity = new Entity(_controller.Level);
            entity.Transform.Position = _controller.GetMouseWorldPosition();
            entity.Models.Add(ModelFactory.CreateCube());
            entity.Velocity.Rotation = .1f;
            _controller.AddEntity(entity);
            _controller.SetSelectedEntity(entity);
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
