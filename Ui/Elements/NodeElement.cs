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

namespace Ui.Elements
{
    /// <summary>
    /// An element that can have child elements.
    /// </summary>
    public abstract class NodeElement : Element
    {
        ImmutableList<IBaseElement> _children = new List<IBaseElement>().ToImmutableList();

        public NodeElement(
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null,
            ElementFunc<float> width = null, 
            ElementFunc<float> height = null, 
            ElementFunc<bool> hidden = null,
            Style style = null)
            : base(x, y, width, height, hidden, style)
        {
        }

        public override IEnumerator<IElement> GetEnumerator()
        {
            var list = new List<IElement>();
            foreach (var child in _children)
            {
                if (child is IElement element)
                {
                    list.Add(element);
                }
                else if (child is IDataTemplate template)
                {
                    var elements = template.GetElements();
                    foreach (var templateElement in elements)
                    {
                        AddChild(templateElement);
                    }
                    list.AddRange(elements);
                }
            }
            return list.GetEnumerator();
        }

        protected virtual void AddChild(IElement element)
        {
            element.ElementArgs = new ElementArgs(this, element);
        }

        public void Add(IElement element) 
        {
            _children = _children.Add(element);
            AddChild(element);
        }

        public virtual void Add(IDataTemplate dataTemplate)
        {
            _children = _children.Add(dataTemplate);
        }
    }
}
