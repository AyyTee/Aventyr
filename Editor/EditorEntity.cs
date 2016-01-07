using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class EditorEntity : EditorObject
    {
        public Entity Entity { get; private set; }

        public EditorEntity(ControllerEditor controller)
            : base(controller)
        {
            Entity = new Entity(Scene);
            Entity.SetParent(this);
        }
    }
}
