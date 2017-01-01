using System.Drawing;

namespace Game
{
    public interface IController
    {
        Size CanvasSize { get; }
        IInput Input {get;}
    }
}
