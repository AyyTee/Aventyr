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
    public class ToolAddActor : Tool
    {
        Doodad _mouseFollow;

        public ToolAddActor(ControllerEditor controller)
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
                /*Entity entity = new Entity(Controller.Level.Scene);
                entity.AddModel(ModelFactory.CreateCube());
                entity.ModelList[0].SetTexture(Renderer.Textures["default.png"]);
                entity.IsPortalable = true;
                EditorEntity editorEntity = new EditorEntity(Controller.Level, entity);
                editorEntity.Name = "Editor Actor";

                Transform2.SetPosition(editorEntity, Controller.GetMouseWorldPosition());
                Controller.selection.Set(editorEntity);

                CommandAddEntity command = new CommandAddEntity(Controller, editorEntity);
                Controller.StateList.Add(command, true);*/
                EditorActor editorActor = new EditorActor(Controller.Level);
                Transform2.SetPosition(editorActor, Controller.GetMouseWorldPosition());
                if (!_input.KeyDown(InputExt.KeyBoth.Shift))
                {
                    Controller.SetTool(null);
                }
            }
        }

        public override void Enable()
        {
            base.Enable();
            _mouseFollow = new Doodad(Controller.Level);
            _mouseFollow.Models.Add(ModelFactory.CreateCube());
            _mouseFollow.Models[0].SetTexture(Renderer.Textures["default.png"]);
            _mouseFollow.IsPortalable = true;
        }

        public override void Disable()
        {
            base.Disable();
            Controller.Level.Doodads.Remove(_mouseFollow);
        }

        public override Tool Clone()
        {
            return new ToolAddActor(Controller);
        }
    }
}
