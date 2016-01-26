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
        protected readonly Transform2 _transform;
        protected readonly ControllerEditor _controller;
        protected EditorObject _editorObject;
        MementoSelection _selection;

        public CommandAdd(ControllerEditor controller, Transform2 transform)
        {
            _transform = transform;
            _controller = controller;
        }

        public virtual void Do()
        {
            _selection = new MementoSelection(_controller.selection);
        }

        public virtual void Redo()
        {
            _editorObject.SetParent(_editorObject.EditorScene.Root);
            _editorObject.Marker.SetParent(_editorObject.EditorScene.Scene.Root);
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
