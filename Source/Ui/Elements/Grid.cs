using Game.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui.Elements
{
    public class GridRow : NodeElement
    {
        public IEnumerable<float> ColumnWidths => ((Grid)ElementArgs.Parent).ColumnWidths;

        internal ElementFunc<float> _thickness;
        public float Thickness => GetValue(_thickness);

        public GridRow(ElementFunc<float> thickness)
        {
            _thickness = thickness;
            _y = arg =>
            {
                var previous = arg.Previous;
                return previous != null ?
                    previous.Y + previous.Height :
                    0;
            };
        }

        protected override void AddChild(Element element)
        {
            base.AddChild(element);
            element._x = arg =>
            {
                var gridRow = (GridRow)arg.Parent;
                return gridRow.ColumnWidths.Take(arg.Index).Sum();
            };
            element._y = arg =>
            {
                return arg.Parent.Y;
            };
            element._width = arg =>
            {
                var gridRow = (GridRow)arg.Parent;
                return gridRow.ColumnWidths.ElementAt(arg.Index);
            };
            element._height = arg =>
            {
                return ((GridRow)arg.Parent).Thickness;
            };
        }
    }

    public class Grid : Element
    {
        internal ElementFunc<IEnumerable<float>> _columnWidths;
        public IEnumerable<float> ColumnWidths => GetValue(_columnWidths);

        ImmutableList<IBaseElement> _children = ImmutableList<IBaseElement>.Empty;

        public Grid(
            ElementFunc<IEnumerable<float>> columnWidths,
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<bool> hidden = null,
            Style style = null)
            : base(
                  x, 
                  y, 
                  arg => arg.Self.Sum(item => item.Width), 
                  arg => arg.Self.Sum(item => item.Height), 
                  hidden, 
                  style)
        {
            _columnWidths = columnWidths;
        }

        public void Add(GridRow element)
        {
            _children = _children.Add(element);
            AddChild(element);
        }

        public void Add(DataTemplate<GridRow> dataTemplate)
        {
            _children = _children.Add(dataTemplate);
        }

        public Element GetCell(Vector2i cellIndex)
        {
            return this.ElementAt(cellIndex.Y).ElementAt(cellIndex.X);
        }

        public override IEnumerator<Element> GetEnumerator()
        {
            var list = new List<Element>();
            foreach (var child in _children)
            {
                if (child is Element element)
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

        protected virtual void AddChild(Element element)
        {
            element.ElementArgs = new ElementArgs(this, element, ElementArgs.Controller);
        }
    }
}
