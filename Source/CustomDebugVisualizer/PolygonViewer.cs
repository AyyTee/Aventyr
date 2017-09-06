using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenTK;
using OxyPlot;
using OxyPlot.Wpf;
using System.Windows.Controls;
using System.Windows.Media;
using CustomDebugVisualizer;
using OxyPlot.Axes;
using OxyPlot.Series;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using VerticalAlignment = System.Windows.VerticalAlignment;

#region Target datatypes
/* We can't constrain List to only some generic types unfortunately.  
 * As a result, our visualizer will claim to work for any List<T> but will throw a runtime 
 * exception for some of them.  Not much can be done about this.*/
[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(PolygonViewer),
typeof(VisualizerObjectSource),
Target = typeof(List<>),
Description = PolygonViewer.Description)]

[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(PolygonViewer),
typeof(VisualizerObjectSource),
Target = typeof(Vector2[]),
Description = PolygonViewer.Description)]

[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(PolygonViewer),
typeof(VisualizerObjectSource),
Target = typeof(Vector2d[]),
Description = PolygonViewer.Description)]

[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(PolygonViewer),
typeof(VisualizerObjectSource),
Target = typeof(List<Vector2d>[]),
Description = PolygonViewer.Description)]

[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(PolygonViewer),
typeof(VisualizerObjectSource),
Target = typeof(List<Vector2>[]),
Description = PolygonViewer.Description)]
#endregion
namespace CustomDebugVisualizer
{
    public class PolygonViewer : DialogDebuggerVisualizer
    {
        public const string Description = "Graph Vertices";

        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            if (windowService == null)
                throw new ArgumentNullException(nameof(windowService));
            if (objectProvider == null)
                throw new ArgumentNullException(nameof(objectProvider));

            object data = objectProvider.GetObject();
            Grid grid = GetGrid(data);
            if (grid == null)
            {
                return;
            }

            var window = new System.Windows.Window
            {
                Width = 500,
                Height = 510,
                Content = grid
            };
            window.ShowDialog();
        }

        static ICollection<List<Vector2d>> CastData(object data)
        {
            var vertices = new List<List<Vector2d>>();

            var cast2 = data as IEnumerable<Vector2>;
            if (cast2 != null)
            {
                vertices.Add(((IEnumerable<Vector2>) data).Select(item => (Vector2d)item).ToList());
                return vertices;
            }
            var cast2D = data as IEnumerable<Vector2d>;
            if (cast2D != null)
            {
                vertices.Add(((IEnumerable<Vector2d>)data).ToList());
                return vertices;
            }
            var cast2Collection = data as ICollection<List<Vector2>>;
            if (cast2Collection != null)
            {
                return cast2Collection.Select(item => item.Select(v => (Vector2d)v).ToList()).ToList();
            }
            return data as ICollection<List<Vector2d>>;
        }

        public static Grid GetGrid(object data)
        {
            ICollection<List<Vector2d>> vertices = CastData(data);
            if (vertices == null)
            {
                return null;
            }

            var grid = new Grid();

            if (vertices.Any(item => item.Any()))
            {
                var model = new PlotModel();
                grid.Children.Add(new PlotView { Model = model });

                foreach (var list in vertices)
                {
                    var point = new OxyPlot.Series.ScatterSeries { MarkerType = MarkerType.Circle };
                    point.Points.Add(new ScatterPoint(list.First().X, list.First().Y));

                    var lines = new OxyPlot.Series.LineSeries { MarkerType = MarkerType.Circle };
                    lines.Points.AddRange(list.Select(item => new DataPoint(item.X, item.Y)));
                    
                    model.Series.Add(lines);
                    model.Series.Add(point);
                }

                SetViewRegion(model, vertices);
            }
            else
            {
                grid.Children.Add(
                    new Label
                    {
                        Content = "No vertices to show.",
                        FontSize = 20,
                        Foreground = new SolidColorBrush(Colors.Gray),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    });
            }

            return grid;
        }

        /// <summary>
        /// Tests the visualizer by hosting it outside of the debugger.
        /// </summary>
        /// <param name="objectToVisualize">The object to display in the visualizer.</param>
        public static void TestShowVisualizer(object objectToVisualize)
        {
            var visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(PolygonViewer), typeof(VisualizerObjectSource));
            visualizerHost.ShowVisualizer();
        }

        internal static void SetViewRegion(PlotModel model, IEnumerable<IEnumerable<Vector2d>> vertices)
        {
            var vMin = new Vector2d();
            var vMax = new Vector2d();
            foreach (var list in vertices)
            {
                vMin = Vector2d.Min(vMin, new Vector2d(list.Min(item => item.X), list.Min(item => item.Y)));
                vMax = Vector2d.Max(vMax, new Vector2d(list.Max(item => item.X), list.Max(item => item.Y)));
            }
            

            double margin = 1;
            if (vertices.Sum(item => item.Count()) >= 2)
            {
                double xDiff = vMax.X - vMin.X;
                double yDiff = vMax.Y - vMin.Y;
                if (xDiff > yDiff)
                {
                    vMin.Y -= (xDiff - yDiff) / 2;
                    vMax.Y += (xDiff - yDiff) / 2;
                }
                else
                {
                    vMin.X -= (yDiff - xDiff) / 2;
                    vMax.X += (yDiff - xDiff) / 2;
                }

                double diff = vMax.X - vMin.X;
                const double marginPercent = 0.1f;
                margin = diff * marginPercent;
            }

            vMin -= new Vector2d(margin, margin);
            vMax += new Vector2d(margin, margin);

            model.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Minimum = vMin.X,
                Maximum = vMax.X,
                Position = AxisPosition.Bottom
            });
            model.Axes.Add(new OxyPlot.Axes.LinearAxis()
            {
                Minimum = vMin.Y,
                Maximum = vMax.Y,
                Position = AxisPosition.Left
            });
        }
    }
}
