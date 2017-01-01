using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorLogic.Command
{
    public class AddEntity : Add
    {
        public AddEntity(ControllerEditor controller, EditorEntity editorEntity)
            : base(controller, editorEntity)
        {
        }

        public override void Do()
        {
            base.Do();
        }

        public override ICommand ShallowClone()
        {
            AddEntity clone = new AddEntity(Controller, (EditorEntity)EditorObject);
            return clone;
        }
    }
}
