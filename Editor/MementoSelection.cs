using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public struct MementoSelection
    {
        public readonly ReadOnlyCollection<EditorObject> Selected;
        public readonly EditorObject First;

        public MementoSelection(Selection selection)
        {
            Selected = selection.GetAll().AsReadOnly();
            First = selection.First;
        }
    }
}
