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
using MoreLinq;

namespace Ui
{
    public class StackFrame : BranchElement, IElement
    {
        public Transform2 Transform { get; set; }

        public bool Hidden { get; set; }

        public bool IsVertical { get; set; }

        public Vector2 Spacing { get; set; }

        public Vector2 Size { get; set; }

        public StackFrame(Transform2 transform = null, bool hidden = false, bool isVertical = true, Vector2 spacing = new Vector2())
        {
            Transform = transform ?? new Transform2();
            Hidden = hidden;
            IsVertical = isVertical;
            Spacing = spacing;
        }

        public StackFrame(out StackFrame id, Transform2 transform = null, bool hidden = false, bool isVertical = true, Vector2 spacing = new Vector2())
            : this(transform, hidden, isVertical)
        {
            id = this;
        }

        public override List<(IElement Child, Transform2 LocalTransform)> GetLocalTransforms()
        {
            var offset = new Vector2();
            var childTransforms = new List<(IElement Child, Transform2 LocalTransform)>();
            for (int i = 0; i < Children.Count; i++)
            {
                childTransforms.Add((Children[i], new Transform2(offset)));
                offset += Spacing;
                offset += IsVertical ?
                    Children[i].Size.YOnly() :
                    Children[i].Size.XOnly();
            }
            return childTransforms;
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
