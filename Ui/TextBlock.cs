using System;
using System.Collections;
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
        public Transform2 Transform { get; set; } = new Transform2();

        public TextEntity Text { get; set; }

        public bool Hidden { get; set; }

        public TextBlock(TextEntity text)
        {
            DebugEx.Assert(text != null);
            Text = text;
        }

        public TextBlock(TextEntity text, out TextBlock id)
            : this(text)
        {
            id = this;
        }

        public List<Model> GetModels()
        {
            return Text.GetModels();
        }

        public bool IsInside(Vector2 localPoint) => false;

        public IEnumerator<IElement> GetEnumerator()
        {
            return new List<IElement>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
