using System.Collections.Generic;
using Game.Common;
using Game.Models;
using OpenTK;
using System;

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

        public static Transform2 PixelAlignedWorldTransform(this IRenderable renderable, ICamera2 camera, Vector2i canvasSize)
        {
            if (renderable.PixelAlign)
            {
                var maxDelta = 0.001f;
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
                if (ComponentMinDifference(screen[0], screen[1]) < maxDelta && 
                    ComponentMinDifference(screen[0], screen[2]) < maxDelta &&
                    Math.Abs(ComponentMaxDifference(screen[0], screen[1]) - 1) < maxDelta &&
                    Math.Abs(ComponentMaxDifference(screen[0], screen[2]) - 1) < maxDelta)
                {
                    Vector2[] aligned;
                    if (XAligned(screen[0], screen[1], maxDelta * 2))
                    {
                        aligned = new[]
                        {
                            screen[0].Round(),
                            new Vector2((float)Math.Round(screen[0].X), (float)Math.Round(screen[1].Y)),
                            new Vector2((float)Math.Round(screen[2].X), (float)Math.Round(screen[0].Y))
                        };
                    }
                    else
                    {
                        aligned = new[]
                        {
                            screen[0].Round(),
                            new Vector2((float)Math.Round(screen[1].X), (float)Math.Round(screen[0].Y)),
                            new Vector2((float)Math.Round(screen[0].X), (float)Math.Round(screen[2].Y))
                        };
                    }

                    var world = camera.ScreenToWorld(aligned, canvasSize);
                    return Transform2.FromPoints(world[0], world[1], world[2]);
                }
            }
            return renderable.WorldTransform;
        }
        
        static float ComponentMinDifference(Vector2 v0, Vector2 v1)
        {
            var delta = (v0 - v1).Abs();
            return Math.Min(delta.X, delta.Y);
        }

        static float ComponentMaxDifference(Vector2 v0, Vector2 v1)
        {
            var delta = (v0 - v1).Abs();
            return Math.Max(delta.X, delta.Y);
        }

        static bool XAligned(Vector2 v0, Vector2 v1, float maxDelta)
        {
            return Math.Abs(v0.X - v1.X) < 0.002f;
        }

        static bool YAligned(Vector2 v0, Vector2 v1, float maxDelta)
        {
            return Math.Abs(v0.X - v1.X) < 0.002f;
        }
    }
}
