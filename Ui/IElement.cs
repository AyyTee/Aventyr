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

        float GetX();
        float GetY();
        /// <summary>
        /// Element and child elements are excluded from processing.
        /// </summary>
        bool GetHidden();
        float GetWidth();
        float GetHeight();

        bool IsInside(Vector2 localPoint);
        List<Model> GetModels();
    }

    public static class ElementEx
    {
        public static ElementFunc<float> AlignX(float t) =>
            args => (args.Parent.GetWidth() - args.Self.GetWidth()) * t;
        public static ElementFunc<float> AlignY(float t) =>
            args => (args.Parent.GetHeight() - args.Self.GetHeight()) * t;
        public static ElementFunc<float> ChildrenMaxX()
        {
            return args => args.Self.MaxOrNull(
                child =>
                {
                    if (DetectLoop.TryExecute(child.GetX, out float x) && DetectLoop.TryExecute(child.GetWidth, out float width))
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
                    if (DetectLoop.TryExecute(child.GetY, out float y) && DetectLoop.TryExecute(child.GetHeight, out float height))
                    {
                        return y + height;
                    }
                    return 0;
                }) ?? 0;
        }

        public static Vector2 GetPosition(this IElement element) => new Vector2(element.GetX(), element.GetY());
        public static Vector2 GetSize(this IElement element) => new Vector2(element.GetWidth(), element.GetHeight());
        public static Transform2 GetTransform(this IElement element) => new Transform2(element.GetPosition());
        public static float GetBottom(this IElement element) => element.GetY() + element.GetHeight();
        public static float GetRight(this IElement element) => element.GetX() + element.GetWidth();
    }

    public delegate T ElementFunc<T>(ElementArgs args);
}
