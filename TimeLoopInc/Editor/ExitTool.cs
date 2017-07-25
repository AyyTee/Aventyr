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
        readonly IVirtualWindow _window;

        public ExitTool(IVirtualWindow window)
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
                var exits = scene.Exits.Add(mouseGridPos);
                return scene.With(exits: exits);
            }
            return null;
        }
    }
}
