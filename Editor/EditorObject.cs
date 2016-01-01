using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public abstract class EditorObject
    {
        public ControllerEditor Controller { get; private set; }
        public EditorObject(ControllerEditor controller)
        {
            Debug.Assert(controller != null);
            Controller = controller;
        }

        public EditorObject(EditorObject editorObject)
        {
            Controller = editorObject.Controller;
        }

        public virtual void SetTransform(Transform2D transform)
        {
            Controller.SetEditorObjectModified();
        }
        public virtual void SetPosition(Vector2 position)
        {
            Controller.SetEditorObjectModified();
        }
        public abstract Transform2D GetTransform();
        public abstract Transform2D GetWorldTransform();
    }
}
