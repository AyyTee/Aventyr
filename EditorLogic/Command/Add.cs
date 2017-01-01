using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorLogic.Command
{
    public abstract class Add : ICommand
    {
        public bool IsMarker { get; set; }
        protected readonly ControllerEditor Controller;
        protected readonly EditorObject EditorObject;
        MementoSelection _selection;

        public Add(ControllerEditor controller, EditorObject editorObject)
        {
            IsMarker = true;
            EditorObject = editorObject;
            Controller = controller;
        }

        public virtual void Do()
        {
            _selection = new MementoSelection(Controller.Selection);
        }

        public virtual void Redo()
        {
            EditorObject.SetParent(EditorObject.Scene);
            //_editorObject.Marker.SetParent(_editorObject.Scene.Scene.Root);
            Controller.Selection.Set(EditorObject);
        }

        public virtual void Undo()
        {
            EditorObject.Remove();
            Controller.Selection.SetRange(_selection);
        }

        public abstract ICommand ShallowClone();
    }
}
