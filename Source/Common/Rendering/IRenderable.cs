using System.Collections.Generic;
using Game.Common;
using Game.Models;
using OpenTK;
using System;
using System.Linq;

namespace Game.Rendering
{
    public interface IRenderable : IGetWorldTransformVelocity
    {
        List<Model> GetModels();
        bool Visible { get; }
        bool DrawOverPortals { get; }
        bool IsPortalable { get; }
        /// <summary>
        /// Attempt to align world transform to pixel grid.
        /// </summary>
        bool PixelAlign { get; }
        /// <summary>
        /// ClipPaths in world coordinates for models.
        /// </summary>
        List<ClipPath> ClipPaths { get; }
    }

    public static class IRenderableEx
    {
        public static List<Model> GetClippedModels(this IRenderable renderable, Transform2 worldTransfom)
        {
            var clippedModels = new List<Model>();
            var models = renderable.GetModels();
            foreach (var model in models)
            {
                var clippedModel = model.ShallowClone();
                foreach (var clipPath in renderable.ClipPaths)
                {
                    var matrix = clippedModel.Transform.GetMatrix() * worldTransfom.GetMatrix();
                    clippedModel.Mesh = clippedModel.Mesh.Bisect(clipPath, matrix);
                }
                clippedModels.Add(clippedModel);
            }
            return clippedModels;
        }

        public static Transform2 PixelAlignedWorldTransform(this IRenderable renderable, ICamera2 camera, Vector2i canvasSize)
        {
            if (renderable.PixelAlign)
            {
                var transform = renderable.WorldTransform;

                var up = transform.GetUp();
                var right = transform.GetRight();
                var points = new[]
                {
                    transform.Position,
                    transform.Position + up,
                    transform.Position + right,
                };
                var screen = camera.WorldToScreen(points, canvasSize);

                var delta = screen[0].Round() - screen[0];
                var aligned = screen.Select(item => item - delta).ToArray();

                var world = camera.ScreenToWorld(aligned, canvasSize);
                return Transform2.FromPoints(world[0], world[1] - world[0], world[2] - world[0]);
            }
            return renderable.WorldTransform;
        }
    }
}
