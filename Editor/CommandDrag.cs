using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class CommandDrag : ICommand
    {
        HashSet<MementoDrag> _modified = new HashSet<MementoDrag>();
        Transform2D _transform;

        public CommandDrag(List<ITransform2D> modified, Transform2D transform)
        {
            foreach (ITransform2D e in modified)
            {
                _modified.Add(new MementoDrag(e));
            }
            _transform = transform.Clone();
        }

        public CommandDrag(List<MementoDrag> modified, Transform2D transform)
        {
            _modified.UnionWith(modified);
            _transform = transform.Clone();
        }

        public CommandDrag(HashSet<MementoDrag> modified, Transform2D transform)
        {
            _modified.UnionWith(modified);
            _transform = transform.Clone();
        }

        public void Do()
        {
            foreach (MementoDrag t in _modified)
            {
                Transform2D transform = t.GetTransform().Add(_transform);
                t.Transformable.SetTransform(transform);
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

        public ICommand Clone()
        {
            return new CommandDrag(_modified, _transform);
        }
    }
}
