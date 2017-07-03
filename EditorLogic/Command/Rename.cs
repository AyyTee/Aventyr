using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorLogic.Command
{
    public class Rename : ICommand
    {
        public bool IsMarker { get; set; }
        EditorObject _editorObject;
        string _namePrevious;
        string _name;

        Rename()
        {

        }

        public Rename(EditorObject editorObject, string newName)
        {
            DebugEx.Assert(editorObject != null);
            IsMarker = true;
            _namePrevious = editorObject.Name;
            _name = newName;
            _editorObject = editorObject;
        }

        public ICommand ShallowClone()
        {
            Rename clone = new Rename();
            clone.IsMarker = IsMarker;
            clone._editorObject = _editorObject;
            clone._name = _name;
            clone._namePrevious = _namePrevious;
            return clone;
        }

        public void Do()
        {
            _editorObject.SetName(_name);
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            _editorObject.SetName(_namePrevious);
        }
    }
}
