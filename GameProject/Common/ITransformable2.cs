namespace Game.Common
{
    public interface ITransformable2 : IGetTransform
    {
        /// <summary>Replaces the local transform with a copy of the passed argument.</summary>
        void SetTransform(Transform2 transform);
    }
}
