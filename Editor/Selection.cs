using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class Selection
    {
        public delegate void EditorObjectHandler(Selection selection);
        public event EditorObjectHandler SelectionChanged;
        /// <summary>
        /// Lock used for preventing the EditorObject selection from being both copied and modified at the same time.
        /// </summary>
        object _lockSelection = new object();
        List<EditorObject> _selectedList = new List<EditorObject>();

        public Selection()
        {
        }

        public void Set(EditorObject selected)
        {
            List<EditorObject> list = new List<EditorObject>();
            list.Add(selected);
            SetRange(list);
        }

        public void SetRange(List<EditorObject> selected)
        {
            lock (_lockSelection)
            {
                _selectedList.Clear();
                _selectedList.AddRange(selected);
            }
            if (SelectionChanged != null)
                SelectionChanged(this);
        }

        public void Add(EditorObject selected)
        {
            lock (_lockSelection)
            {
                _selectedList.Add(selected);
            }
            if (SelectionChanged != null)
                SelectionChanged(this);
        }

        public void AddRange(List<EditorObject> selected)
        {
            lock (_lockSelection)
            {
                _selectedList.AddRange(selected);
            }
            if (SelectionChanged != null)
                SelectionChanged(this);
        }

        public EditorObject GetFirst()
        {
            lock (_lockSelection)
            {
                if (_selectedList.Count > 0)
                {
                    return _selectedList[0];
                }
                return null;
            }
        }

        public List<EditorObject> GetAll()
        {
            lock (_lockSelection)
            {
                return new List<EditorObject>(_selectedList);
            }
        }
    }
}
