using System;
using Game.Rendering;
using Ui;

namespace TimeLoopInc
{
    public class LevelEditor
    {
        readonly IVirtualWindow _window;
        readonly UiController _menu;

        public LevelEditor(IVirtualWindow window)
        {
            _window = window;
        }
    }
}
