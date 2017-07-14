using System;
using Game.Common;

namespace TimeLoopInc
{
    public class SelectInput : IInput
    {
        public Vector2i GridSelection { get; }

        public SelectInput(Vector2i gridSelection)
        {
            GridSelection = gridSelection;
        }
    }
}
