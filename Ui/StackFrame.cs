using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Models;
using OpenTK;
using System.Collections.Immutable;

namespace Ui
{
    public class StackFrame : BranchElement, IElement
    {
        public Transform2 Transform { get; set; }

        public bool Hidden { get; set; }

        public bool IsVertical { get; set; }

        public float Spacing { get; set; }

        public StackFrame(Transform2 transform = null, bool hidden = false, bool isVertical = true, float spacing = 0)
        {
            Transform = transform ?? new Transform2();
            Hidden = hidden;
            IsVertical = isVertical;
            Spacing = spacing;
        }

        public StackFrame(out StackFrame id, Transform2 transform = null, bool hidden = false, bool isVertical = true, float spacing = 0)
            : this(transform, hidden, isVertical)
        {
            id = this;
        }

        public bool IsInside(Vector2 localPoint)
        {
            return false;
        }

        public List<Model> GetModels()
        {
            return new List<Model>();
        }
    }
}
