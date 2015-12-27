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
            base.Update();
            if (_mouseFollow != null)
            {
                Transform2D transform = _mouseFollow.GetTransform();
                transform.Position = Controller.GetMouseWorldPosition();
                _mouseFollow.SetTransform(transform);
            }
            if (_input.KeyPress(Key.Delete) || _input.KeyPress(Key.Escape) || _input.MousePress(MouseButton.Right))
            {
                Controller.SetTool(null);
            }
            else if (_input.MousePress(MouseButton.Left))
            {
                EditorEntity entity = Controller.CreateLevelEntity();
                EntityFactory.CreateEntityBox(entity.Entity, Controller.GetMouseWorldPosition());
                entity.Entity.Models[0].SetTexture(Renderer.Textures["default.png"]);
                entity.Entity.IsPortalable = true;
                
                Controller.SetSelectedEntity(entity);

                if (!_input.KeyDown(InputExt.KeyBoth.Shift))
                {
                    Controller.SetTool(null);
                }
            }
        }

        public override void Enable()
        {
            base.Enable();
            _mouseFollow = new Entity(Controller.Level);
            _mouseFollow.Models.Add(ModelFactory.CreateCube());
            _mouseFollow.Models[0].SetTexture(Renderer.Textures["default.png"]);
            _mouseFollow.IsPortalable = true;
        }

        public override void Disable()
        {
            base.Disable();
            Controller.Level.RemoveEntity(_mouseFollow);
        }

        public override Tool Clone()
        {
            return new ToolAddEntity(Controller);
        }
    }
}
