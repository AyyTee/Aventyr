using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Game.Common;
using Game.Models;
using Game.Rendering;
using OpenTK;

namespace Ui
{
    public class TextBlock : IElement
    {
        public ImmutableList<IElement> Children { get; set; } = new List<IElement>().ToImmutableList();

        public Transform2 Transform { get; set; } = new Transform2();

        public TextEntity Text { get; set; }

        public TextBlock(TextEntity text)
        {
            DebugEx.Assert(text != null);
            Text = text;
        }

        public List<Model> GetModels()
        {
            return Text.GetModels();
        }

        public bool IsInside(Vector2 localPoint) => false;
    }
}
