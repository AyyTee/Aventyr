using Game.Common;

namespace Game.Rendering
{
    public interface ITexture
    {
        /// <summary>
        /// Size of texture in pixel coordinates.
        /// </summary>
        /// <value>The size.</value>
        Vector2i Size { get; }

        int GetId();
    }
}
