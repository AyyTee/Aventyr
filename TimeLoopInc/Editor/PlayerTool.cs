using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Rendering;
using OpenTK.Input;
using OpenTK;
using Game.Common;

namespace TimeLoopInc.Editor
{
    public class PlayerTool : ITool
    {
        readonly IEditorController _editor;

        public PlayerTool(IEditorController editor)
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
                var entities = scene.Entities
                    .RemoveAll(item => item is Player || item.StartTransform.Position == mouseGridPos)
                    .Add(new Player(new Transform2i(mouseGridPos), 0));
                _editor.ApplyChanges(scene.With(entities: entities));
            }
            else if (window.ButtonPress(MouseButton.Right))
            {
                _editor.ApplyChanges(EditorController.Remove(scene, mouseGridPos));
            }
        }
    }
}
