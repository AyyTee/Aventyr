using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public abstract class CommandAdd : ICommand
    {
        protected readonly ControllerEditor _controller;
        protected readonly EditorObject _editorObject;
        MementoSelection _selection;

        public CommandAdd(ControllerEditor controller, EditorObject editorObject)
        {
            _editorObject = editorObject;
            _controller = controller;
        }

        public virtual void Do()
        {
            _selection = new MementoSelection(_controller.selection);
        }

        public virtual void Redo()
        {
            _editorObject.SetParent(_editorObject.Scene.Root);
            _editorObject.Marker.SetParent(_editorObject.Scene.Scene.Root);
            _controller.selection.Set(_editorObject);
        }

        public virtual void Undo()
        {
            _editorObject.Remove();
            _controller.selection.SetRange(_selection);
        }

        public abstract ICommand Clone();
    }
}
