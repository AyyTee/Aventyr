using Game;
using System;

namespace Ui
{
    public interface IUiController
    {
        Resources Fonts { get; }
        ISelectable Selected { get; }
        IHoverable Hovered { get; }
        DateTime DateTime { get; }
    }
}