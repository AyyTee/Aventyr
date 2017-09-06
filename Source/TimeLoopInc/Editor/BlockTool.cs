using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Rendering;
using OpenTK;
using OpenTK.Input;
using Game.Common;

namespace TimeLoopInc.Editor
{
    public class BlockTool : ITool
    {
        readonly IEditorController _editor;

        public BlockTool(IEditorController editor)
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
            if (window.ButtonPress(_editor.PlaceButton))
            {
                var entities = scene.Entities
                    .RemoveAll(item => item.StartTransform.Position == mouseGridPos)
                    .Add(new Block(new Transform2i(mouseGridPos)));
                _editor.ApplyChanges(scene.With(entities: entities));
            }
            EditorController.DeleteAndSelect(_editor, mouseGridPos);
        }
    }
}
