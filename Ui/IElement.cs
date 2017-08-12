using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Game;
using Game.Common;
using Game.Models;
using OpenTK;

namespace Ui
{
    public interface IElement : IEnumerable<IElement>
    {
        ElementArgs ElementArgs { get; set; }

        Transform2 GetTransform();
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
        public static Transform2 Center(ElementArgs args) => new Transform2((args.Parent.GetSize() - args.Self.GetSize()) / 2);

        public static Vector2 GetSize(this IElement element) => new Vector2(element.GetWidth(), element.GetHeight());
    }
}
