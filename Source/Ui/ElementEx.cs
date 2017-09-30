using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Game;
using Game.Common;
using Game.Models;
using OpenTK;
using System.Runtime.CompilerServices;
using Ui.Elements;

namespace Ui
{
    public static class ElementEx
    {
        public static IEnumerable<Type> InheritanceHierarchy(this Type type)
        {
            for (var current = type; current != null;)
            {
                yield return current;

                current = current.GenericTypeArguments.Length > 0 ?
                    current.GetGenericTypeDefinition() :
                    current.BaseType;
            }
        }

        public static ElementFunc<float> AlignX(float t) =>
            args => (args.Parent.Width - args.Self.Width) * t;
        public static ElementFunc<float> AlignY(float t) =>
            args => (args.Parent.Height - args.Self.Height) * t;
        public static ElementFunc<float> ChildrenMaxX()
        {
            return args => args.Self.MaxOrNull(
                child =>
                {
                    DetectLoop.TryExecute(() => child.X, out float x);
                    DetectLoop.TryExecute(() => child.Width, out float width);
                    return x + width;
                }) ?? 0;
        }

        public static ElementFunc<float> ChildrenMaxY()
        {
            return args => args.Self.MaxOrNull(
                child =>
                {
                    DetectLoop.TryExecute(() => child.Y, out float y);
                    DetectLoop.TryExecute(() => child.Height, out float height);
                    return y + height;
                }) ?? 0;
        }

        public static Vector2 GetPosition(this Element element) => new Vector2(element.X, element.Y);
        public static Vector2 GetSize(this Element element) => new Vector2(element.Width, element.Height);
        public static Transform2 GetTransform(this Element element) => new Transform2(element.GetPosition());
        public static float GetBottom(this Element element) => element.Y + element.Height;
        public static float GetRight(this Element element) => element.X + element.Width;
    }

    public delegate T ElementFunc<T>(ElementArgs args);
    public delegate void ElementAction<T>(T args) where T : ElementArgs;
}
