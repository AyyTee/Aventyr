using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class CommandAddEntity : CommandAdd
    {
        public CommandAddEntity(ControllerEditor controller, EditorEntity editorEntity)
            : base(controller, editorEntity)
        {
        }

        public override void Do()
        {
            base.Do();
        }

        public override ICommand Clone()
        {
            CommandAddEntity clone = new CommandAddEntity(_controller, (EditorEntity)_editorObject);
            return clone;
        }
    }
}
