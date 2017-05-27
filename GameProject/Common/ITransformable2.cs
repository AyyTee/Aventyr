using OpenTK;

namespace Game.Common
{
    public interface IGetSetTransform : IGetTransform
    {
        /// <summary>Replaces the local transform with a copy of the passed argument.</summary>
        void SetTransform(Transform2 transform);
    }
}
