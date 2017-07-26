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
        readonly IVirtualWindow _window;

        public PlayerTool(IVirtualWindow window)
        {
            _window = window;
        }

        public List<IRenderable> Render(SceneBuilder scene, ICamera2 camera)
        {
            return new List<IRenderable>();
        }

        public SceneBuilder Update(SceneBuilder scene, ICamera2 camera)
        {
            var mousePosition = _window.MouseWorldPos(camera);
            var mouseGridPos = (Vector2i)mousePosition.Floor(Vector2.One);
            if (_window.ButtonPress(MouseButton.Left))
            {
                var entities = scene.Entities
                    .RemoveAll(item => item is Player || item.StartTransform.Position == mouseGridPos)
                    .Add(new Player(new Transform2i(mouseGridPos), 0));
                return scene.With(entities: entities);
            }
            else if (_window.ButtonPress(MouseButton.Right))
            {
                return EditorController.Remove(scene, mouseGridPos);
            }
            return null;
        }
    }
}
