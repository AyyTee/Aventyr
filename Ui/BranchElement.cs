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
    public abstract class BranchElement : Element, IEnumerable<IElement>
    {
        public ImmutableList<IElement> Children { get; set; } = new List<IElement>().ToImmutableList();

        public BranchElement(
            Func<ElementArgs, Transform2> transform = null, 
            Func<ElementArgs, float> width = null, 
            Func<ElementArgs, float> height = null, 
            Func<ElementArgs, bool> hidden = null)
            : base(transform, width, height, hidden)
        {
        }

        public IEnumerator<IElement> GetEnumerator() => Children.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void Add(IElement element)
        {
            Children = Children.Concat(new[] { element }).ToImmutableList();
        }
    }
}
