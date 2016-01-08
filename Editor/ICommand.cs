using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public interface ICommand
    {
        void Do();
        void Redo();
        void Undo();
        ICommand Clone();
    }
}
