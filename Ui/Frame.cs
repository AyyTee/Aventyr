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
    public class Frame : BranchElement, IElement
    {
        public ElementArgs ElementArgs { get; set; }

        internal Func<ElementArgs, Transform2> GetTransform { get; }
        public Transform2 Transform => GetTransform(ElementArgs);

        public bool Hidden { get; set; }

        public Vector2 Size { get; set; }

        public Frame(Func<ElementArgs, Transform2> transform = null, bool hidden = false)
        {
            GetTransform = transform ?? (_ => new Transform2());
            Hidden = hidden;
        }

        public Frame(out Frame id, Func<ElementArgs, Transform2> transform = null, bool hidden = false)
            : this(transform, hidden)
        {
            id = this;
        }

        public List<Model> GetModels() => new List<Model>();

        public bool IsInside(Vector2 localPoint) => false;
    }
}
