using Game.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui.Elements
{
    public class Grid : NodeElement
    {
        internal ElementFunc<IEnumerable<float>> _columnWidths;
        public IEnumerable<float> ColumnWidths => GetValue(_columnWidths);

        internal ElementFunc<IEnumerable<float>> _rowHeights;
        public IEnumerable<float> RowHeights => GetValue(_rowHeights);

        public Grid(
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<IEnumerable<float>> columnWidths = null,
            ElementFunc<IEnumerable<float>> rowHeights = null,
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
            _rowHeights = rowHeights;
        }

        protected override void AddChild(Element element)
        {
            base.AddChild(element);
            
            element._x = args => ((Grid)args.Parent).ColumnWidths.Take(GetCellIndex(args).X).Sum();
            element._y = args => ((Grid)args.Parent).RowHeights.Take(GetCellIndex(args).Y).Sum();
            element._width = args => ((Grid)args.Parent).ColumnWidths.ElementAt(GetCellIndex(args).X);
            element._height = args => ((Grid)args.Parent).RowHeights.ElementAt(GetCellIndex(args).Y);
        }

        static Vector2i GetCellIndex(ElementArgs args)
        {
            var grid = (Grid)args.Parent;
            var index = args.Index;
            return new Vector2i(
                index % grid.ColumnWidths.Count(),
                index / grid.ColumnWidths.Count());
        }
    }
}
