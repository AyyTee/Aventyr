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
        readonly EditorPortal _linked;

        public CommandAddPortal(ControllerEditor controller, EditorPortal portal, EditorPortal linked = null)
            : base(controller, portal)
        {
            _linked = linked;
        }

        public override void Do()
        {
            base.Do();
            if (_linked != null)
            {
                EditorPortal portal = (EditorPortal)_editorObject;
                portal.Linked = _linked;
                _linked.Linked = portal;
                ///((EditorPortal)_editorObject).Portal.SetLinked(_linked.Portal);
            }
        }

        public override void Undo()
        {
            base.Undo();
            //((EditorPortal)_editorObject).Portal.SetLinked(null);
        }

        public override void Redo()
        {
            base.Redo();
            if (_linked != null)
            {
                //((EditorPortal)_editorObject).Portal.SetLinked(_linked.Portal);
            }
        }

        public override ICommand Clone()
        {
            CommandAddPortal clone = new CommandAddPortal(_controller, (EditorPortal)_editorObject, _linked);
            return clone;
        }
    }
}
