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

        [DetectLoop]
        public float Spacing => SpacingFunc(ElementArgs);
        [DetectLoop]
        public float Length => IsVertical ? Height : Width;
        [DetectLoop]
        public float Thickness => IsVertical ? Width : Height;

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
                WidthFunc = thickness ?? (args => args.Parent.Width);
                HeightFunc = _ =>
                {
                    var last = this.LastOrDefault();
                    return last == null ?
                        0 :
                        last.Y+ last.Height;
                };
            }
            else if (!IsVertical)
            {
                WidthFunc = _ =>
                {
                    var last = this.LastOrDefault();
                    return last == null ? 
                        0 :
                        last.X+ last.Width;
                };
                HeightFunc = thickness ?? (args => args.Parent.Height);
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
            base.Add(element);
            var cast = (Element)element;
            if (IsVertical)
            {
                cast.YFunc = ChildGetY;
            }
            else
            {
                cast.XFunc = ChildGetX;
            }
        }

        float ChildGetX(ElementArgs args)
        {
            var previous = args.Parent.ElementAtOrDefault(args.Index - 1);
            return
                (previous?.GetRight() ?? 0) +
                (previous == null ? 0 : ((StackFrame)args.Parent).Spacing);
        }

        float ChildGetY(ElementArgs args)
        {
            var previous = args.Parent.ElementAtOrDefault(args.Index - 1);
            return
                (previous?.GetBottom() ?? 0) +
                (previous == null ? 0 : ((StackFrame)args.Parent).Spacing);
        }
    }
}
