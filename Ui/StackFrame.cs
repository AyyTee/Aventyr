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
    /// <summary>
    /// Stacks children elements either vertically or horizontally. 
    /// Note that this class overrides the x or y function of its children (based on whether it's horizontal or vertical respectively).
    /// </summary>
    public class StackFrame : NodeElement, IElement
    {

        public bool IsVertical { get; }

        public ElementFunc<float> SpacingFunc { get; }

        public StackFrame(
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null,
            ElementFunc<float> thickness = null,
            ElementFunc<bool> hidden = null, 
            bool isVertical = true,
            ElementFunc<float> spacing = null)
            : base(x, y, hidden: hidden)
        {
            IsVertical = isVertical;
            SpacingFunc = spacing ?? (_ => 0);

            if (IsVertical)
            {
                WidthFunc = thickness ?? (args => args.Parent.GetWidth());
                HeightFunc = _ =>
                {
                    var last = this.LastOrDefault();
                    return last == null ?
                        0 :
                        last.GetY() + last.GetHeight();
                };
            }
            else if (!IsVertical)
            {
                WidthFunc = _ =>
                {
                    var last = this.LastOrDefault();
                    return last == null ? 
                        0 :
                        last.GetX() + last.GetWidth();
                };
                HeightFunc = thickness ?? (args => args.Parent.GetHeight());
            }
        }

        public StackFrame(
            out StackFrame id,
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null,
            ElementFunc<float> thickness = null,
            ElementFunc<bool> hidden = null, 
            bool isVertical = true,
            ElementFunc<float> spacing = null)
            : this(x, y, thickness, hidden, isVertical, spacing)
        {
            id = this;
        }

        public override void Add(IElement element)
        {
            var previous = this.LastOrDefault();
            Children = Children.Add(element);

            var cast = ((Element)element);

            element.ElementArgs = new ElementArgs(this, element);
            if (IsVertical)
            {
                cast.YFunc = args => 
                    (previous?.GetBottom() ?? 0) + 
                    (previous == null ? 0 : ((StackFrame)args.Parent).GetSpacing());
            }
            else
            {
                cast.XFunc = args => 
                    (previous?.GetRight() ?? 0) + 
                    (previous == null ? 0 : ((StackFrame)args.Parent).GetSpacing());
            }
        }

        [DetectLoopAspect]
        public float GetSpacing() => SpacingFunc(ElementArgs);
        [DetectLoopAspect]
        public float GetLength() => IsVertical ? GetHeight() : GetWidth();
        [DetectLoopAspect]
        public float GetThickness() => IsVertical ? GetWidth() : GetHeight();
    }
}
