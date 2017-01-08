using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using Game;
using Game.Portals;

#region Target datatypes
[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(SceneViewer),
typeof(VisualizerSceneSource),
Target = typeof(Scene),
Description = SceneViewer.Description)]
#endregion
namespace CustomDebugVisualizer
{
    public class SceneViewer : DialogDebuggerVisualizer
    {
        public const string Description = "Scene Viewer";

        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            if (windowService == null)
                throw new ArgumentNullException(nameof(windowService));
            if (objectProvider == null)
                throw new ArgumentNullException(nameof(objectProvider));

            object data = Serializer.Deserialize(objectProvider.GetData());
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

            //// Create a thread
            //Thread newWindowThread = new Thread(new ThreadStart(() =>
            //{
            //    // Create and show the Window
            //    var window = new System.Windows.Window
            //    {
            //        Width = 500,
            //        Height = 510,
            //        //Content = grid
            //    };


            //    window.Show();
            //    // Start the Dispatcher Processing
            //    System.Windows.Threading.Dispatcher.Run();
            //}));
            //// Set the apartment state
            //newWindowThread.SetApartmentState(ApartmentState.STA);
            //// Make the thread a background thread
            //newWindowThread.IsBackground = true;
            //// Start the thread
            //newWindowThread.Start();
        }

        static ICollection<List<Vector2d>> CastData(object data)
        {
            var vertices = new List<List<Vector2d>>();

            var scene = data as Scene;
            if (scene != null)
            {
                PortalCommon.UpdateWorldTransform(scene);
                foreach (ISceneObject item in scene.GetAll())
                {
                    var wall = item as IWall;
                    var portal = item as IPortal;
                    if (wall != null)
                    {
                        vertices.Add(wall.GetWorldVertices().Select(v => (Vector2d)v).ToList());
                    }
                    else if (portal != null)
                    {
                        vertices.Add(Portal.GetWorldVerts(portal).Select(v => (Vector2d)v).ToList());
                    }
                }
                return vertices;
            }
            return null;
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
            var visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(SceneViewer), typeof(VisualizerSceneSource));
            visualizerHost.ShowVisualizer();
        }

        static void SetViewRegion(PlotModel model, IEnumerable<IEnumerable<Vector2d>> vertices)
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
