using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc.Editor
{
    public interface IEditorController
    {
        IVirtualWindow Window { get; }
        GridCamera Camera { get; }
        SceneBuilder Scene { get; }

        void ApplyChanges(SceneBuilder changedScene);
    }
}
