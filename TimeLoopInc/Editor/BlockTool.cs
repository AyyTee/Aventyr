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
        readonly IVirtualWindow _window;

        public BlockTool(IVirtualWindow window)
        {
            _window = window;
        }

        public List<IRenderable> Render(SceneBuilder scene, ICamera2 camera)
        {
            return new List<IRenderable>();
        }

        public SceneBuilder Update(SceneBuilder scene, ICamera2 camera)
        {
            if (_window.ButtonPress(MouseButton.Left))
            {
                var mousePosition = _window.MouseWorldPos(camera);
                var mouseGridPos = (Vector2i)mousePosition.Floor(Vector2.One);
                var entities = scene.Entities
                    .RemoveAll(item => item.StartTransform.Position == mouseGridPos)
                    .Add(new Block(new Transform2i(mouseGridPos)));
                return scene.With(entities: entities);
            }
            return null;
        }
    }
}
