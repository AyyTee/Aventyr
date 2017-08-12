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

        public IEnumerator<IElement> GetEnumerator() => GetSubFrames().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void Add(IElement element)
        {
            Children = Children.Concat(new[] { element }).ToImmutableList();
        }

        public bool IsVertical { get; }

        public float Spacing { get; set; }

        public StackFrame(
            Func<ElementArgs, Transform2> transform = null, 
            Func<ElementArgs, float> width = null,
            Func<ElementArgs, float> height = null,
            bool hidden = false, 
            bool isVertical = true, 
            float spacing = 0)
            : base(transform, width, height)
        {
            Hidden = hidden;
            IsVertical = isVertical;
            Spacing = spacing;
        }

        public StackFrame(
            out StackFrame id, 
            Func<ElementArgs, Transform2> transform = null,
            Func<ElementArgs, float> width = null,
            Func<ElementArgs, float> height = null,
            bool hidden = false, 
            bool isVertical = true, 
            Vector2 spacing = new Vector2())
            : this(transform, width, height, hidden, isVertical)
        {
            id = this;
        }

        List<IElement> GetSubFrames()
        {
            var subFrames = new List<IElement>();
            if (!Children.Any())
            {
                return subFrames;
            }

            if (IsVertical)
            {
                subFrames.Add(new Frame(
                    width: args => args.Parent.GetWidth(), 
                    height: _ => Children[0].GetHeight())
                {
                    Children[0]
                });
            }
            else
            {
                subFrames.Add(new Frame(
                    width: _ => Children[0].GetWidth(), 
                    height: args => args.Parent.GetHeight())
                {
                    Children[0]
                });
            }

            foreach (var child in Children.Skip(1))
            {
                Frame frame;
                var previousFrame = subFrames.Last();
                if (IsVertical)
                {
                    frame = new Frame(
                        args => previousFrame.GetTransform()
                            .AddPosition(new Vector2(0, previousFrame.GetHeight() + ((StackFrame)args.Parent).Spacing)),
                        args => args.Parent.GetWidth(),
                        _ => child.GetHeight())
                    {
                        child
                    };
                }
                else
                {
                    frame = new Frame(
                        args => previousFrame.GetTransform()
                            .AddPosition(new Vector2(previousFrame.GetWidth() + ((StackFrame)args.Parent).Spacing, 0)),
                        _ => child.GetWidth(),
                        args => args.Parent.GetHeight())
                    {
                        child
                    };
                }
                subFrames.Add(frame);
            }

            foreach (var frame in subFrames)
            {
                frame.ElementArgs = new ElementArgs(this, frame);
            }

            return subFrames;
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
