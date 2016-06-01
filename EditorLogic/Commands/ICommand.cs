using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorLogic.Command
{
    public interface ICommand
    {
        void Do();
        void Redo();
        void Undo();
        ICommand Clone();
    }
}
