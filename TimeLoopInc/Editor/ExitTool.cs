using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Rendering;
using OpenTK.Input;
using Game.Common;
using OpenTK;

namespace TimeLoopInc.Editor
{
    public class ExitTool : ITool
    {
        readonly IEditorController _editor;

        public ExitTool(IEditorController editor)
        {
            _editor = editor;
        }

        public List<IRenderable> Render()
        {
            return new List<IRenderable>();
        }

        public void Update()
        {
            var window = _editor.Window;
            var scene = _editor.Scene;
            var mousePosition = window.MouseWorldPos(_editor.Camera);
            var mouseGridPos = (Vector2i)mousePosition.Floor(Vector2.One);
            if (window.ButtonPress(MouseButton.Left))
            {
                var exits = scene.Exits.Add(mouseGridPos);
                _editor.ApplyChanges(scene.With(exits: exits));
            }
            else if (window.ButtonPress(MouseButton.Right))
            {
                _editor.ApplyChanges(EditorController.Remove(scene, mouseGridPos));
            }
        }
    }
}
