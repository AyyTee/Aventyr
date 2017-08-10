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
        Func<ElementArgs, Transform2> Transform { get; }
        /// <summary>
        /// Element and descendent elements are excluded from processing.
        /// </summary>
        bool Hidden { get; }
        Vector2 Size { get; }

        bool IsInside(Vector2 localPoint);
        List<Model> GetModels();

    }
}
