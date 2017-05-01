using Game;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Rendering;

namespace EditorLogic.Tools
{
    public class ToolAddPlayer : Tool
    {
        Doodad _mouseFollow;

        public ToolAddPlayer(ControllerEditor controller)
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
            if (Input.ButtonPress(Key.Delete) || Input.ButtonPress(Key.Escape) || Input.ButtonPress(MouseButton.Right))
            {
                Controller.SetTool(null);
            }
            else if (Input.ButtonPress(MouseButton.Left))
            {
                EditorPlayer editorActor = new EditorPlayer(Controller.Level);
                editorActor.SetPosition(Controller.GetMouseWorld());
                Controller.SetTool(null);
            }
        }

        public override void Enable()
        {
            base.Enable();
            _mouseFollow = new Doodad(Controller.Level, "Add Player");
            _mouseFollow.Models.Add(ModelFactory.CreatePlayer());
            _mouseFollow.IsPortalable = true;
        }

        public override void Disable()
        {
            base.Disable();
            Controller.Level.Doodads.Remove(_mouseFollow);
        }
    }
}
