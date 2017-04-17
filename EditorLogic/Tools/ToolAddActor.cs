using Game;
using Game.Common;
using OpenTK.Input;

namespace EditorLogic.Tools
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
                transform.Position = Controller.GetMouseWorld();
                _mouseFollow.SetTransform(transform);
            }
            if (Input.KeyPress(Key.Delete) || Input.KeyPress(Key.Escape) || Input.MousePress(MouseButton.Right))
            {
                Controller.SetTool(null);
            }
            else if (Input.MousePress(MouseButton.Left))
            {
                EditorActor editorActor = new EditorActor(Controller.Level, PolygonFactory.CreateRectangle(4, 0.5f));
                Transform2.SetPosition(editorActor, Controller.GetMouseWorld());
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
            _mouseFollow.Models.Add(Game.Rendering.ModelFactory.CreatePolygon(PolygonFactory.CreateRectangle(4, 0.5f)));
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
