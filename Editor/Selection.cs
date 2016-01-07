using Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class Selection
    {
        public delegate void EditorObjectHandler(Selection selection);
        public event EditorObjectHandler SelectionChanged;
        public readonly Scene Scene;
        EditorObject _first;

        public Selection(Scene scene)
        {
            Scene = scene;
        }

        public void Set(EditorObject selected)
        {
            Reset();
            _first = selected;
            if (selected != null)
            {
                selected.SetSelected(true);
            }
        }

        public void SetRange(List<EditorObject> selected)
        {
            foreach (EditorObject e in selected)
            {
                Debug.Assert(e != null);
            }
            Reset();
            if (selected.Count > 0)
            {
                _first = selected[0];
            }
            foreach (EditorObject e in selected)
            {
                e.SetSelected(true);
            }
            if (SelectionChanged != null)
                SelectionChanged(this);
        }

        public void Reset()
        {
            _first = null;
            foreach (EditorObject e in GetAll())
            {
                e.SetSelected(false);
            }
        }

        public void Toggle(EditorObject selected)
        {
            if (selected == null)
            {
                return;
            }

            if (selected.IsSelected)
            {
                Remove(selected);
            }
            else
            {
                Add(selected);
            }
        }

        public void Add(EditorObject selected)
        {
            if (selected == null)
            {
                return;
            }
            _first = selected;
            selected.SetSelected(true);
            if (SelectionChanged != null)
                SelectionChanged(this);
        }

        public void AddRange(List<EditorObject> selected)
        {
            foreach (EditorObject e in selected)
            {
                Debug.Assert(e != null);
            }
            _first = selected[selected.Count];
            foreach (EditorObject e in selected)
            {
                e.SetSelected(true);
            }
            if (SelectionChanged != null)
                SelectionChanged(this);
        }

        public EditorObject GetFirst()
        {
            return _first;
        }

        public bool Remove(EditorObject deselect)
        {
            if (deselect == null)
            {
                return false;
            }
            if (_first == deselect)
            {
                _first = null;
            }
            bool wasSelected = deselect.IsSelected;
            deselect.SetSelected(false);
            return wasSelected;
        }

        public List<EditorObject> GetAll()
        {
            return Scene.FindByType<EditorObject>().FindAll(item => item.IsSelected);
        }
    }
}
