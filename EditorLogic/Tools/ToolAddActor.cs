using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;

namespace EditorLogic
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
            if (_input.KeyPress(Key.Delete) || _input.KeyPress(Key.Escape) || _input.MousePress(MouseButton.Right))
            {
                Controller.SetTool(null);
            }
            else if (_input.MousePress(MouseButton.Left))
            {
                EditorActor editorActor = new EditorActor(Controller.Level, PolygonFactory.CreateRectangle(4, 0.5f));
                Transform2.SetPosition(editorActor, Controller.GetMouseWorld());
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
            _mouseFollow.Models.Add(Game.ModelFactory.CreatePolygon(PolygonFactory.CreateRectangle(4, 0.5f)));
            _mouseFollow.Models[0].SetTexture(Controller.renderer.Textures["default.png"]);
            _mouseFollow.IsPortalable = true;
        }

        public override void Disable()
        {
            base.Disable();
            Controller.Level.Doodads.Remove(_mouseFollow);
        }
    }
}
