using Game.Common;

namespace Game
{
    public interface IGetTransform
    {
        /// <summary>Returns a copy of the local transform.</summary>
        Transform2 GetTransform();
    }
}
