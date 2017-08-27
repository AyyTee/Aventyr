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

namespace Ui.Elements
{
    /// <summary>
    /// Stacks children elements either vertically or horizontally. 
    /// Note that this class overrides the x or y function of its children (based on whether it's horizontal or vertical respectively).
    /// </summary>
    public class StackFrame : NodeElement, IElement
    {
        public bool IsVertical { get; }

        internal ElementFunc<float> _spacing;

        [DetectLoop]
        public float Spacing => InvokeFunc(_spacing);
        public float Length => IsVertical ? Height : Width;
        public float Thickness => IsVertical ? Width : Height;

        public StackFrame(
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null,
            ElementFunc<float> thickness = null,
            ElementFunc<bool> hidden = null, 
            bool isVertical = true,
            ElementFunc<float> spacing = null,
            Style style = null)
            : base(x, y, hidden: hidden, style: style)
        {
            IsVertical = isVertical;
            _spacing = spacing;

            if (IsVertical)
            {
                _width = thickness ?? (args => args.Parent.Width);
                _height = _ =>
                {
                    var last = this.LastOrDefault();
                    return last == null ?
                        0 :
                        last.Y+ last.Height;
                };
            }
            else if (!IsVertical)
            {
                _width = _ =>
                {
                    var last = this.LastOrDefault();
                    return last == null ? 
                        0 :
                        last.X+ last.Width;
                };
                _height = thickness ?? (args => args.Parent.Height);
            }
        }

        public StackFrame(
            out StackFrame id,
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null,
            ElementFunc<float> thickness = null,
            ElementFunc<bool> hidden = null, 
            bool isVertical = true,
            ElementFunc<float> spacing = null,
            Style style = null)
            : this(x, y, thickness, hidden, isVertical, spacing, style)
        {
            id = this;
        }

        public static new Style DefaultStyle(IUiController controller)
        {
            var type = typeof(StackFrame);
            return new Style
            {
                new StyleElement(type, nameof(Spacing), _ => 0f),
            };
        }

        protected override void AddChild(IElement element)
        {
            base.AddChild(element);
            var cast = (Element)element;
            if (IsVertical)
            {
                cast._y = ChildGetY;
            }
            else
            {
                cast._x = ChildGetX;
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
