using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class CommandAddPortal : CommandAdd
    {
        public CommandAddPortal(ControllerEditor controller, Transform2 transform)
            : base(controller, transform)
        {
        }

        public override void Do()
        {
            base.Do();
            EditorPortal editorPortal = new EditorPortal(_controller.Level);
            _editorObject = editorPortal;
            editorPortal.SetTransform(_transform);
            _controller.selection.Set(editorPortal);
        }

        public override ICommand Clone()
        {
            CommandAddPortal clone = new CommandAddPortal(_controller, _transform);
            clone._editorObject = _editorObject;
            return clone;
        }
    }
}
