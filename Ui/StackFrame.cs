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
    public class StackFrame : Element, IElement
    {
        public ImmutableList<IElement> Children { get; set; } = new List<IElement>().ToImmutableList();

        public bool IsVertical { get; }

        public ElementFunc<float> SpacingFunc { get; }

        public StackFrame(
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null,
            ElementFunc<float> width = null,
            ElementFunc<float> height = null,
            ElementFunc<bool> hidden = null, 
            bool isVertical = true,
            ElementFunc<float> spacing = null)
            : base(x, y, width, height, hidden)
        {
            IsVertical = isVertical;
            SpacingFunc = spacing ?? (_ => 0);

            if (IsVertical && height == null)
            {
                HeightFunc = _ =>
                {
                    var last = this.LastOrDefault();
                    return last == null ?
                        0 :
                        last.GetY() + last.GetHeight();
                };
            }
            else if (!IsVertical && width == null)
            {
                WidthFunc = _ =>
                {
                    var last = this.LastOrDefault();
                    return last == null ? 
                        0 :
                        last.GetX() + last.GetWidth();
                };
            }
        }

        public StackFrame(
            out StackFrame id,
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null,
            ElementFunc<float> width = null,
            ElementFunc<float> height = null,
            ElementFunc<bool> hidden = null, 
            bool isVertical = true,
            ElementFunc<float> spacing = null)
            : this(x, y, width, height, hidden, isVertical, spacing)
        {
            id = this;
        }

        public IEnumerator<IElement> GetEnumerator() => Children.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void Add(IElement element)
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
    }
}
