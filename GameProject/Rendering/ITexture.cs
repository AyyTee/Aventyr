using Game.Common;

namespace Game.Rendering
{
    public interface ITexture
    {
        Vector2i Size { get; }

        int GetId();
    }
}
