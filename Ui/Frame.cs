using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Game.Common;
using Game.Models;
using OpenTK;

namespace Ui
{
    public class Frame : IElement
    {
        public ImmutableList<IElement> Children { get; set; } = new List<IElement>().ToImmutableList();

        public Transform2 Transform { get; set; } = new Transform2();

        public bool Hidden { get; set; }

        public Frame()
        {
        }

        public Frame(out Frame id)
        {
            id = this;
        }

        public Frame(out Frame id, Transform2 transform, bool hidden = false)
            : this(out id)
        {
            Transform = transform;
            Hidden = hidden;
        }

        public List<Model> GetModels() => new List<Model>();

        public bool IsInside(Vector2 localPoint) => false;

		public IEnumerator<IElement> GetEnumerator() => Children.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public void Add(IElement element)
		{
			Children = Children.Concat(new[] { element }).ToImmutableList();
		}
    }
}
