using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorLogic.Command
{
    public class Delete : ICommand
    {
        public bool IsMarker { get; set; }

        public Delete()
        {
            IsMarker = true;
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

        public ICommand ShallowClone()
        {
            return new Delete();
        }
    }
}
