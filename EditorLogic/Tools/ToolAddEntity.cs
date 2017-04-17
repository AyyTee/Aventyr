using EditorLogic.Command;
using Game;
using Game.Common;
using Game.Models;
using OpenTK.Input;

namespace EditorLogic.Tools
{
    public class ToolAddEntity : Tool
    {
        Doodad _mouseFollow;

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
                transform.Position = Controller.GetMouseWorld();
                _mouseFollow.SetTransform(transform);
            }
            if (Input.KeyPress(Key.Delete) || Input.KeyPress(Key.Escape) || Input.MousePress(MouseButton.Right))
            {
                Controller.SetTool(null);
            }
            else if (Input.MousePress(MouseButton.Left))
            {
                EditorEntity editorEntity = new EditorEntity(Controller.Level);
                Model m = Game.Rendering.ModelFactory.CreateCube();
                m.SetTexture(Controller.Window.Textures.@Default);
                editorEntity.AddModel(m);
                editorEntity.Name = "Editor Entity";
                
                Transform2.SetPosition(editorEntity, Controller.GetMouseWorld());
                Controller.Selection.Set(editorEntity);

                AddEntity command = new AddEntity(Controller, editorEntity);
                Controller.StateList.Add(command, true);

                if (!Input.KeyDown(KeyBoth.Shift))
                {
                    Controller.SetTool(null);
                }
            }
        }

        public override void Enable()
        {
            base.Enable();
            _mouseFollow = new Doodad(Controller.Level);
            _mouseFollow.Models.Add(Game.Rendering.ModelFactory.CreateCube());
            _mouseFollow.Models[0].SetTexture(Controller.Window.Textures.@Default);
            _mouseFollow.IsPortalable = true;
        }

        public override void Disable()
        {
            base.Disable();
            Controller.Level.Doodads.Remove(_mouseFollow);
        }
    }
}
