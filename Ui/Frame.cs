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
        public Transform2 Transform { get; set; }

        public bool Hidden { get; set; }

        public Vector2 Size { get; set; }

        public Frame(Transform2 transform = null, bool hidden = false)
        {
            Transform = transform ?? new Transform2();
            Hidden = hidden;
        }

        public Frame(out Frame id, Transform2 transform = null, bool hidden = false)
            : this(transform, hidden)
        {
            id = this;
        }

        public List<Model> GetModels() => new List<Model>();

        public bool IsInside(Vector2 localPoint) => false;
    }
}
