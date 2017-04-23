using Game.Common;

namespace Game.Rendering
{
    public interface IClientSizeProvider
    {
        Vector2i ClientSize { get; }
    }
}