using Game.Common;
using OpenTK;
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
        internal ElementFunc<IEnumerable<float>> _rowHeights;
        internal ElementFunc<float> _columnSpacing;
        internal ElementFunc<float> _rowSpacing;

        public IEnumerable<float> ColumnWidths => GetValue(_columnWidths);
        public IEnumerable<float> RowHeights => GetValue(_rowHeights);
        public float ColumnSpacing => GetValue(_columnSpacing);
        public float RowSpacing => GetValue(_rowSpacing);

        public Grid(
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<IEnumerable<float>> columnWidths = null,
            ElementFunc<IEnumerable<float>> rowHeights = null,
            ElementFunc<float> columnSpacing = null,
            ElementFunc<float> rowSpacing = null,
            ElementFunc<bool> hidden = null,
            Style style = null)
            : base(x, y, GridWidth, GridHeight, hidden, style)
        {
            _columnWidths = columnWidths;
            _rowHeights = rowHeights;
            _columnSpacing = columnSpacing;
            _rowSpacing = rowSpacing;
        }

        protected override void AddChild(Element element)
        {
            base.AddChild(element);
            
            element._x = args =>
            {
                var grid = (Grid)args.Parent;
                var column = GetCellIndex(args).X;
                return grid.CellX(column);
            };
            element._y = args =>
            {
                var grid = (Grid)args.Parent;
                var row = GetCellIndex(args).Y;
                return grid.CellY(row);
            };
            element._width = args => ((Grid)args.Parent).ColumnWidths.ElementAt(GetCellIndex(args).X);
            element._height = args => ((Grid)args.Parent).RowHeights.ElementAt(GetCellIndex(args).Y);
        }

        float CellX(int column)
        {
            return ColumnWidths.Take(column).Sum() + ColumnSpacing * column;
        }

        float CellY(int row)
        {
            return RowHeights.Take(row).Sum() + RowSpacing * row;
        }

        static float GridWidth(ElementArgs args)
        {
            var grid = (Grid)args.Self;
            return grid.ColumnWidths.Sum() + grid.ColumnSpacing * Math.Max(0, grid.ColumnWidths.Count() - 1);
        }

        static float GridHeight(ElementArgs args)
        {
            var grid = (Grid)args.Self;
            var columns = grid.ColumnWidths.Count();
            var rows = (columns - 1 + grid.Count()) / columns;
            return grid.RowHeights.Take(rows).Sum() + grid.RowSpacing * Math.Max(0, rows - 1);
        }

        static Vector2i GetCellIndex(ElementArgs args)
        {
            var grid = (Grid)args.Parent;
            var index = args.Index;
            return new Vector2i(
                index % grid.ColumnWidths.Count(),
                index / grid.ColumnWidths.Count());
        }

        public static new Style DefaultStyle(IUiController controller)
        {
            const int columns = 3;
            const int rows = 3;
            return new Style
            {
                new StyleElement<Grid, IEnumerable<float>>(nameof(ColumnWidths), args => Enumerable.Repeat(args.Parent.Width / columns, columns)),
                new StyleElement<Grid, IEnumerable<float>>(nameof(RowHeights), args => Enumerable.Repeat(args.Parent.Width / rows, rows)),
                new StyleElement<Grid, float>(nameof(ColumnSpacing), _ => 0f),
                new StyleElement<Grid, float>(nameof(RowSpacing), _ => 0f)
            };
        }
    }
}
