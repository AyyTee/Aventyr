using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public struct MementoSelection
    {
        readonly List<EditorObject> _selected;
        public List<EditorObject> Selected { get { return new List<EditorObject>(_selected); } }
        public readonly EditorObject First;

        public MementoSelection(Selection selection)
        {
            _selected = selection.GetAll();
            First = selection.First;
        }
    }
}
