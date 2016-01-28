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
                Transform2 transform = _mouseFollow.GetTransform();
                transform.Position = Controller.GetMouseWorldPosition();
                _mouseFollow.SetTransform(transform);
            }
            if (_input.KeyPress(Key.Delete) || _input.KeyPress(Key.Escape) || _input.MousePress(MouseButton.Right))
            {
                Controller.SetTool(null);
            }
            else if (_input.MousePress(MouseButton.Left))
            {
                /*EditorEntity entity = Controller.CreateLevelEntity();
                entity.SetPosition(Controller.GetMouseWorldPosition());
                EntityFactory.CreateEntityBox(entity.Entity, new Vector2());*/
                EditorEntity editorEntity = new EditorEntity(Controller.Level);//_controller.CreateLevelEntity();
                editorEntity.Entity.AddModel(ModelFactory.CreateCube());
                editorEntity.Entity.ModelList[0].SetTexture(Renderer.Textures["default.png"]);
                editorEntity.Entity.IsPortalable = true;
                Transform2.SetPosition(editorEntity, Controller.GetMouseWorldPosition());
                Controller.selection.Set(editorEntity);

                CommandAddEntity command = new CommandAddEntity(Controller, editorEntity);
                Controller.StateList.Add(command, true);

                if (!_input.KeyDown(InputExt.KeyBoth.Shift))
                {
                    Controller.SetTool(null);
                }
            }
        }

        public override void Enable()
        {
            base.Enable();
            _mouseFollow = new Entity(Controller.Back);
            _mouseFollow.AddModel(ModelFactory.CreateCube());
            _mouseFollow.ModelList[0].SetTexture(Renderer.Textures["default.png"]);
            _mouseFollow.IsPortalable = true;
        }

        public override void Disable()
        {
            base.Disable();
            _mouseFollow.Remove();
        }

        public override Tool Clone()
        {
            return new ToolAddEntity(Controller);
        }
    }
}
