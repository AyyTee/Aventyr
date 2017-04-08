using System.Drawing;

namespace Game.Rendering
{
    public interface IClientSizeProvider
    {
        Size ClientSize { get; }
    }
}