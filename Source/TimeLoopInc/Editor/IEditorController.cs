using Game.Common;
using Game.Rendering;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc.Editor
{
    public interface IEditorController
    {
        string LevelName { get; set; }
        string SavePath { get; }
        IVirtualWindow Window { get; }
        GridCamera Camera { get; }
        SceneBuilder Scene { get; }

        MouseButton PlaceButton { get; }
        MouseButton SelectButton { get; }
        Key DeleteButton { get; }

        void ApplyChanges(SceneBuilder changedScene, bool undoSkip = false);
    }
}
