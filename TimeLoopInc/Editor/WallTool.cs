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
    public class WallTool : ITool
    {
        readonly IEditorController _editor;

        public WallTool(IEditorController editor)
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
            var camera = _editor.Camera;
            var scene = _editor.Scene;
            var mousePosition = window.MouseWorldPos(camera);
            var mouseGridPos = (Vector2i)mousePosition.Floor(Vector2.One);
            if (window.ButtonPress(MouseButton.Left))
            {
                var walls = scene.Walls.Add(mouseGridPos);
                var links = EditorController.GetPortals(
                    portal => EditorController.PortalValidSides(portal.Position, walls).Any(),
                    scene.Links);
                _editor.ApplyChanges(scene.With(walls, links: links));
            }
            else if (window.ButtonPress(MouseButton.Right))
            {
                _editor.ApplyChanges(scene.With(walls: scene.Walls.Remove(mouseGridPos)));
            }
        }
    }
}
