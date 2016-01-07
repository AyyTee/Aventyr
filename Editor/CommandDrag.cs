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
        HashSet<MementoTransform2D> _modified = new HashSet<MementoTransform2D>();
        Transform2D _transform;

        public CommandDrag(List<ITransform2D> modified, Transform2D transform)
        {
            foreach (ITransform2D e in modified)
            {
                _modified.Add(new MementoTransform2D(e));
            }
            _transform = transform.Clone();
        }

        public CommandDrag(List<MementoTransform2D> modified, Transform2D transform)
        {
            _modified.UnionWith(modified);
            _transform = transform.Clone();
        }

        public CommandDrag(HashSet<MementoTransform2D> modified, Transform2D transform)
        {
            _modified.UnionWith(modified);
            _transform = transform.Clone();
        }

        public void Do()
        {
            foreach (MementoTransform2D t in _modified)
            {
                Transform2D transform = t.Transform.Add(_transform);
                t.Transformable.SetTransform(transform);
            }
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            foreach (MementoTransform2D t in _modified)
            {
                t.ResetTransform();
            }
        }

        public ICommand DeepClone()
        {
            return new CommandDrag(_modified, _transform);
        }
    }
}
