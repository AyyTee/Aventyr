using Game.Common;

namespace Game
{
    public interface IGetTransformVelocity : IGetTransform
    {
        /// <summary>
        /// Returns a copy of the local velocity.
        /// </summary>
        Transform2 GetVelocity();
    }
}
