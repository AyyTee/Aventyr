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

        static ICollection<Series> CastData(object data)
        {
            var vertices = new List<Series>();

            var scene = data as Scene;
            if (scene != null)
            {
                PortalCommon.UpdateWorldTransform(scene, true);
                foreach (ISceneObject item in scene.GetAll())
                {
                    var wall = item as IWall;
                    var portal = item as IPortal;
                    if (wall != null)
                    {
                        vertices.Add(new Series
                        {
                            Vertices = wall.GetWorldVertices().Select(v => (Vector2d)v).ToList(),
                            Name = item.Name
                        });
                    }
                    else if (portal != null)
                    {
                        vertices.Add(new Series
                        {
                            Vertices = portal.GetWorldVerts().Select(v => (Vector2d)v).ToList(),
                            Name = item.Name
                        });
                    }
                }
                return vertices;
            }
            return null;
        }

        class Series
        {
            public List<Vector2d> Vertices;
            public string Name;
        }

        public static Grid GetGrid(object data)
        {
            ICollection<Series> vertices = CastData(data);
            if (vertices == null)
            {
                return null;
            }

            var grid = new Grid();

            if (vertices.Any(item => item.Vertices.Any()))
            {
                var model = new PlotModel();
                grid.Children.Add(new PlotView { Model = model });

                foreach (Series list in vertices)
                {
                    var point = new OxyPlot.Series.ScatterSeries { MarkerType = MarkerType.Circle };
                    point.Points.Add(new ScatterPoint(list.Vertices.First().X, list.Vertices.First().Y));

                    var lines = new OxyPlot.Series.LineSeries
                    {
                        MarkerType = MarkerType.Circle,
                        Title = list.Name
                    };
                    lines.Points.AddRange(list.Vertices.Select(item => new DataPoint(item.X, item.Y)));
                    

                    model.Series.Add(lines);
                    model.Series.Add(point);
                }

                PolygonViewer.SetViewRegion(model, vertices.Select(item => item.Vertices));
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
    }
}
