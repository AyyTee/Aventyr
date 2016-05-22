using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor.Command
{
    public class Delete : ICommand
    {
        public Delete()
        {

        }

        public void Do()
        {

        }

        public void Redo()
        {

        }

        public void Undo()
        {

        }

        public ICommand Clone()
        {
            return new Delete();
        }
    }
}
