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
    public abstract class NodeElement : Element, IElement
    {
        public ImmutableList<IBaseElement> Children { get; set; } = new List<IBaseElement>().ToImmutableList();

        public NodeElement(
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null,
            ElementFunc<float> width = null, 
            ElementFunc<float> height = null, 
            ElementFunc<bool> hidden = null)
            : base(x, y, width, height, hidden)
        {
        }

        public IEnumerator<IElement> GetEnumerator()
        {
            var list = new List<IElement>();
            foreach (var child in Children)
            {
                if (child is IElement element)
                {
                    list.Add(element);
                }
                else if (child is IDataTemplate template)
                {
                    list.AddRange(template.GetElements());
                }
            }
            return list.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public virtual void Add(IElement element) 
        {
            Children = Children.Add(element);
            element.ElementArgs = new ElementArgs(this, element);
        }

        public virtual void Add(IDataTemplate dataTemplate)
        {
            Children = Children.Add(dataTemplate);
        }
    }
}
