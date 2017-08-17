using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Game;
using Game.Common;
using Game.Models;
using OpenTK;
using System.Runtime.CompilerServices;

namespace Ui
{
    public interface IElement : IEnumerable<IElement>
    {
        ElementArgs ElementArgs { get; set; }

        float X { get; }

        float Y { get; }

        /// <summary>
        /// Element and child elements are excluded from processing.
        /// </summary>
        bool Hidden { get; }
        float Width { get; }

        float Height { get; }

        bool IsInside(Vector2 localPoint);
        List<Model> GetModels();
    }

    public static class ElementEx
    {
        public static ElementFunc<float> AlignX(float t) =>
            args => (args.Parent.Width- args.Self.Width) * t;
        public static ElementFunc<float> AlignY(float t) =>
            args => (args.Parent.Height- args.Self.Height) * t;
        public static ElementFunc<float> ChildrenMaxX()
        {
            return args => args.Self.MaxOrNull(
                child =>
                {
                    if (DetectLoop.TryExecute(() => child.X, out float x) && DetectLoop.TryExecute(() => child.Width, out float width))
                    {
                        return x + width;
                    }
                    return 0;
                }) ?? 0;
        }
            
        public static ElementFunc<float> ChildrenMaxY()
        {
            return args => args.Self.MaxOrNull(
                child =>
                {
                    if (DetectLoop.TryExecute(() => child.Y, out float y) && DetectLoop.TryExecute(() => child.Height, out float height))
                    {
                        return y + height;
                    }
                    return 0;
                }) ?? 0;
        }

        public static Vector2 GetPosition(this IElement element) => new Vector2(element.X, element.Y);
        public static Vector2 GetSize(this IElement element) => new Vector2(element.Width, element.Height);
        public static Transform2 GetTransform(this IElement element) => new Transform2(element.GetPosition());
        public static float GetBottom(this IElement element) => element.Y+ element.Height;
        public static float GetRight(this IElement element) => element.X+ element.Width;
    }

    public delegate T ElementFunc<T>(ElementArgs args);
}
