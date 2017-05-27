using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;

namespace EditorLogic.Command
{
    /// <summary>
    /// Moves a list of ITransform2 instances in world space and accounts for EditorObjects being parented to other instances.
    /// </summary>
    public class Drag : ICommand
    {
        public bool IsMarker { get; set; }
        HashSet<MementoDrag> _modified = new HashSet<MementoDrag>();
        Transform2 _transform;

        public Drag(List<EditorObject> modified, Transform2 transform)
        {
            IsMarker = true;
            foreach (EditorObject e in modified)
            {
                _modified.Add(new MementoDrag(e));
            }
            _transform = transform;
        }

        public Drag(List<MementoDrag> modified, Transform2 transform)
        {
            _modified.UnionWith(modified);
            _transform = transform;
        }

        public Drag(HashSet<MementoDrag> modified, Transform2 transform)
        {
            _modified.UnionWith(modified);
            _transform = transform;
        }

        public void Do()
        {
            foreach (MementoDrag memento in _modified)
            {
                EditorObject editorObject = memento.Transformable as EditorObject;
                if (editorObject != null && editorObject.Parent != null)
                {
                    Transform2 t = editorObject.GetTransform();
                    Transform2 t2 = _transform;

                    Transform2 parent = editorObject.GetWorldTransform();
                    parent = parent.Inverted();

                    //t2 = t2.Transform(parent);
                    //t2.Position = t2.Position / parent.Size;// - parent.Position;
                    t = t.Add(t2);
                    //t.Position += t2.Position;
                    //t.Subtract(editorObject.Parent.GetWorldTransform());
                    editorObject.SetTransform(t);
                }
                else
                {
                    Transform2 t = memento.GetTransform().Add(_transform);
                    memento.Transformable.SetTransform(t);
                }
            }
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            foreach (MementoDrag t in _modified)
            {
                t.ResetTransform();
            }
        }

        public ICommand ShallowClone()
        {
            return new Drag(_modified, _transform);
        }
    }
}
