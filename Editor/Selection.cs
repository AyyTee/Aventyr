using Game;
using OpenTK;
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
        public readonly EditorScene Scene;
        public EditorObject First { get; private set; }
        Entity _firstMarker;

        public Selection(EditorScene scene)
        {
            Scene = scene;
            _firstMarker = new Entity(Scene.Scene);
            _firstMarker.AddModel(ModelFactory.CreateCircle(new Vector3(), 0.08f, 10));
            _firstMarker.ModelList[0].SetColor(new Vector3(0f, 1f, 0f));
            _firstMarker.ModelList[0].Transform.Position = new Vector3(0, 0, DrawDepth.EntityActive);
            _firstMarker.DrawOverPortals = true;
            _firstMarker.SetParent(null);
        }

        public void Set(EditorObject selected)
        {
            Reset();
            SetFirst(selected);
            if (selected != null)
            {
                selected.SetSelected(true);
            }
        }

        public void SetRange(MementoSelection selected)
        {
            SetRange(selected.Selected.ToList());
            SetFirst(selected.First);
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
                SetFirst(selected[0]);
            }
            foreach (EditorObject e in selected)
            {
                e.SetSelected(true);
            }
            if (SelectionChanged != null)
                SelectionChanged(this);
        }

        public void SetFirst(EditorObject first)
        {
            First = first;
            if (First != null)
            {
                _firstMarker.Visible = true;
                //_firstMarker.SetParent(First.Marker);
            }
            else
            {
                _firstMarker.Visible = false;
                //_firstMarker.SetParent(null);
            }
        }

        public void Reset()
        {
            SetFirst(null);
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
            SetFirst(selected);
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
            SetFirst(selected[0]);
            foreach (EditorObject e in selected)
            {
                e.SetSelected(true);
            }
            if (SelectionChanged != null)
                SelectionChanged(this);
        }

        public bool Remove(EditorObject deselect)
        {
            if (deselect == null)
            {
                return false;
            }
            if (First == deselect)
            {
                if (GetAll().Count > 0)
                {
                    SetFirst(GetAll()[0]);
                }
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
