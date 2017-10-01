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
        ImmutableList<IBaseElement> _children = ImmutableList<IBaseElement>.Empty;

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

        public override IEnumerator<Element> GetEnumerator()
        {
            foreach (var child in _children)
            {
                if (child is Element element)
                {
                    yield return element;
                }
                else if (child is IDataTemplate<Element> template)
                {
                    var elements = template.GetElements();
                    foreach (var templateElement in elements)
                    {
                        AddChild(templateElement);
                        yield return templateElement;
                    }
                }
            }
        }

        protected virtual void AddChild(Element element)
        {
            element.ElementArgs = new ElementArgs(this, element, ElementArgs.Controller);
        }

        public void Add(Element element) 
        {
            _children = _children.Add(element);
            AddChild(element);
        }

        public virtual void Add(IDataTemplate<Element> dataTemplate)
        {
            _children = _children.Add(dataTemplate);
        }
    }
}
