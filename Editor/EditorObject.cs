using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public abstract class EditorObject
    {
        public EditorObject()
        {

        }

        public abstract void SetTransform(Transform2D transform);
        public abstract Transform2D GetTransform();
        public abstract Transform2D GetWorldTransform();
    }
}
