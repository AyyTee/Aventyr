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
        ImmutableList<IElement> _cachedChildren;

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
            _cachedChildren = null;
        }

        [DetectLoopAspect]
        public float GetSpacing() => SpacingFunc(ElementArgs);

        ImmutableList<IElement> GetSubFrames()
        {
            if (_cachedChildren != null)
            {
                return _cachedChildren;
            }

            var subFrames = new List<IElement>();
            if (Children.Any())
            {
                ChildFrame firstFrame;
                if (IsVertical)
                {
                    firstFrame = new ChildFrame(
                        Children[0],
                        width: args => args.Parent.GetWidth(),
                        height: args => args.Self.First().GetHeight());
                }
                else
                {
                    firstFrame = new ChildFrame(
                        Children[0],
                        width: args => args.Self.First().GetWidth(),
                        height: args => args.Parent.GetHeight());
                }
                subFrames.Add(firstFrame);

                foreach (var child in Children.Skip(1))
                {
                    ChildFrame frame;
                    var previousFrame = subFrames.Last();
                    if (IsVertical)
                    {
                        frame = new ChildFrame(
                            child,
                            _ => 0,
                            args => previousFrame.GetY() + previousFrame.GetHeight() + ((StackFrame)args.Parent).GetSpacing(),
                            args => args.Parent.GetWidth(),
                            _ => child.GetHeight());
                    }
                    else
                    {
                        frame = new ChildFrame(
                            child,
                            args => previousFrame.GetX() + previousFrame.GetWidth() + ((StackFrame)args.Parent).GetSpacing(),
                            _ => 0,
                            _ => child.GetWidth(),
                            args => args.Parent.GetHeight());
                    }
                    subFrames.Add(frame);
                }

                foreach (var frame in subFrames)
                {
                    frame.ElementArgs = new ElementArgs(this, frame);
                    frame.First().ElementArgs = new ElementArgs(frame, frame.First());
                }
            }

            _cachedChildren = subFrames.ToImmutableList();
            return _cachedChildren;
        }

        class ChildFrame : Element, IElement
        {
            public IElement Child { get; }

            public ChildFrame(
                IElement child,
                ElementFunc<float> x = null,
                ElementFunc<float> y = null,
                ElementFunc<float> width = null,
                ElementFunc<float> height = null)
                : base(x, y, width, height)
            {
                Child = child;
            }

            public IEnumerator<IElement> GetEnumerator() => new List<IElement> { Child }.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
