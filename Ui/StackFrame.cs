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

        public IEnumerator<IElement> GetEnumerator() => GetSubFrames().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void Add(IElement element)
        {
            Children = Children.Add(element);
        }

        public float GetSpacing() => SpacingFunc(ElementArgs);

        List<IElement> GetSubFrames()
        {
            var subFrames = new List<IElement>();
            if (!Children.Any())
            {
                return subFrames;
            }

            Frame firstFrame;
            if (IsVertical)
            {
                firstFrame = new Frame(
                    width: args => args.Parent.GetWidth(), 
                    height: _ => Children[0].GetHeight());
            }
            else
            {
                firstFrame = new Frame(
                    width: _ => Children[0].GetWidth(), 
                    height: args => args.Parent.GetHeight());
            }
            subFrames.Add(firstFrame);
            firstFrame.Add(Children[0]);

            foreach (var child in Children.Skip(1))
            {
                Frame frame;
                var previousFrame = subFrames.Last();
                if (IsVertical)
                {
                    frame = new Frame(
                        _ => 0,
                        args => previousFrame.GetY() + previousFrame.GetHeight() + ((StackFrame)args.Parent).GetSpacing(),
                        args => args.Parent.GetWidth(),
                        ElementEx.ChildHeight());
                }
                else
                {
                    frame = new Frame(
                        args => previousFrame.GetX() + previousFrame.GetWidth() + ((StackFrame)args.Parent).GetSpacing(),
                        _ => 0,
                        ElementEx.ChildWidth(),
                        args => args.Parent.GetHeight());
                }
                frame.Add(child);
                subFrames.Add(frame);
            }

            foreach (var frame in subFrames)
            {
                frame.ElementArgs = new ElementArgs(this, frame);
            }

            return subFrames;
        }
    }
}
