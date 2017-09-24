using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;
using Game.Common;
using Game.Rendering;
using Game.Models;
using OpenTK.Graphics;
using System.Collections.Immutable;

namespace TimeLoopInc.Editor
{
    public class BlockTool : ITool
    {
        readonly IEditorController _editor;
        readonly Renderable _box;

        public BlockTool(IEditorController editor)
        {
            _editor = editor;
            _box = new Renderable(
                new Transform2(),
                ModelResources.Box(_editor.Window.Resources).Load(_editor.Window).Models
                    .Select(item => {
                        var clone = item.ShallowClone();
                        clone.Mesh = new ReadOnlyMesh(
                            clone.Mesh
                                .GetVertices()
                                .Select(v => v.With(color: new Color4(1, 1, 1, 0.5f)))
                                .ToImmutableArray(), 
                            clone.Mesh
                                .GetIndices()
                                .ToImmutableArray());
                        return clone;
                    })
                    .ToList());
            //_box.Models[0].IsDithered = true;
        }

        public List<IRenderable> Render()
        {
            var mousePos = _editor.Window.MouseWorldPos(_editor.Camera);
            var mouseGridPos = (Vector2i)mousePos.Floor(Vector2.One);
            _box.WorldTransform = new Transform2((Vector2)mouseGridPos + Vector2.One / 2);
            return new List<IRenderable> { _box };
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
