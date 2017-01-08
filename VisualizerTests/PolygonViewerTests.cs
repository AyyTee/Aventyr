using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CustomDebugVisualizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTK;
using Game;
using Game.Common;
using Game.Physics;
using Game.Portals;

namespace VisualizerTests
{
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PolygonViewerTests
    {
        /// <summary>
        /// This is less of a unit test and more of a way to quickly preview the polygon visualizer.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void ShowVisualizerTest()
        {
            var vectors = new List<List<Vector2d>>
            {
                new List<Vector2d> {
                    new Vector2d(),
                    new Vector2d(1, 0),
                    new Vector2d(0, 2)
                },
                new List<Vector2d> {
                    new Vector2d(2, 2),
                    new Vector2d(3, 2),
                    new Vector2d(2, 4)
                }
            };

            PolygonViewer.TestShowVisualizer(vectors);
        }

        [TestMethod]
        public void Vector2ArrayTest()
        {
            var vectors = new[]
            {
                new Vector2(),
                new Vector2(1, 0),
                new Vector2(0, 2)
            };

            Assert.IsTrue(PolygonViewer.GetGrid(vectors) != null);
        }

        [TestMethod]
        public void Vector2dArrayTest()
        {
            var vectors = new[]
            {
                new Vector2d(),
                new Vector2d(1, 0),
                new Vector2d(0, 2)
            };

            Assert.IsTrue(PolygonViewer.GetGrid(vectors) != null);
        }

        [TestMethod]
        public void Vector2ListTest()
        {
            var vectors = new List<Vector2>
            {
                new Vector2(),
                new Vector2(1, 0),
                new Vector2(0, 2)
            };

            Assert.IsTrue(PolygonViewer.GetGrid(vectors) != null);
        }

        [TestMethod]
        public void Vector2dListTest()
        {
            var vectors = new List<Vector2d>
            {
                new Vector2d(),
                new Vector2d(1, 0),
                new Vector2d(0, 2)
            };

            Assert.IsTrue(PolygonViewer.GetGrid(vectors) != null);
        }

        [TestMethod]
        public void Vector2dListListTest()
        {
            var vectors = new List<List<Vector2d>>
            {
                new List<Vector2d> {
                    new Vector2d(),
                    new Vector2d(1, 0),
                    new Vector2d(0, 2)
                },
                new List<Vector2d> {
                    new Vector2d(2, 2),
                    new Vector2d(3, 2),
                    new Vector2d(2, 4)
                }
            };

            Assert.IsTrue(PolygonViewer.GetGrid(vectors) != null);
        }

        [TestMethod]
        public void Vector2ListListTest()
        {
            var vectors = new List<List<Vector2>>
            {
                new List<Vector2> {
                    new Vector2(),
                    new Vector2(1, 0),
                    new Vector2(0, 2)
                },
                new List<Vector2> {
                    new Vector2(2, 2),
                    new Vector2(3, 2),
                    new Vector2(2, 4)
                }
            };

            Assert.IsTrue(PolygonViewer.GetGrid(vectors) != null);
        }

        [TestMethod]
        public void Vector2dListArrayTest()
        {
            var vectors = new[]
            {
                new List<Vector2d> {
                    new Vector2d(),
                    new Vector2d(1, 0),
                    new Vector2d(0, 2)
                },
                new List<Vector2d> {
                    new Vector2d(2, 2),
                    new Vector2d(3, 2),
                    new Vector2d(2, 4)
                }
            };

            Assert.IsTrue(PolygonViewer.GetGrid(vectors) != null);
        }

        [TestMethod]
        public void Vector2ListArrayTest()
        {
            var vectors = new[]
            {
                new List<Vector2> {
                    new Vector2(),
                    new Vector2(1, 0),
                    new Vector2(0, 2)
                },
                new List<Vector2> {
                    new Vector2(2, 2),
                    new Vector2(3, 2),
                    new Vector2(2, 4)
                }
            };

            Assert.IsTrue(PolygonViewer.GetGrid(vectors) != null);
        }

        [TestMethod]
        public void SceneTest()
        {
            var scene = new Scene();
            new FloatPortal(scene);
            //new Actor(scene, PolygonFactory.CreateNGon(5, 2, new Vector2()));
            PortalCommon.UpdateWorldTransform(scene);
            Assert.IsTrue(SceneViewer.GetGrid(scene) != null);

            SceneViewer.TestShowVisualizer(scene);
        }

        [TestMethod]
        public void InvalidTypeTest()
        {
            Assert.IsTrue(PolygonViewer.GetGrid(new object()) == null);
        }
    }
}
