using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK;
using OxyPlot;
using OxyPlot.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OxyPlot.Axes;
using OxyPlot.Series;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using VerticalAlignment = System.Windows.VerticalAlignment;

#region Target datatypes
[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(CustomDebugVisualizer.PolygonViewer),
typeof(VisualizerObjectSource),
Target = typeof(List<Vector2>),
Description = "Graph vertices")]

[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(CustomDebugVisualizer.PolygonViewer),
typeof(VisualizerObjectSource),
Target = typeof(Vector2[]),
Description = "Graph vertices")]

[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(CustomDebugVisualizer.PolygonViewer),
typeof(VisualizerObjectSource),
Target = typeof(List<Vector2d>),
Description = "Graph vertices")]

[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(CustomDebugVisualizer.PolygonViewer),
typeof(VisualizerObjectSource),
Target = typeof(Vector2d[]),
Description = "Graph vertices")]
#endregion
namespace CustomDebugVisualizer
{
    public class PolygonViewer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            if (windowService == null)
                throw new ArgumentNullException(nameof(windowService));
            if (objectProvider == null)
                throw new ArgumentNullException(nameof(objectProvider));

            var window = new Window
            {
                Width = 500,
                Height = 510
            };

            window.Content = GetGrid(objectProvider.GetObject());
            window.ShowDialog();
        }

        public static Grid GetGrid(object data)
        {
            IEnumerable<Vector2d> vertices;
            
            if (data as IEnumerable<Vector2> != null)
            {
                vertices = (data as IEnumerable<Vector2>).Select(item => (Vector2d)item);
            }
            else if (data as IEnumerable<Vector2d> != null)
            {
                vertices = (IEnumerable<Vector2d>)data;
            }
            else
            {
                throw new InvalidDataException(nameof(PolygonViewer) + " cannot display this data type.");
            }
            //var vertices = (IEnumerable<Vector2>)objectProvider.GetObject();

            var grid = new Grid();

            if (vertices.Any())
            {
                var model = new PlotModel();
                grid.Children.Add(new PlotView { Model = model });

                var point = new OxyPlot.Series.ScatterSeries { MarkerType = MarkerType.Circle };
                point.Points.Add(new ScatterPoint(vertices.First().X, vertices.First().Y));

                var lines = new OxyPlot.Series.LineSeries { MarkerType = MarkerType.Circle };
                lines.Points.AddRange(vertices.Select(item => new DataPoint(item.X, item.Y)));
                SetViewRegion(model, vertices);

                model.Series.Add(lines);
                model.Series.Add(point);
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
            var visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(PolygonViewer));
            visualizerHost.ShowVisualizer();
        }

        static void SetViewRegion(PlotModel model, IEnumerable<Vector2d> vertices)
        {
            var vMin = new Vector2d(vertices.Min(item => item.X), vertices.Min(item => item.Y));
            var vMax = new Vector2d(vertices.Max(item => item.X), vertices.Max(item => item.Y));

            double margin = 1;
            if (vertices.Count() > 1)
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
