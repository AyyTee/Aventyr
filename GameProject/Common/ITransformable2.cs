using OpenTK;

namespace Game.Common
{
    public interface ITransformable2 : IGetTransform
    {
        /// <summary>Replaces the local transform with a copy of the passed argument.</summary>
        void SetTransform(Transform2 transform);
    }

    public static class ITransformable2Ex
    {
        public static void SetPosition(this ITransformable2 transformable, Vector2 position)
        {
            var transform = transformable.GetTransform();
            transform.Position = position;
            transformable.SetTransform(transform);
        }

        public static void SetRotation(this ITransformable2 transformable, float rotation)
        {
            var transform = transformable.GetTransform();
            transform.Rotation = rotation;
            transformable.SetTransform(transform);
        }

        public static void SetSize(this ITransformable2 transformable, float size)
        {
            var transform = transformable.GetTransform();
            transform.Size = size;
            transformable.SetTransform(transform);
        }
    }
}
