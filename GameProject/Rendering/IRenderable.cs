using System.Collections.Generic;
using Game.Common;
using Game.Models;

namespace Game.Rendering
{
    public interface IRenderable : IGetWorldTransformVelocity
    {
        List<Model> GetModels();
        bool Visible { get; }
        bool DrawOverPortals { get; }
        bool IsPortalable { get; }
        /// <summary>
        /// ClipPaths in world coordinates for models.
        /// </summary>
        List<ClipPath> ClipPaths { get; }
    }

    public static class IRenderableEx
    {
        public static List<Model> GetClippedModels(this IRenderable renderable)
        {
            var clippedModels = new List<Model>();
            var models = renderable.GetModels();
            foreach (var model in models)
            {
                var clippedModel = model.ShallowClone();
                foreach (var clipPath in renderable.ClipPaths)
                {
                    var matrix = clippedModel.Transform.GetMatrix() * renderable.WorldTransform.GetMatrix();
                    clippedModel.Mesh = clippedModel.Mesh.Bisect(clipPath, matrix);
                }
                clippedModels.Add(clippedModel);
            }
            return clippedModels;
        }
    }
}
