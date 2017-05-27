using Game;
using Game.Common;
using Game.Rendering;
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
                Transform2 transform = _mouseFollow.GetTransform()
                    .SetPosition(Controller.GetMouseWorld());
                _mouseFollow.SetTransform(transform);
            }
            if (Input.ButtonPress(Key.Delete) || Input.ButtonPress(Key.Escape) || Input.ButtonPress(MouseButton.Right))
            {
                Controller.SetTool(null);
            }
            else if (Input.ButtonPress(MouseButton.Left))
            {
                EditorActor editorActor = new EditorActor(Controller.Level, PolygonFactory.CreateRectangle(4, 0.5f));
                editorActor.SetTransform(editorActor.GetTransform().SetPosition(Controller.GetMouseWorld()));
                if (!Input.ButtonDown(KeyBoth.Shift))
                {
                    Controller.SetTool(null);
                }
            }
        }

        public override void Enable()
        {
            base.Enable();
            _mouseFollow = new Doodad("Add Actor");
            Controller.Level.Doodads.Add(_mouseFollow);
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
