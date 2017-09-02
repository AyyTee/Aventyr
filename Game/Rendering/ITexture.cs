using Game.Common;

namespace Game.Rendering
{
    public interface ITexture
    {
        /// <summary>
        /// Size of texture in pixel coordinates.
        /// </summary>
        Vector2i Size { get; }
        bool IsTransparent { get; }
        int Id { get; }
    }
}
