using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc.Editor
{
    public interface ITool
    {
        /// <summary>
        /// Returns a modified copy of the scene or null if no changes are made.
        /// </summary>
        SceneBuilder Update(SceneBuilder scene, ICamera2 camera);
        List<IRenderable> Render(SceneBuilder scene, ICamera2 camera);
    }
}
